using AutoMapper;
using VibPortalApi.Models.DB2Models;

namespace VibPortalApi.Profiles
{
    public class EuravibProfile : Profile
    {
        public EuravibProfile()
        {
            CreateMap<EuravibImportDto, EuravibImport>();
        }
    }
}
