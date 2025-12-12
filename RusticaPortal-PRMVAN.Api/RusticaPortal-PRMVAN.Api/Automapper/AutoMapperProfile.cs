using AutoMapper;
using ArellanoCore.Api.Entities.Dto.Information;
using ArellanoCore.Api.Entities.Information;

namespace ArellanoCore.Api.Automapper
{
    public class AutoMapperProfile:  Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<AditionalInfomation, AditionInformationDTO>();
        }
    }
}
