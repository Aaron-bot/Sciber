using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using scribercontext.Helper;
using scribercontext.DAL;
using scribercontext.Model;
using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;

namespace scribercontext.Controllers
{
    public class URLDTO
    {
        public String URL { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class VideosController : ControllerBase
    {
        private readonly scriberContext _context;
        private IVideoRepository videoRepository;
        private readonly IMapper _mapper;
        private char searchString;

        public object videos { get; private set; }

        public VideosController(scriberContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
            this.videoRepository = new VideoRepository(new scriberContext());
        }

        // GET: api/Videos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Video>>> GetVideo()
        {
            return await _context.Video.ToListAsync();
        }

        // GET: api/Videos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Video>> GetVideo(int id)
        {
            var video = await _context.Video.FindAsync(id);

            if (video == null)
            {
                return NotFound();
            }

            return video;
        }

        // PUT: api/Videos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVideo(int id, Video video)
        {
            if (id != video.VideoId)
            {
                return BadRequest();
            }

            _context.Entry(video).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VideoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Videos
        [HttpPost]
        public async Task<ActionResult<Video>> PostVideo([FromBody]URLDTO data)
        {
            Video video;
            String videoURL;
            String videoId;
            try
            {
                videoURL = data.URL;
                videoId = YouTubeHelper.GetVideoIdFromURL(videoURL);
                video = YouTubeHelper.GetVideoInfo(videoId);
            } catch
            {
                return BadRequest("Invalid YouTube URL");
            }
            _context.Video.Add(video);
            await _context.SaveChangesAsync();

            int id = video.VideoId;

            scriberContext tempContext = new scriberContext();
            TranscriptionsController transcriptionsController = new TranscriptionsController(tempContext);

            Task addCaptions = Task.Run(async () =>
            {
                List<Transcription> transcriptions = new List<Transcription>();
                transcriptions = YouTubeHelper.GetTranscriptions(videoId);

                for (int i = 0; i < transcriptions.Count; i++)
                {
                    Transcription transcription = transcriptions.ElementAt(i);
                    transcription.VideoId = id;
                    await transcriptionsController.PostTranscription(transcription);
                }
            });

            return CreatedAtAction("GetVideo", new { id = video.VideoId }, video);
        }

        // DELETE: api/Videos/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Video>> DeleteVideo(int id)
        {
            var video = await _context.Video.FindAsync(id);
            if (video == null)
            {
                return NotFound();
            }

            _context.Video.Remove(video);
            await _context.SaveChangesAsync();

            return video;
        }

        private bool VideoExists(int id)
        {
            return _context.Video.Any(e => e.VideoId == id);
        }

        [HttpGet("SearchByTranscriptions/{searchString}")]
        public async Task<ActionResult<IEnumerable<Video>>> Search(String searchString)
        {
            if (string.IsNullOrEmpty(searchString))
            {
                return BadRequest("Search string cannot be null or empty.");
            }
            var videos = await _context.Video.Include(video => video.Transcription).Select(video => new Video
            {
                VideoId = video.VideoId,
                VideoTitle = video.VideoTitle,
                VideoLength = video.VideoLength,
                WebUrl = video.WebUrl,
                ThunbnailUrl = video.ThunbnailUrl,
                IsFavourite = video.IsFavourite,
                Transcription = video.Transcription.Where(tran => tran.Phrase.Contains(searchString)).ToList()
            }).ToListAsync();

            videos.RemoveAll(video => video.Transcription.Count == 0);
            return Ok(videos);
        }

        [HttpPatch("update/{id}")]
        public VideoDTO Patch(int id, [FromBody]JsonPatchDocument<VideoDTO> videoPatch)
        {
            Video originVideo = videoRepository.GetVideoById(id);
            VideoDTO videoDTO = _mapper.Map<VideoDTO>(originVideo);
            videoPatch.ApplyTo(videoDTO);
            _mapper.Map(videoDTO, originVideo);
            _context.Update(originVideo);
            _context.SaveChanges();
            return videoDTO;
        }

    }
}
