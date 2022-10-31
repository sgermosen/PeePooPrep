using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PeePooFinder.Models
{
    public class SubmitModel
    {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string type { get; set; }
        public DateTime createdAt { get; set; }
        public int rating { get; set; }
        public double @long { get; set; }
        public double lat { get; set; }
        public string observations { get; set; }
        public bool haveBabyChanger { get; set; }
        public bool isRoomy { get; set; }
        public bool isAvailable { get; set; }
        public int urinals { get; set; }
        public int toilets { get; set; }
        public byte[] ImageBytes { get; set; }
        public string ImageName { get; set; }
        public string OwnerUserName { get; set; }
        public Stream ImgStream { get; set; }
    }
}
