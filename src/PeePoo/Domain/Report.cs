using System;

namespace Domain
{
    public class Report
    {
        public Guid Id { get; set; }
        public string TargetType { get; set; }
        public Guid TargetId { get; set; }
        public string ReporterId { get; set; }
        public string Reason { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Resolved { get; set; }
    }
}
