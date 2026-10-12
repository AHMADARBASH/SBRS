using SBRS.Domain.Common;



    public class DistrictEntity: LookupEntity
    {
        public Guid RegionId { get; private set; }

        private DistrictEntity() { }
        public DistrictEntity(Guid regionId, string arName, string enName, string code)
            : base(arName, enName, code) => RegionId = regionId;
    }

