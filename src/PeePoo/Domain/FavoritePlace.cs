using System;

namespace Domain
{
    public class FavoritePlace
    {
        public Guid PlaceId { get; set; }
        public Place Place { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public bool IsOwner { get; set; }
    }
}
