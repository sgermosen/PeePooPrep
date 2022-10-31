using System;
using System.Collections.Generic;
using System.Text;

namespace PeePooFinder.Models
{
    public class Visits
    {
        public string id { get; set; }
        public string placeId { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public DateTime createdAt { get; set; }
        public int rating { get; set; }
        public string ImageName { get; set; }
        public byte[] ImageBytes { get; set; }
        public string OwnerUserName { get; set; }
    }
}
