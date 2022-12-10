using DashboardWebApp.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DashboardWebApp.Models
{
    public class VideoFilterViewModel
    {
        public string? ISBN { get; set; }
        public DateTime? DateTakenFrom { get; set; } = DateTime.UtcNow.AddMonths(-3);
        public DateTime? DateTakenTo { get; set; } = DateTime.UtcNow;
        public DateTime? DateUploadedFrom { get; set; } = DateTime.UtcNow.AddDays(-1);
        public DateTime? DateUploadedTo { get; set; } = DateTime.UtcNow;
        public string[]? SelectedTakenByUsers { get; set; } = new string[0];
        public string? UserNote { get; set; }
        public string[]? SelectedBookTypes { get; set; } = new string[0];
        public string[]? SelectedBookVideoLabels { get; set; } = new string[0];
        public int? VideoDurationFrom { get; set; }
        public int? VideoDurationTo { get; set; }
        public int? FileSizeFrom { get; set; }
        public int? FileSizeTo { get; set; }
        public string[]? SelectedHasComments { get; set; } = new string[0];
    }
}
