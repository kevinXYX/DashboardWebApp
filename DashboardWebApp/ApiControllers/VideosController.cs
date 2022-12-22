using DashboardWebApp.Models;
using DashboardWebApp.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;

namespace DashboardWebApp.ApiControllers
{
    [Authorize]
    public class VideosController : ControllerBase
    {
        private readonly IVideoService videoService;
        public VideosController(IVideoService videoService)
        {
            this.videoService = videoService;
        }

        [HttpGet("api/videos/getstream")]
        public async Task<IActionResult> GetStream(string url)
        {
            MemoryStream streamCache = null;

            var file = url;

            var streamBytes = HttpContext.Session.Get(file);

            if (streamBytes != null)
            {
                streamCache = new MemoryStream(streamBytes);
            }

            if (streamCache == null)
            {
                var client = new HttpClient() { Timeout = TimeSpan.FromMinutes(9999) };
                byte[] fileBytes = await client.GetByteArrayAsync(file);
                var newStream = new MemoryStream(fileBytes);
                HttpContext.Session.Set(file, fileBytes);
                streamCache = newStream;
            }

            return File(streamCache, new MediaTypeHeaderValue("video/mp4").MediaType, true);
        }

        [HttpPost("api/videos/addvideocomment")]
        public IActionResult AddVideoComment(VideoPostContent videoPostContent)
        {
            return Ok(this.videoService.AddVideoComment(videoPostContent.BookId, videoPostContent.Content));
        }

        [HttpPost("api/videos/addvideolabel")]
        public IActionResult AddVideoLabel(VideoPostContent videoPostContent)
        {
            return Ok(this.videoService.AddVideoLabel(videoPostContent.BookId, videoPostContent.Content));
        }
    }

    public class VideoPostContent
    {
        public int BookId { get; set; }
        public string Content { get; set; }
    }
}
