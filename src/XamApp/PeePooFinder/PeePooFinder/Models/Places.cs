using System;
using System.Collections.Generic;
using System.Text;

namespace PeePooFinder.Models
{
    public class Places
    {
        public string id { get; set; }
        public string name { get; set; }
        public DateTime createdAt { get; set; }
        public string type { get; set; }
        public string description { get; set; }
        public string observations { get; set; }
        public bool isAvailable { get; set; }
        public bool haveBabyChanger { get; set; }
        public bool isRoomy { get; set; }
        public int urinals { get; set; }
        public int toilets { get; set; }
        public int rating { get; set; }
        public double @long { get; set; }
        public double lat { get; set; }
        public bool isAproved { get; set; }
        public string ownerUsername { get; set; }
        public List<Favorite> favorites { get; set; }
        public List<PlacePhoto> photos { get; set; }
        public List<Visit> visits { get; set; }
        public string file { get; set; }
        public string image { get; set; }
    }
    public class Favorite
    {
        public string username { get; set; }
        public string displayName { get; set; }
        public string bio { get; set; }
        public string image { get; set; }
    }
    public class PlacePhoto
    {
        public string id { get; set; }
        public string url { get; set; }
        public string placeId { get; set; }
        public string place { get; set; }
        public string userId { get; set; }
        public string user { get; set; }
        public bool isMain { get; set; }
        public bool isAproved { get; set; }
    }
    public class VisitPhoto
    {
        public string id { get; set; }
        public string url { get; set; }
        public string visitId { get; set; }
        public string visit { get; set; }
        public string userId { get; set; }
        public string user { get; set; }
        public bool isAproved { get; set; }
    }

    public class Visit
    {
        public string id { get; set; }
        public string title { get; set; }
        public DateTime createdAt { get; set; }
        public string description { get; set; }
        public int rating { get; set; }
        public string username { get; set; }
        public string displayName { get; set; }
        public string image { get; set; }
        public string placeId { get; set; }
        public List<VisitPhoto> photos { get; set; }
        public string file { get; set; }
    }

}
