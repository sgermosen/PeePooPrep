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
    }
}
