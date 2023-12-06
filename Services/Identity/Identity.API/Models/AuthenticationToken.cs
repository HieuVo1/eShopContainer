namespace Identity.API.Models
{
    public record AuthenticationToken(string Token, int ExpiresIn);
}
