using System;

namespace Domain
{
    public class UserBlock
    {
        public string BlockerId { get; set; }
        public string BlockedId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
