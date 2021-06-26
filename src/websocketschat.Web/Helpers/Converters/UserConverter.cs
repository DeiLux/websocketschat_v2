using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DbModels = websocketschat.Database.Models;
using CoreModels = websocketschat.Core.Models;
using websocketschat.Datatransfer.HttpDto;

namespace websocketschat.Web.Helpers.Converters
{
    public class UserConverter : Profile
    {
        public UserConverter()
        {
            CreateMap<PostUserDto, CoreModels.User>().ReverseMap();
            CreateMap<GetUserDto, CoreModels.User>().ReverseMap();

            CreateMap<DbModels.User, CoreModels.User>().ReverseMap();
        }
    }
}
