using System;
using System.Collections.Generic;

namespace Domain
{
    public class Place
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string Observations { get; set; }
        public bool IsAvailable { get; set; }
        public bool HaveBabyChanger { get; set; }
        public bool IsRoomy { get; set; }
        public int Urinals { get; set; }
        public int Toilets { get; set; }
        public int Rating { get; set; }
        public double Long { get; set; }
        public double Lat { get; set; }
        public bool IsAproved { get; set; }
        public ICollection<Visit> Visits { get; set; } = new List<Visit>();
        public ICollection<FavoritePlace> Favorites { get; set; } = new List<FavoritePlace>();
        public ICollection<Photo> Photos { get; set; } = new List<Photo>();

    }
}
