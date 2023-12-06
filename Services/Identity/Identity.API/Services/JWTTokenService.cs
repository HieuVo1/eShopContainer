using Identity.API.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Identity.API.Services
{
    public class JWTTokenService
    {
        private readonly List<User> _users = new List<User>()
        {
                new User(){
                    Username = "admin",
                    Password = "admin",
                    Role =  "Administrator",
                    Scopes = new[] {"shoes.create" }
                },
                new User(){
                    Username = "user",
                    Password = "user",
                    Role =  "User",
                    Scopes = new[] {"shoes.read" }
                }
        };

        public Models.AuthenticationToken? GenerateAuthToken(LoginModel loginModel)
        {
            var user = _users.FirstOrDefault(u => u.Username == loginModel.Username
                                               && u.Password == loginModel.Password);
            if (user is null)
            {
                return null;
            }
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("secretJWTsigningKey@123"));
            var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
            var expirationTimeStamp = DateTime.Now.AddMinutes(5);
            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Name, user.Username),
            new Claim("Roles", user.Role),
            new Claim("scope", string.Join(" ", user.Scopes))
        };
            var tokenOptions = new JwtSecurityToken(
                issuer: "http://localhost:5002",
                claims: claims,
                expires: expirationTimeStamp,
                signingCredentials: signingCredentials
            );
            var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenOptions);
            return new Models.AuthenticationToken(tokenString, (int)expirationTimeStamp.Subtract(DateTime.Now).TotalSeconds);
        }
    }
}
