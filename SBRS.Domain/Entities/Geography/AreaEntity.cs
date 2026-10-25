using SBRS.Domain.Common;
using SBRS.Domain.Exceptions;



    public class AreaEntity :LookupEntity
    {
        public Guid DistrictId { get; private set; }
        public Guid CityId { get; private set; }

        private AreaEntity() { }
        public AreaEntity(Guid districtId, Guid cityId, string arName, string enName, string code)
            : base(arName, enName, code)
        {
            if (cityId == Guid.Empty) throw new DomainException("City is required.");
            DistrictId = districtId;
            CityId = cityId;
        }
    }
