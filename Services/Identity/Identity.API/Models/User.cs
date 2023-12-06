namespace Identity.API.Models
{
    public record User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string[] Scopes { get; set; }
    }
}
