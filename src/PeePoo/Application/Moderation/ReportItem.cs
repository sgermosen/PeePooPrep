using System;

namespace Application.Moderation
{
    public class ReportItem
    {
        public Guid Id { get; set; }
        public string TargetType { get; set; }
        public Guid TargetId { get; set; }
        public string Reason { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ReporterUsername { get; set; }
        /// <summary>Name of the place, or title of the review; null if the content no longer exists.</summary>
        public string TargetTitle { get; set; }
        public string TargetText { get; set; }
        public string TargetAuthor { get; set; }
        public bool TargetHidden { get; set; }
        public int OpenReportsForTarget { get; set; }
    }
}
