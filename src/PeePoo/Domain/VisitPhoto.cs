using System;
using System.Collections.Generic;

namespace Domain
{
    public class VisitPhoto
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public Guid VisitId { get; set; }
        public Visit Visit { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public bool IsAproved { get; set; }
    }
}
