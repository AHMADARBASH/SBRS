using SBRS.Domain.Common;




    public class AreaEntity :LookupEntity
    {
        public Guid DistrictId { get; private set; }

        private AreaEntity() { }
        public AreaEntity(Guid districtId, string arName, string enName, string code)
            : base(arName, enName, code) => DistrictId = districtId;
    }

