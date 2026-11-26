param(
    [ValidateSet('full', 'refresh')]
    [string]$Mode = 'refresh',
    [string]$Root = ''
)

$ErrorActionPreference = 'Stop'
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
if ([string]::IsNullOrWhiteSpace($Root)) {
    $Root = (Resolve-Path (Join-Path $scriptDir '..\..\..\..')).Path
}
$Root = (Resolve-Path $Root).Path
$OutDir = Join-Path $Root '.opencode\project-map'
$StatePath = Join-Path $OutDir 'state.json'
if (-not (Test-Path $OutDir)) { New-Item -ItemType Directory -Path $OutDir -Force | Out-Null }

$SkipDirs = @('bin', 'obj', 'Migrations', 'node_modules', '.git', 'docs', '.vs', 'Local Assets', 'TestResults', 'project-map')
$ScanExt = @('.cs', '.csproj', '.json', '.slnx')

function Get-Rel($full) { $full.Substring($Root.Length + 1).Replace('\', '/') }

function Write-Text($path, $text) {
    [System.IO.File]::WriteAllText($path, $text, (New-Object System.Text.UTF8Encoding($false)))
}

function Get-Files {
    $all = New-Object System.Collections.Generic.List[string]
    $stack = New-Object System.Collections.Generic.Stack[string]
    $stack.Push($Root)
    while ($stack.Count -gt 0) {
        $dir = $stack.Pop()
        foreach ($d in [System.IO.Directory]::GetDirectories($dir)) {
            $leaf = Split-Path -Leaf $d
            if ($SkipDirs -contains $leaf) { continue }
            if ($leaf.StartsWith('.') -and $leaf -ne '.opencode') { continue }
            if ($leaf -eq '.opencode') { continue }
            $stack.Push($d)
        }
        foreach ($f in [System.IO.Directory]::GetFiles($dir)) {
            $ext = [System.IO.Path]::GetExtension($f).ToLowerInvariant()
            if ($ScanExt -notcontains $ext) { continue }
            if ($ext -eq '.md') { continue }
            $all.Add((Get-Rel $f))
        }
    }
    $all.Sort([StringComparer]::Ordinal)
    return $all
}

function Get-Stamp($rel) {
    $fi = [System.IO.FileInfo](Join-Path $Root $rel)
    return ([long]$fi.LastWriteTimeUtc.Ticks).ToString() + ':' + $fi.Length
}

function Get-Text($rel) {
    if (-not $script:Cache.ContainsKey($rel)) {
        $script:Cache[$rel] = [System.IO.File]::ReadAllText((Join-Path $Root $rel))
    }
    return $script:Cache[$rel]
}
$script:Cache = @{}

function Get-Singular($n) {
    if ($n -match 'ies$') { return $n.Substring(0, $n.Length - 3) + 'y' }
    if ($n -match '(ses|xes|ches|shes)$') { return $n.Substring(0, $n.Length - 2) }
    if ($n -match 's$' -and $n -notmatch 'ss$') { return $n.Substring(0, $n.Length - 1) }
    return $n
}

function Get-Classes($text) {
    $rx = [regex]'(?m)^\s*(?:public|internal)\s+(?:(?:sealed|abstract|static|partial|readonly)\s+)*(?:class|record|interface|enum|struct)\s+(\w+)'
    return @($rx.Matches($text) | ForEach-Object { $_.Groups[1].Value })
}

# ---------- section builders ----------

function Build-Layer {
    $lines = @('# LAYER: project -> referenced project')
    $projs = $script:Files | Where-Object { $_ -like '*.csproj' }
    foreach ($p in $projs) {
        $name = [System.IO.Path]::GetFileNameWithoutExtension($p)
        $text = Get-Text $p
        $refs = [regex]::Matches($text, '<ProjectReference\s+Include="([^"]+)"') | ForEach-Object {
            [System.IO.Path]::GetFileNameWithoutExtension($_.Groups[1].Value.Replace('\', '/'))
        }
        if ($refs.Count -eq 0) { $lines += "$name -> (none)" }
        foreach ($r in $refs) { $lines += "$name -> $r" }
    }
    return ($lines -join "`n")
}

function Build-Features {
    $lines = @('# FEATURES: files grouped per feature (paths relative to repo)')
    $featureRoot = 'SBRS.Application/Features/'
    $featDirs = @{}
    foreach ($f in $script:Files) {
        if (-not $f.StartsWith($featureRoot)) { continue }
        $rest = $f.Substring($featureRoot.Length)
        $idx = $rest.IndexOf('/')
        if ($idx -lt 0) { continue }
        $name = $rest.Substring(0, $idx)
        if (-not $featDirs.ContainsKey($name)) { $featDirs[$name] = New-Object System.Collections.Generic.List[string] }
        $featDirs[$name].Add($f)
    }
    $repoIfaces = @{}
    $repoImpls = @{}
    foreach ($f in $script:Files) {
        if ($f -match '^SBRS\.Domain/Interfaces/I(\w+)Repository\.cs$') { $repoIfaces[$Matches[1]] = $f }
        if ($f -match '^SBRS\.Infrastructure/Persistence/Repositories/(\w+)Repository\.cs$') { $repoImpls[$Matches[1]] = $f }
    }
    $controllers = @($script:Files | Where-Object { $_ -like 'SBRS.API/Controllers/*Controller.cs' })

    foreach ($name in ($featDirs.Keys | Sort-Object)) {
        $sing = Get-Singular $name
        $files = $featDirs[$name]
        $groups = @{}
        foreach ($f in $files) {
            $rel = $f.Substring(($featureRoot + $name + '/').Length)
            $parts = $rel.Split('/')
            $g = if ($parts.Count -gt 1) { $parts[0] } else { 'root' }
            if (-not $groups.ContainsKey($g)) { $groups[$g] = New-Object System.Collections.Generic.List[string] }
            $groups[$g].Add($parts[$parts.Count - 1])
        }
        $validators = @($files | Where-Object { (Get-Text $_) -match 'class\s+\w*Validator\b' } | ForEach-Object { [System.IO.Path]::GetFileName($_) })

        $ctrl = ''
        foreach ($c in $controllers) {
            if ($c -match "/($name|$($sing))Controller\.cs$") { $ctrl = $c; break }
        }
        if (-not $ctrl) {
            foreach ($c in $controllers) {
                if ((Get-Text $c) -match [regex]::Escape("Features.$name.")) { $ctrl = $c; break }
            }
        }
        $contractDir = "SBRS.API/Contracts/$name"
        $contracts = @($script:Files | Where-Object { $_ -like "$contractDir/*" })
        if ($contracts.Count -eq 0 -and $ctrl) {
            $ct = Get-Text $ctrl
            if ($ct -match "using\s+SBRS\.Contracts\.$name;") {
                $contracts = @($script:Files | Where-Object { $_ -like "$contractDir/*" })
            }
        }
        $repoParts = @()
        foreach ($cand in @($name, $sing)) {
            if ($repoIfaces.ContainsKey($cand)) { $repoParts += $repoIfaces[$cand] }
            if ($repoImpls.ContainsKey($cand)) { $repoParts += $repoImpls[$cand] }
        }
        $repoParts = @($repoParts | Select-Object -Unique)

        $seg = @()
        if ($ctrl) { $seg += "ctrl[$ctrl]" }
        if ($contracts.Count -gt 0) { $seg += "contracts[$contractDir]:" + (($contracts | ForEach-Object { [System.IO.Path]::GetFileName($_) }) -join ',') }
        foreach ($g in @('Commands', 'Queries', 'DTOs', 'Mappings', 'Services', 'root')) {
            if ($groups.ContainsKey($g)) {
                $tag = switch ($g) { 'Commands' { 'cmd' } 'Queries' { 'qry' } 'DTOs' { 'dto' } 'Mappings' { 'map' } 'Services' { 'svc' } default { 'src' } }
                $seg += "${tag}[" + $featureRoot + $name + '/' + $g + ']:' + (($groups[$g] | Sort-Object) -join ',')
            }
        }
        if ($validators.Count -gt 0) { $seg += 'validators:' + ($validators -join ',') }
        if ($repoParts.Count -gt 0) { $seg += 'repo:' + ($repoParts -join ',') }
        $lines += "$name | " + ($seg -join ' | ')
    }
    return ($lines -join "`n")
}

function Build-Flows {
    $lines = @('# FLOWS: controller -> operation -> handler -> repo interface -> repo impl -> entity')
    $controllers = @($script:Files | Where-Object { $_ -like 'SBRS.API/Controllers/*Controller.cs' })
    $ops = @{}
    foreach ($f in $script:Files) {
        if ($f -notmatch '^SBRS\.Application/Features/(\w+)/(Commands|Queries)/(\w+)\.cs$') { continue }
        $feat = $Matches[1]; $kind = $Matches[2]
        $text = Get-Text $f
        $opNames = [regex]::Matches($text, '(?m)^\s*public\s+(?:sealed\s+)?record\s+(\w+(?:Command|Query))\b') | ForEach-Object { $_.Groups[1].Value }
        foreach ($op in $opNames) {
            if (-not $ops.ContainsKey($op)) { $ops[$op] = @{ feat = $feat; kind = $kind; file = $f; text = $text } }
        }
    }
    $handlerRepo = @{}
    foreach ($op in $ops.Keys) {
        $text = $ops[$op].text
        $h = [regex]::Match($text, "class\s+$([regex]::Escape($op))Handler\s*\(([^)]*)\)")
        $repos = @()
        if ($h.Success) { $repos = @([regex]::Matches($h.Groups[1].Value, 'I\w+Repository') | ForEach-Object { $_.Value }) }
        if ($repos.Count -eq 0) { $repos = @([regex]::Matches($text, 'I\w+Repository') | ForEach-Object { $_.Value } | Select-Object -Unique) }
        $handlerRepo[$op] = @($repos | Select-Object -Unique)
    }
    $repoImplOf = @{}
    $repoEntity = @{}
    foreach ($f in $script:Files) {
        if ($f -match '^SBRS\.Infrastructure/Persistence/Repositories/(\w+)Repository\.cs$') {
            $n = $Matches[1]
            $repoImplOf[$n] = $f
            $t = Get-Text $f
            $e = [regex]::Match($t, '\b(\w+Entity)\b')
            if ($e.Success) { $repoEntity[$n] = $e.Groups[1].Value }
        }
    }
    $seen = @{}
    foreach ($c in $controllers) {
        $ctext = Get-Text $c
        $cname = [System.IO.Path]::GetFileNameWithoutExtension($c)
        $lines2 = $ctext -split "`r?`n"
        $action = ''
        foreach ($ln in $lines2) {
            if ($ln -match 'public\s+(?:async\s+)?[\w<>,\[\]\?\. ]+\s+(\w+)\s*\(') { $action = $Matches[1] }
            foreach ($m in [regex]::Matches($ln, 'new\s+(\w+(?:Command|Query))\s*\(')) {
                $op = $m.Groups[1].Value
                if (-not $ops.ContainsKey($op)) { continue }
                $key = "$c.$action.$op"
                if ($seen.ContainsKey($key)) { continue }
                $seen[$key] = 1
                $chain = @("$cname.$action", $op, "${op}Handler")
                foreach ($ri in $handlerRepo[$op]) {
                    $chain += $ri
                    $base = $ri.TrimStart('I'); $base = $base -replace 'Repository$', ''
                    if ($repoImplOf.ContainsKey($base)) { $chain += [System.IO.Path]::GetFileName($repoImplOf[$base]) }
                    if ($repoEntity.ContainsKey($base)) { $chain += $repoEntity[$base] }
                }
                $lines += ($ops[$op].feat + ' | ' + ($chain -join ' -> '))
            }
        }
    }
    return ($lines -join "`n")
}

function Build-Entities {
    $lines = @('# ENTITIES: entity | base | FK props | EF config')
    $cfgOf = @{}
    foreach ($f in $script:Files) {
        if ($f -notlike 'SBRS.Infrastructure/Persistence/Configurations/*') { continue }
        $t = Get-Text $f
        foreach ($m in [regex]::Matches($t, 'IEntityTypeConfiguration<(\w+)>')) {
            $e = $m.Groups[1].Value
            if (-not $cfgOf.ContainsKey($e)) { $cfgOf[$e] = $f }
        }
    }
    foreach ($f in $script:Files) {
        if ($f -notlike 'SBRS.Domain/Entities/*.cs') { continue }
        $t = Get-Text $f
        $decl = [regex]::Match($t, 'class\s+(\w+)\s*:\s*(\w+)')
        if (-not $decl.Success) { continue }
        $cls = $decl.Groups[1].Value; $base = $decl.Groups[2].Value
        $fks = @([regex]::Matches($t, 'public\s+Guid\??\s+(\w+Id)\b') | ForEach-Object { $_.Groups[1].Value } | Select-Object -Unique)
        $cfg = ''
        if ($cfgOf.ContainsKey($cls)) { $cfg = $cfgOf[$cls] }
        elseif ($cfgOf.ContainsKey($base)) { $cfg = $cfgOf[$base] }
        $lines += "$cls : $base | fk:" + ($(if ($fks.Count) { $fks -join ',' } else { '-' })) + ' | cfg:' + $(if ($cfg) { $cfg } else { '-' }) + " | $f"
    }
    return ($lines -join "`n")
}

function Build-Endpoints {
    $lines = @('# ENDPOINTS: VERB /route -> action | authorization (fallback = authenticated user)')
    foreach ($f in $script:Files) {
        if ($f -notlike 'SBRS.API/Controllers/*Controller.cs') { continue }
        $ctrlName = [System.IO.Path]::GetFileNameWithoutExtension($f)
        $token = $ctrlName -replace 'Controller$', ''
        $text = Get-Text $f
        $classLine = ($text -split "`r?`n" | Select-String -Pattern '^\s*public\s+class\s' | Select-Object -First 1).LineNumber
        if (-not $classLine) { $classLine = 1 }
        $head = ($text -split "`r?`n")[0..([Math]::Max(0, $classLine - 2))] -join "`n"
        $baseRoute = 'api/[controller]'
        $br = [regex]::Match($head, '\[Route\("([^"]*)"\)\]')
        if ($br.Success) { $baseRoute = $br.Groups[1].Value }
        $baseRoute = $baseRoute.Replace('[controller]', $token).Trim('/')
        $pendingVerb = ''; $pendingRoute = ''; $pendingAuth = ''
        foreach ($ln in ($text -split "`r?`n")) {
            $m = [regex]::Match($ln, '\[Http(Get|Post|Put|Patch|Delete)(?:\("([^"]*)"\))?\]')
            if ($m.Success) {
                $pendingVerb = $m.Groups[1].Value.ToUpperInvariant()
                if ($m.Groups[2].Success) { $pendingRoute = $m.Groups[2].Value }
            }
            $mr = [regex]::Match($ln, '\[Route\("([^"]*)"\)\]')
            if ($mr.Success) { $pendingRoute = $mr.Groups[1].Value }
            if ($ln -match '\[AllowAnonymous\]') { $pendingAuth = 'anonymous' }
            $ma = [regex]::Match($ln, '\[Authorize\(Policy\s*=\s*(?:Policies\.)?(\w+)\)\]')
            if ($ma.Success) { $pendingAuth = 'policy:' + $ma.Groups[1].Value }
            $mar = [regex]::Match($ln, '\[Authorize\(Roles\s*=\s*([^\)]*)\)\]')
            if ($mar.Success) { $pendingAuth = 'roles:' + $mar.Groups[1].Value.Trim() }
            if ($ln -match '^\s*\[Authorize\]') { if (-not $pendingAuth) { $pendingAuth = 'authenticated' } }
            $mm = [regex]::Match($ln, 'public\s+(?:async\s+)?[\w<>,\[\]\?\. ]+\s+(\w+)\s*\(')
            if ($mm.Success -and $pendingVerb) {
                $route = $pendingRoute
                if ($route.StartsWith('~/')) { $route = $route.Substring(2) }
                if ($route.StartsWith('/')) { $route = $route.TrimStart('/') }
                elseif ($route) { $route = $baseRoute + '/' + $route.Trim('/') }
                else { $route = $baseRoute }
                $auth = if ($pendingAuth) { $pendingAuth } else { 'authenticated' }
                $lines += "$pendingVerb /$route -> $ctrlName.$($mm.Groups[1].Value) | $auth"
                $pendingVerb = ''; $pendingRoute = ''; $pendingAuth = ''
            }
        }
    }
    return ($lines -join "`n")
}

function Build-Auth {
    $lines = @('# AUTHORIZATION: policies, roles')
    foreach ($f in $script:Files) {
        if ($f -notlike 'SBRS.Domain/Constants/*.cs' -and $f -notlike 'SBRS.API/Authorization/*.cs') { continue }
        $t = Get-Text $f
        $consts = [regex]::Matches($t, 'const\s+string\s+(\w+)\s*=\s*"([^"]*)"')
        if ($consts.Count -eq 0) { continue }
        $lines += "[$f]"
        foreach ($c in $consts) { $lines += '  ' + $c.Groups[1].Value + ' = "' + $c.Groups[2].Value + '"' }
        foreach ($p in [regex]::Matches($t, 'AddPolicy\(\s*([\w\.]+)\s*,([\s\S]*?)\);')) {
            $roles = [regex]::Matches($p.Groups[2].Value, 'Roles\.(\w+)') | ForEach-Object { $_.Groups[1].Value }
            if ($roles.Count -gt 0) { $lines += '  policy ' + $p.Groups[1].Value + ' -> roles: ' + ($roles -join ',') }
        }
    }
    return ($lines -join "`n")
}

function Build-Di {
    $lines = @('# DI / CONFIG ENTRY POINTS (key names only, no values)')
    foreach ($f in $script:Files) {
        if ($f -match '(^|/)DependencyInjection\.cs$' -or $f -match 'AppDbContext\.cs$' -or $f -like 'SBRS.Infrastructure/Persistence/Seed/*.cs') {
            $lines += "[$f]"
            $t = Get-Text $f
            $classes = Get-Classes $t
            if ($classes.Count -gt 0) { $lines += '  ' + ($classes -join ', ') }
            $methods = @([regex]::Matches($t, 'public\s+static\s+[\w<>,\[\]\?\. ]+\s+(\w+)\s*\(') | ForEach-Object { $_.Groups[1].Value } | Select-Object -Unique)
            if ($methods.Count -gt 0) { $lines += '  entry: ' + ($methods -join ', ') }
        }
    }
    foreach ($f in $script:Files) {
        if ($f -notlike 'SBRS.API/appsettings*.json') { continue }
        $lines += "[$f] keys:"
        try {
            $json = Get-Text $f | ConvertFrom-Json
            $keys = New-Object System.Collections.Generic.List[string]
            function Walk($obj, $prefix) {
                if ($null -eq $obj) { return }
                if ($obj -is [System.Management.Automation.PSCustomObject]) {
                    foreach ($p in $obj.PSObject.Properties) {
                        $k = if ($prefix) { "${prefix}:$($p.Name)" } else { $p.Name }
                        if ($p.Value -is [System.Management.Automation.PSCustomObject]) { Walk $p.Value $k } else { $keys.Add($k) }
                    }
                }
            }
            Walk $json ''
            $lines += '  ' + ($keys -join ', ')
        } catch { $lines += '  (unparsed)' }
    }
    return ($lines -join "`n")
}

function Build-Xref {
    $lines = @('# XREF: file -> classes (Domain, Application, Infrastructure)')
    foreach ($f in $script:Files) {
        if ($f -notmatch '^SBRS\.(Domain|Application|Infrastructure)/') { continue }
        if ($f -notlike '*.cs') { continue }
        $classes = @(Get-Classes (Get-Text $f) | Select-Object -Unique)
        if ($classes.Count -gt 0) { $lines += "$f : " + ($classes -join ', ') }
    }
    return ($lines -join "`n")
}

# ---------- run ----------

$script:Files = Get-Files
$current = @{}
foreach ($f in $script:Files) { $current[$f] = Get-Stamp $f }

$old = @{}
if ($Mode -eq 'refresh' -and (Test-Path $StatePath)) {
    try {
        $state = Get-Content $StatePath -Raw | ConvertFrom-Json
        foreach ($p in $state.files.PSObject.Properties) { $old[$p.Name] = [string]$p.Value }
    } catch { $old = @{} }
}

$changed = New-Object System.Collections.Generic.List[string]
if ($Mode -eq 'full' -or $old.Count -eq 0) {
    $changed.AddRange([string[]]$script:Files)
} else {
    foreach ($f in $script:Files) {
        if (-not $old.ContainsKey($f) -or $old[$f] -ne $current[$f]) { $changed.Add($f) }
    }
    foreach ($k in $old.Keys) { if (-not $current.ContainsKey($k)) { $changed.Add($k) } }
}

function Test-Dirty($dirs, $exact) {
    foreach ($c in $changed) {
        foreach ($e in $exact) { if ($c -eq $e) { return $true } }
        foreach ($d in $dirs) { if ($c.StartsWith($d)) { return $true } }
    }
    return $false
}

$csproj = @($script:Files | Where-Object { $_ -like '*.csproj' })
$appDirs = @('SBRS.Application/Features/', 'SBRS.API/Contracts/', 'SBRS.API/Controllers/', 'SBRS.Domain/Interfaces/', 'SBRS.Infrastructure/Persistence/Repositories/')
$sections = @(
    @{ file = 'layer.txt'; build = 'Build-Layer'; dirs = @(); exact = $csproj },
    @{ file = 'features.txt'; build = 'Build-Features'; dirs = $appDirs; exact = @() },
    @{ file = 'flows.txt'; build = 'Build-Flows'; dirs = @($appDirs + 'SBRS.Domain/Entities/'); exact = @() },
    @{ file = 'entities.txt'; build = 'Build-Entities'; dirs = @('SBRS.Domain/Entities/', 'SBRS.Infrastructure/Persistence/Configurations/'); exact = @() },
    @{ file = 'endpoints.txt'; build = 'Build-Endpoints'; dirs = @('SBRS.API/Controllers/'); exact = @() },
    @{ file = 'auth-di.txt'; build = 'Build-Auth'; dirs = @('SBRS.Domain/Constants/', 'SBRS.API/Authorization/'); exact = @() },
    @{ file = 'xref.txt'; build = 'Build-Xref'; dirs = @('SBRS.Domain/', 'SBRS.Application/', 'SBRS.Infrastructure/'); exact = @() }
)
$diSection = @{ file = 'di.txt'; build = 'Build-Di'; dirs = @('SBRS.Infrastructure/Persistence/Seed/'); exact = @($script:Files | Where-Object { $_ -match '(^|/)DependencyInjection\.cs$' -or $_ -match 'AppDbContext\.cs$' -or $_ -like 'SBRS.API/appsettings*.json' }) }

$rebuilt = 0
foreach ($s in ($sections + $diSection)) {
    $path = Join-Path $OutDir $s.file
    $dirty = ($Mode -eq 'full') -or (-not (Test-Path $path)) -or (Test-Dirty $s.dirs $s.exact)
    if ($dirty) {
        $content = & $s.build
        Write-Text $path $content
        $rebuilt++
    }
}

# index always regenerated from current files on disk
$idx = @()
$idx += '# PROJECT MAP (generated by .opencode/skills/token-saver/scripts/generate-map.ps1)'
$idx += '# Read this first, then only the section you need. Do not read whole files/dirs.'
$idx += '# Layers, features:'
$idx += (Get-Content (Join-Path $OutDir 'layer.txt') -Raw).TrimEnd()
$idx += ''
$idx += (Get-Content (Join-Path $OutDir 'features.txt') -Raw).TrimEnd()
$idx += ''
$idx += '# MORE: flows.txt (controller->handler->repo->entity) | entities.txt (entity/base/FKs/config) | endpoints.txt (verb/route/action/auth) | di.txt (DI, DbContext, seeders, appsettings keys) | auth-di.txt (policies, roles) | xref.txt (class -> file)'
$idx += "# FILES: $($script:Files.Count) source files scanned. Generated: $((Get-Date).ToString('s'))"
Write-Text (Join-Path $OutDir 'index.txt') ($idx -join "`n")

$stateObj = [ordered]@{ generated = (Get-Date).ToString('s'); files = $current }
Write-Text $StatePath ($stateObj | ConvertTo-Json -Depth 5)

$size = 0
foreach ($f in (Get-ChildItem $OutDir -File | Where-Object { $_.Name -ne 'state.json' })) { $size += $f.Length }
$idxSize = (Get-Item (Join-Path $OutDir 'index.txt')).Length
Write-Output ("map updated: {0} files changed, {1} sections rebuilt, index {2} B, map {3} B" -f $changed.Count, $rebuilt, $idxSize, $size)
