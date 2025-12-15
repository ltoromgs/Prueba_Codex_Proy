using AutoMapper;
using RusticaPortal_PRMVAN.Api.Entities.Dto.Information;
using RusticaPortal_PRMVAN.Api.Entities.Information;

namespace RusticaPortal_PRMVAN.Api.Automapper
{
    public class AutoMapperProfile:  Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<AditionalInfomation, AditionInformationDTO>();
        }
    }
}
