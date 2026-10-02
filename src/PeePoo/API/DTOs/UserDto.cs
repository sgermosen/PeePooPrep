namespace API.DTOs
{
    public class UserDto
    {
        public string DisplayName { get; set; }
        public string Username { get; set; }
        public string Token { get; set; }
        /// <summary>Reserved for avatars; always null for now.</summary>
        public string Image { get; set; }
        public bool IsAdmin { get; set; }
    }
}
