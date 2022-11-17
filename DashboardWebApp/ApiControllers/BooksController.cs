using KendoNET.DynamicLinq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DashboardWebApp.ApiControllers
{
    [Authorize]
    public class BooksController : ControllerBase
    {
        [HttpPost]
        [Route("api/books/getBooks")]
        public DataSourceResult GetBooks([FromBody] DataSourceRequest requestModel)
        {
            var books = new List<Book>();
            books.Add(new Book
            {
                Id = 1,
                UserId = 12,
                TypeId = 3,
                Created = "09/26/2022",
                Isbn = "19943949924",
                FileName = "file1",
                UploadDate = "09/26/2022",
                VideoDuration = 30,
                FileSize = 30094
            });
            books.Add(new Book
            {
                Id = 2,
                UserId = 12,
                TypeId = 3,
                Created = "09/26/2022",
                Isbn = "29943949924",
                FileName = "file1",
                UploadDate = "09/26/2022",
                VideoDuration = 30,
                FileSize = 30094
            });
            books.Add(new Book
            {
                Id = 3,
                UserId = 12,
                TypeId = 3,
                Created = "09/26/2022",
                Isbn = "39943949924",
                FileName = "file1",
                UploadDate = "09/26/2022",
                VideoDuration = 30,
                FileSize = 30094
            });
            books.Add(new Book
            {
                Id = 4,
                UserId = 12,
                TypeId = 3,
                Created = "09/26/2022",
                Isbn = "49943949924",
                FileName = "file1",
                UploadDate = "09/26/2022",
                VideoDuration = 30,
                FileSize = 30094
            });
            books.Add(new Book
            {
                Id = 5,
                UserId = 12,
                TypeId = 3,
                Created = "09/26/2022",
                Isbn = "59943949924",
                FileName = "file1",
                UploadDate = "09/26/2022",
                VideoDuration = 30,
                FileSize = 30094
            });
            books.Add(new Book
            {
                Id = 6,
                UserId = 12,
                TypeId = 3,
                Created = "09/26/2022",
                Isbn = "69943949924",
                FileName = "file1",
                UploadDate = "09/26/2022",
                VideoDuration = 30,
                FileSize = 30094
            });
            books.Add(new Book
            {
                Id = 7,
                UserId = 12,
                TypeId = 3,
                Created = "09/26/2022",
                Isbn = "69943949924",
                FileName = "file1",
                UploadDate = "09/26/2022",
                VideoDuration = 30,
                FileSize = 30094
            });
            books.Add(new Book
            {
                Id = 8,
                UserId = 12,
                TypeId = 3,
                Created = "09/26/2022",
                Isbn = "69943949924",
                FileName = "file1",
                UploadDate = "09/26/2022",
                VideoDuration = 30,
                FileSize = 30094
            });
            books.Add(new Book
            {
                Id = 9,
                UserId = 12,
                TypeId = 3,
                Created = "09/26/2022",
                Isbn = "69943949924",
                FileName = "file1",
                UploadDate = "09/26/2022",
                VideoDuration = 30,
                FileSize = 30094
            });
            books.Add(new Book
            {
                Id = 10,
                UserId = 12,
                TypeId = 3,
                Created = "09/26/2022",
                Isbn = "69943949924",
                FileName = "file1",
                UploadDate = "09/26/2022",
                VideoDuration = 30,
                FileSize = 30094
            });
            books.Add(new Book
            {
                Id = 11,
                UserId = 12,
                TypeId = 3,
                Created = "09/26/2022",
                Isbn = "69943949924",
                FileName = "file1",
                UploadDate = "09/26/2022",
                VideoDuration = 30,
                FileSize = 30094
            });
            books.Add(new Book
            {
                Id = 12,
                UserId = 12,
                TypeId = 3,
                Created = "09/26/2022",
                Isbn = "69943949924",
                FileName = "file1",
                UploadDate = "09/26/2022",
                VideoDuration = 30,
                FileSize = 30094
            });
            return books.AsQueryable().ToDataSourceResult(requestModel);
        }
    }

    public class Book
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TypeId { get; set; }
        public string Created { get; set; }
        public string Isbn { get; set; }
        public string Note { get; set; }
        public string FileName { get; set; }
        public int FileSize { get; set; }
        public int VideoDuration { get; set; }
        public string UploadDate { get; set; }
    }
}
