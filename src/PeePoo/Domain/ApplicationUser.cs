using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace Domain
{
    public class ApplicationUser : IdentityUser
    {
        public string DisplayName { get; set; }
        public string Bio { get; set; }
        public ICollection<Place> Places { get; set; }
        public ICollection<Photo> Photos { get; set; }
        public ICollection<FavoritePlace> FavoritePlaces { get; set; }

    }
}
