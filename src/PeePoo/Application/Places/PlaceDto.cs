using Application.Core;
using System;
using System.Collections.Generic;

namespace Application.Places
{
    public class PlaceDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string Observations { get; set; }
        public string Address { get; set; }
        public string OpeningHours { get; set; }
        public bool IsAvailable { get; set; }
        public bool HaveBabyChanger { get; set; }
        public bool IsRoomy { get; set; }
        public bool IsAccessible { get; set; }
        public bool IsFree { get; set; }
        public int Urinals { get; set; }
        public int Toilets { get; set; }
        /// <summary>Rounded community rating (average of reviews, or the submitter's rating if there are none).</summary>
        public int Rating { get; set; }
        public double? AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public double Long { get; set; }
        public double Lat { get; set; }
        public bool IsAproved { get; set; }
        public DateTime? LastVerifiedAt { get; set; }
        public double? DistanceKm { get; set; }
        public string OwnerUsername { get; set; }
        public string Image { get; set; }
        public List<PhotoDto> Photos { get; set; } = new();
        public int FavoritesCount { get; set; }
        public bool IsFavorite { get; set; }
        public bool IsOwner { get; set; }

        internal PlaceDto Finish()
        {
            if (AverageRating.HasValue)
            {
                AverageRating = Math.Round(AverageRating.Value, 1);
                Rating = (int)Math.Round(AverageRating.Value, MidpointRounding.AwayFromZero);
            }
            return this;
        }
    }
}
