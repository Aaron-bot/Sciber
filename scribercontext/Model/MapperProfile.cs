using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;

namespace scribercontext.Model
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<Video, VideoDTO>();
            CreateMap<VideoDTO, Video>();

        }
    }
}
