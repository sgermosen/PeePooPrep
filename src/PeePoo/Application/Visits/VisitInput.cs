using Microsoft.AspNetCore.Http;
using System;

namespace Application.Visits
{
    public class VisitInput
    {
        public Guid? Id { get; set; }
        public Guid PlaceId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Rating { get; set; }
        public IFormFile File { get; set; }
    }

    public class VisitEditInput
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int Rating { get; set; }
    }
}
