using System;

namespace Domain
{
    public class PlaceVisit
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Description { get; set; }
        public int Rating { get; set; }
        public Guid PlaceId { get; set; }
        public Place Place { get; set; }
        public string AuthorId { get; set; }
        public ApplicationUser Author { get; set; }

    }
}
