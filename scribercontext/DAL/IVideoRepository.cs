using System;
using System.Collections.Generic;
using scribercontext.Model;

namespace scribercontext.DAL
{
    public interface IVideoRepository : IDisposable
    {
        IEnumerable<Video> GetVideos();
        Video GetVideoByID(int VideoId);
        void InsertVideo(Video video);
        void DeleteVideo(int VideoId);
        void UpdateVideo(Video video);
        void Save();
        Video GetVideoById(int id);
    }
}
