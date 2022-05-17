using System;

namespace Domain
{
    public class Photo
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public Guid PlaceId { get; set; }
        public Place Place { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public bool IsMain { get; set; }
        public bool IsAproved { get; set; }

    }
}
