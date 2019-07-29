using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using scribercontext.Model;

namespace scribercontext.DAL
{
    public class VideoRepository : IVideoRepository, IDisposable
    {
        private scriberContext context;

        public VideoRepository(scriberContext context)
        {
            this.context = context;
        }

        public IEnumerable<Video> GetVideos()
        {
            return context.Video.ToList();
        }

        public Video GetVideoByID(int id)
        {
            return context.Video.Find(id);
        }

        public void InsertVideo(Video video)
        {
            context.Video.Add(video);
        }

        public void DeleteVideo(int videoId)
        {
            Video video = context.Video.Find(videoId);
            context.Video.Remove(video);
        }

        public void UpdateVideo(Video video)
        {
            context.Entry(video).State = EntityState.Modified;
        }

        public void Save()
        {
            context.SaveChanges();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        IEnumerable<Video> IVideoRepository.GetVideos()
        {
            throw new NotImplementedException();
        }

        Video IVideoRepository.GetVideoByID(int VideoId)
        {
            throw new NotImplementedException();
        }

        void IVideoRepository.InsertVideo(Video video)
        {
            throw new NotImplementedException();
        }

        void IVideoRepository.DeleteVideo(int VideoId)
        {
            throw new NotImplementedException();
        }

        void IVideoRepository.UpdateVideo(Video video)
        {
            throw new NotImplementedException();
        }

        void IVideoRepository.Save()
        {
            throw new NotImplementedException();
        }

        Video IVideoRepository.GetVideoById(int id)
        {
            throw new NotImplementedException();
        }

        void IDisposable.Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
