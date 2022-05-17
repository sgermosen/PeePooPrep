using System;

namespace Application.Visits
{
    public class VisitDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Description { get; set; }
        public int Rating { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string Image { get; set; }
    }
}
