using AutoMapper;

using LoginDomain.Dtos;
using LoginDomain.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace LoginDomain.Helpers
{
   public class AutoMapperProfile:Profile
    {
        public AutoMapperProfile ()
        {
            CreateMap<Users, UserDto>();
            CreateMap<UserDto, Users>();
        }
    }
}
