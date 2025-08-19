using Dapper;
using Kahoot.Helpers;
using Kahoot.Models.Auth;
using Kahoot.Services.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;

namespace Kahoot.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly string _connectionString;
        private readonly IJwtHelper _jwtHelper;

        public AuthService(IConfiguration configuration, IJwtHelper jwtHelper)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _jwtHelper = jwtHelper;
        }

        public async Task<LoginResponse> AuthenticateUserCommonLogin(LoginRequest request)
        {
            using var connection = new SqlConnection(_connectionString);

            // Encode the password to Base64
            var encodedPassword = Convert.ToBase64String(Encoding.UTF8.GetBytes(request.UserPassword));

            var parameters = new DynamicParameters();
            parameters.Add("@LoginCode", request.LoginCode);
            parameters.Add("@UserPassword", encodedPassword);
            parameters.Add("@ModuleName", request.ModuleName);
            parameters.Add("@IPAddr", request.IPAddr ?? "::1");
            parameters.Add("@SessId", request.SessId ?? Guid.NewGuid().ToString());
            parameters.Add("@Browser", request.Browser);
            parameters.Add("@BVersion", request.BVersion ?? "1.0");
            parameters.Add("@PCUSName", request.PCUSName);
            parameters.Add("@result", dbType: DbType.Byte, direction: ParameterDirection.Output);

            // Execute the stored procedure
            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "HR.AuthenticateUserCommonLogin",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            var response = new LoginResponse
            {
                Result = parameters.Get<byte>("@result")
            };

            // Always populate user info if available
            if (result != null)
            {
                response.UserId = result.UserId;
                response.WelcomeName = result.WelcomeName;
                response.LastLogin = result.lastLogin;
                response.Image = result.img;
            }

            // Get permitted and denied modules
            var permittedModules = await connection.QueryAsync<string>(
                "HR.FetchAllPermittedModulesForUser",
                new { UserId = response.UserId },
                commandType: CommandType.StoredProcedure
            );

            var deniedModules = await connection.QueryAsync<string>(
                "HR.FetchNotPermittedModulesForUser",
                new { UserId = response.UserId },
                commandType: CommandType.StoredProcedure
            );

            response.Permissions = permittedModules?.ToList() ?? new List<string>();
            response.DeniedModules = deniedModules?.ToList() ?? new List<string>();

            // Set appropriate message
            response.Message = response.Result switch
            {
                0 => "Invalid login code",
                1 => "Authentication successful",
                2 => "Invalid username or password",
                3 => "No permission to module: " + request.ModuleName,
                4 => "Account is locked",
                _ => "Authentication failed"
            };

            // Generate token with permissions for all successful authentications
            // (even if specific module access is denied)
            if (response.Result == 1 || response.Result == 3)
            {
                response.Token = _jwtHelper.GenerateToken(
                    response.UserId,
                    request.LoginCode,
                    response.Permissions
                );
            }

            return response;
        }
    }
}