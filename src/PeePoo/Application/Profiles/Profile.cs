using System;

namespace Application.Profiles
{
    public class Profile
    {
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string Bio { get; set; }
        /// <summary>Reserved for avatars; always null for now (clients show initials).</summary>
        public string Image { get; set; }
        public int PlacesCount { get; set; }
        public int ReviewsCount { get; set; }
        public DateTime JoinedAt { get; set; }
    }
}
