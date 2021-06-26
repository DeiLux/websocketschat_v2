using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DbModels = websocketschat.Database.Models;
using CoreModels = websocketschat.Core.Models;

namespace websocketschat.Web.Helpers.Converters
{
    public class RoleConverter : Profile
    {
        public RoleConverter()
        {
            CreateMap<DbModels.Role, CoreModels.Role>().ReverseMap();
        }
    }
}
