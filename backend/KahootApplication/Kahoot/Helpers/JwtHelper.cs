//using Microsoft.IdentityModel.Tokens;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;

//namespace Kahoot.Helpers
//{
//    public interface IJwtHelper
//    {
//        string GenerateToken(int userId, string username, List<string> permissions);
//    }

//    public class JwtHelper : IJwtHelper
//    {
//        private readonly IConfiguration _configuration;

//        public JwtHelper(IConfiguration configuration)
//        {
//            _configuration = configuration;
//        }

//        public string GenerateToken(int userId, string username, List<string> permissions)
//        {
//            var tokenHandler = new JwtSecurityTokenHandler();
//            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]);

//            var claims = new List<Claim>
//            {
//                new Claim("UserId", userId.ToString()),
//                new Claim(ClaimTypes.Name, username),
//                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
//            };

//            // Add permissions as claims
//            foreach (var permission in permissions)
//            {
//                claims.Add(new Claim("Permission", permission));
//            }

//            var tokenDescriptor = new SecurityTokenDescriptor
//            {
//                Subject = new ClaimsIdentity(claims),
//                Expires = DateTime.UtcNow.AddHours(Convert.ToDouble(_configuration["Jwt:ExpireHours"])),
//                Issuer = _configuration["Jwt:Issuer"],
//                Audience = _configuration["Jwt:Audience"],
//                SigningCredentials = new SigningCredentials(
//                    new SymmetricSecurityKey(key),
//                    SecurityAlgorithms.HmacSha256Signature)
//            };

//            var token = tokenHandler.CreateToken(tokenDescriptor);
//            return tokenHandler.WriteToken(token);
//        }
//    }
//}
using Kahoot.Services.Implementations;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Kahoot.Helpers
{
    public interface IJwtHelper
    {
        string GenerateToken(int userId, string username, List<string> permissions);
        string GenerateStudentToken(int studentId, string rollNo, string studentName);
        string GenerateToken(Dictionary<string, string> claims);
    }

    public class JwtHelper : IJwtHelper
    {
        private readonly IConfiguration _configuration;

        public JwtHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Existing method for user authentication
        public string GenerateToken(int userId, string username, List<string> permissions)
        {
            var claims = new List<Claim>
            {
                new Claim("UserId", userId.ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Add permissions as claims
            foreach (var permission in permissions)
            {
                claims.Add(new Claim("Permission", permission));
            }

            return CreateToken(claims);
        }

        // New method for student authentication
        public string GenerateStudentToken(int studentId, string rollNo, string studentName)
        {
            var claims = new List<Claim>
            {
                new Claim("StudentId", studentId.ToString()),
                new Claim("RollNo", rollNo),
                new Claim(ClaimTypes.Name, studentName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("UserType", "Student") // Helps identify token type
            };

            return CreateToken(claims);
        }

        // Generic method for custom claims
        public string GenerateToken(Dictionary<string, string> claims)
        {
            var claimList = new List<Claim>();
            foreach (var claim in claims)
            {
                claimList.Add(new Claim(claim.Key, claim.Value));
            }
            claimList.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

            return CreateToken(claimList);
        }

        // Private method to handle common token creation logic
        private string CreateToken(List<Claim> claims)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(Convert.ToDouble(_configuration["Jwt:ExpireHours"])),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}

