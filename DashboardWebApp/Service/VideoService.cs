using DashboardWebApp.Data;
using DashboardWebApp.Models;
using Microsoft.EntityFrameworkCore;

namespace DashboardWebApp.Service
{
    public class VideoService : IVideoService
    {
        private readonly ApplicationDbContext context;
        private readonly IUserService userService;
        public VideoService(IDbFactory dbFactory, IUserService userService)
        {
            context = dbFactory.GetDatabaseContext();
            this.userService = userService;
        }

        public BookDetailsViewModel GetBookDetails(int bookId)
        {
            var bookTypes = context.BookTypes.ToList();
            var book = context.Books.Include(x => x.User).SingleOrDefault(x => x.Id == bookId);

            if (book == null)
                return new BookDetailsViewModel();

            var bookVideo = context.BookVideos.SingleOrDefault(x => x.FileName == book.FileName && x.UserId == book.UserId);

            var bookDetailsViewModel = new BookDetailsViewModel()
            {
                BookId = book.Id,
                FileName = book.FileName,
                BookVideoUrl = bookVideo?.VideoUri ?? string.Empty,
                VideoDuration = book.VideoDuration > 60 ? $"{book.VideoDuration / 60} minute(s)" : $"< a minute",
                VideoFileSize = book.FileSize > 1000 ? $"{(book.FileSize / 1000)} MB" : $"< 1 MB",
                DateUploaded = book.UploadDate.GetValueOrDefault().ToString("dd MMM yyyy hh:mm tt"),
                DateTaken = book.Created.ToString("dd MMM yyyy hh:mm tt"),
                UserNote = book.Note,
                BookType = bookTypes.SingleOrDefault(b => b.Id == book.TypeId).Name ?? string.Empty,
                CreatedBy = book.User.UserName
            };

            return bookDetailsViewModel;
        }

        public List<VideoHistoryViewModel> GetVideoHistory(int bookId)
        {
            var bookVideoHistory = context.BookVideoHistory.Where(x => x.BookId == bookId).OrderByDescending(x => x.Created).Take(7);

            if (bookVideoHistory == null || bookVideoHistory.Count() == 0)
                return new List<VideoHistoryViewModel>();

            return bookVideoHistory.Include(x => x.User).Select(x => new VideoHistoryViewModel
            {
                History = x.History,
                HistoryDate = $"{x.Created.ToString("dd MMM yyyy")} at {x.Created.ToString("hh:mm tt")}"
            }).ToList();
        }

        public List<VideoCommentsViewModel> GetVideoComments(int bookId)
        {
            var bookVideoComments = context.BookVideoComments.Where(x => x.BookId == bookId).OrderByDescending(x => x.Created).Take(7);

            if (bookVideoComments == null || bookVideoComments.Count() == 0)
                return new List<VideoCommentsViewModel>();

            return bookVideoComments.Include(x => x.User).Select(x => new VideoCommentsViewModel
            {
                Comment = x.Comment,
                CommentByUser = x.User.UserName,
                CommentDate = $"{x.Created.ToString("dd MMM yyyy")} at {x.Created.ToString("hh:mm tt")}"
            }).ToList();
        }

        public List<VideoLabelsViewModel> GetVideoLabels(int bookId)
        {
            var bookVideoLabels = context.BookVideoLabels.Where(x => x.BookId == bookId).OrderByDescending(x => x.Created).Take(6);

            if (bookVideoLabels == null || bookVideoLabels.Count() == 0)
                return new List<VideoLabelsViewModel>();

            return bookVideoLabels.Select(x => new VideoLabelsViewModel
            {
                Label = x.Label
            }).ToList();
        }

        public BookVideoComments AddVideoComment(int bookId, string comment)
        {
            var currentUser = this.userService.GetCurrentUser();

            if (currentUser == null)
                return null;

            var book = context.Books.SingleOrDefault(x => x.Id == bookId);

            if (book == null)
                return null;

            var bookVideoComments = new BookVideoComments
            {
                BookId = bookId,
                Comment = comment,
                UserId = currentUser.UserId,
                UserName = currentUser.UserName,
                DisplayDate = $"{DateTime.UtcNow.ToString("dd MMM yyyy")} at {DateTime.UtcNow.ToString("hh:mm tt")}",
                Created = DateTime.UtcNow,
            };

            context.BookVideoComments.Add(bookVideoComments);

            context.BookVideoHistory.Add(new BookVideoHistory
            {
                BookId = bookId,
                Created = DateTime.UtcNow,
                UserId = currentUser.UserId,
                History = $"{currentUser.UserName} added a new comment: {comment}"
            });

            context.SaveChanges();

            return bookVideoComments;
        }

        public BookVideoLabels AddVideoLabel(int bookId, string label)
        {
            var currentUser = this.userService.GetCurrentUser();

            if (currentUser == null)
                return null;

            var book = context.Books.SingleOrDefault(x => x.Id == bookId);

            if (book == null)
                return null;

            var bookVideoLabels = new BookVideoLabels
            {
                BookId = bookId,
                Label = label,
                UserId = currentUser.UserId,
                Created = DateTime.UtcNow,
                DisplayDate = $"{DateTime.UtcNow.ToString("dd MMM yyyy")} at {DateTime.UtcNow.ToString("hh:mm tt")}",
            };

            context.BookVideoLabels.Add(bookVideoLabels);

            context.BookVideoHistory.Add(new BookVideoHistory
            {
                BookId = bookId,
                Created = DateTime.UtcNow,
                UserId = currentUser.UserId,
                History = $"{currentUser.UserName} added a new label {label}",
                DisplayDate = $"{DateTime.UtcNow.ToString("dd MMM yyyy")} at {DateTime.UtcNow.ToString("hh:mm tt")}",
            });

            context.SaveChanges();

            return bookVideoLabels;
        }
    }
}
