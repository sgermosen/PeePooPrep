using Microsoft.AspNetCore.Http;
using System;

namespace Application.Places
{
    /// <summary>
    /// What a client may send when creating or editing a place. Server-owned fields
    /// (approval, owner, verification, timestamps) are deliberately absent.
    /// </summary>
    public class PlaceInput
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string Observations { get; set; }
        public string Address { get; set; }
        public string OpeningHours { get; set; }
        public bool IsAvailable { get; set; } = true;
        public bool HaveBabyChanger { get; set; }
        public bool IsRoomy { get; set; }
        public bool IsAccessible { get; set; }
        public bool IsFree { get; set; } = true;
        public int Urinals { get; set; }
        public int Toilets { get; set; }
        /// <summary>The submitter's own first impression (1-5), or 0 to skip.</summary>
        public int Rating { get; set; }
        public double Lat { get; set; }
        public double Long { get; set; }
        public IFormFile File { get; set; }
    }
}
