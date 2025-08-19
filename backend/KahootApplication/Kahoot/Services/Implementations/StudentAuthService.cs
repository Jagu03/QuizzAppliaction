////using Dapper;
////using Kahoot.Helpers;
////using Kahoot.Models.StudentAuth;
////using Kahoot.Services.Interfaces;
////using Microsoft.Data.SqlClient;
////using System.Data;
////using System.Globalization;
////using System.Text;

////namespace Kahoot.Services.Implementations
////{
////    public class StudentAuthService : IStudentAuthService
////    {
////        private readonly string _connectionString;
////        private readonly IJwtHelper _jwtHelper;
////        private readonly ILogger<StudentAuthService> _logger;
////        private readonly IHttpContextAccessor _httpContextAccessor;

////        public StudentAuthService(
////            IConfiguration configuration,
////            IJwtHelper jwtHelper,
////            ILogger<StudentAuthService> logger,
////            IHttpContextAccessor httpContextAccessor)
////        {
////            _connectionString = configuration.GetConnectionString("DefaultConnection");
////            _jwtHelper = jwtHelper;
////            _logger = logger;
////            _httpContextAccessor = httpContextAccessor;
////        }

////        public async Task<StudentLoginResponse> AuthenticateStudent(StudentLoginRequest request)
////        {
////            try
////            {
////                // Validate date format if dob is provided
////                if (!string.IsNullOrEmpty(request.Dob) && request.Dob != "string")
////                {
////                    if (!DateTime.TryParseExact(request.Dob, "dd/MM/yyyy",
////                        CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
////                    {
////                        return new StudentLoginResponse
////                        {
////                            Result = 5,
////                            Message = "Invalid date format. Please use DD/MM/YYYY"
////                        };
////                    }
////                }

////                using var connection = new SqlConnection(_connectionString);
////                await connection.OpenAsync();

////                var parameters = new DynamicParameters();
////                parameters.Add("@RollNo", request.RollNo);

////                // Handle password encoding based on login type
////                if (request.LoginAs == 1) // Student login
////                {
////                    parameters.Add("@StudPassword", Convert.ToBase64String(Encoding.UTF8.GetBytes(request.StudPassword)));
////                }
////                else // Parent login (date of birth)
////                {
////                    parameters.Add("@StudPassword", request.StudPassword);
////                }

////                parameters.Add("@dob", string.IsNullOrEmpty(request.Dob) || request.Dob == "string" ? null : request.Dob);
////                parameters.Add("@loginAs", (byte)request.LoginAs); // Explicit cast to byte
////                parameters.Add("@IPAddr", string.IsNullOrEmpty(request.IPAddr) ? GetClientIpAddress() : request.IPAddr);
////                parameters.Add("@SessId", string.IsNullOrEmpty(request.SessId) ? GenerateSessionId() : request.SessId);
////                parameters.Add("@Browser", string.IsNullOrEmpty(request.Browser) ? GetUserAgent() : request.Browser);
////                parameters.Add("@BVersion", string.IsNullOrEmpty(request.BVersion) ? "1.0" : request.BVersion);
////                parameters.Add("@OtherLoginId", (long)request.OtherLoginId); // Explicit cast to long
////                parameters.Add("@deviceId", string.IsNullOrEmpty(request.DeviceId) ? null : request.DeviceId);
////                parameters.Add("@result", dbType: DbType.Byte, direction: ParameterDirection.Output);

////                // Execute the stored procedure
////                var result = await connection.QueryFirstOrDefaultAsync<StudentLoginResponse>(
////                    "HR.AuthenticateStudent",
////                    parameters,
////                    commandType: CommandType.StoredProcedure
////                );

////                if (result == null)
////                {
////                    var errorResult = new StudentLoginResponse
////                    {
////                        Result = (byte)parameters.Get<byte>("@result"), // Explicit cast
////                        Message = GetErrorMessage(parameters.Get<byte>("@result"))
////                    };
////                    _logger.LogWarning("Authentication failed for RollNo: {RollNo}, Result: {Result}",
////                        request.RollNo, errorResult.Result);
////                    return errorResult;
////                }

////                result.Result = (byte)parameters.Get<byte>("@result"); // Explicit cast
////                result.Message = GetErrorMessage(result.Result);

////                if (result.Result == 1) // Success
////                {
////                    result.Token = _jwtHelper.GenerateStudentToken(
////                        result.StudentId,
////                        result.RollNo,
////                        result.StudentName
////                    );
////                    _logger.LogInformation("Student authenticated: {StudentId}, {RollNo}",
////                        result.StudentId, result.RollNo);
////                }

////                return result;
////            }
////            catch (SqlException sqlEx) when (sqlEx.Number == 241) // Date conversion error
////            {
////                _logger.LogError(sqlEx, "Date conversion error during student authentication");
////                return new StudentLoginResponse
////                {
////                    Result = 6,
////                    Message = "Invalid date format in request"
////                };
////            }
////            catch (SqlException sqlEx)
////            {
////                _logger.LogError(sqlEx, "Database error during student authentication");
////                return new StudentLoginResponse
////                {
////                    Result = 3,
////                    Message = "Database error occurred"
////                };
////            }
////            catch (Exception ex)
////            {
////                _logger.LogError(ex, "Unexpected error during student authentication");
////                return new StudentLoginResponse
////                {
////                    Result = 4,
////                    Message = "An unexpected error occurred"
////                };
////            }
////        }

////        #region Helper Methods

////        private string GenerateSessionId()
////        {
////            return Guid.NewGuid().ToString("N");
////        }

////        private string GetClientIpAddress()
////        {
////            return _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "::1";
////        }

////        private string GetUserAgent()
////        {
////            return _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString() ?? "Unknown";
////        }

////        private string GetErrorMessage(int resultCode)
////        {
////            return resultCode switch
////            {
////                0 => "Invalid credentials",
////                1 => "Authentication successful",
////                2 => "Student not found",
////                3 => "Database error",
////                4 => "System error",
////                5 => "Invalid date format",
////                6 => "Date conversion error",
////                _ => "Authentication failed"
////            };
////        }

////        #endregion
////    }
////}
using Dapper;
using Kahoot.Helpers;
using Kahoot.Models.StudentAuth;
using Kahoot.Services.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.Text;

namespace Kahoot.Services.Implementations
{
    public class StudentAuthService : IStudentAuthService
    {
        private readonly string _connectionString;
        private readonly IJwtHelper _jwtHelper;
        private readonly ILogger<StudentAuthService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public StudentAuthService(
            IConfiguration configuration,
            IJwtHelper jwtHelper,
            ILogger<StudentAuthService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _jwtHelper = jwtHelper;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<StudentLoginResponse> AuthenticateStudent(StudentLoginRequest request)
        {
            try
            {
                // Validate date format if dob is provided
                if (!string.IsNullOrEmpty(request.Dob) && request.Dob != "string")
                {
                    if (!DateTime.TryParseExact(request.Dob, "dd/MM/yyyy",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                    {
                        return new StudentLoginResponse
                        {
                            Result = 5,
                            Message = "Invalid date format. Please use DD/MM/YYYY"
                        };
                    }
                }

                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new DynamicParameters();
                parameters.Add("@RollNo", request.RollNo);

                // Handle password encoding based on login type
                if (request.LoginAs == 1) // Student login
                {
                    parameters.Add("@StudPassword", Convert.ToBase64String(Encoding.UTF8.GetBytes(request.StudPassword)));
                }
                else // Parent login (date of birth)
                {
                    parameters.Add("@StudPassword", request.StudPassword);
                }

                parameters.Add("@dob", string.IsNullOrEmpty(request.Dob) || request.Dob == "string" ? null : request.Dob);
                parameters.Add("@loginAs", (byte)request.LoginAs); // Explicit cast to byte
                parameters.Add("@IPAddr", string.IsNullOrEmpty(request.IPAddr) ? GetClientIpAddress() : request.IPAddr);
                parameters.Add("@SessId", string.IsNullOrEmpty(request.SessId) ? GenerateSessionId() : request.SessId);
                parameters.Add("@Browser", string.IsNullOrEmpty(request.Browser) ? GetUserAgent() : request.Browser);
                parameters.Add("@BVersion", string.IsNullOrEmpty(request.BVersion) ? "1.0" : request.BVersion);
                parameters.Add("@OtherLoginId", (long)request.OtherLoginId); // Explicit cast to long
                parameters.Add("@deviceId", string.IsNullOrEmpty(request.DeviceId) ? null : request.DeviceId);
                parameters.Add("@result", dbType: DbType.Byte, direction: ParameterDirection.Output);

                // Execute the stored procedure
                var result = await connection.QueryFirstOrDefaultAsync<StudentLoginResponse>(
                    "HR.AuthenticateStudent",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                if (result == null)
                {
                    var errorResult = new StudentLoginResponse
                    {
                        Result = (byte)parameters.Get<byte>("@result"), // Explicit cast
                        Message = GetErrorMessage(parameters.Get<byte>("@result"))
                    };
                    _logger.LogWarning("Authentication failed for RollNo: {RollNo}, Result: {Result}",
                        request.RollNo, errorResult.Result);
                    return errorResult;
                }

                result.Result = (byte)parameters.Get<byte>("@result"); // Explicit cast
                result.Message = GetErrorMessage(result.Result);

                if (result.Result == 1) // Success
                {
                    result.Token = _jwtHelper.GenerateStudentToken(
                        result.StudentId,
                        result.RollNo,
                        result.StudentName
                    );
                    _logger.LogInformation("Student authenticated: {StudentId}, {RollNo}",
                        result.StudentId, result.RollNo);
                }

                return result;
            }
            catch (SqlException sqlEx) when (sqlEx.Number == 241) // Date conversion error
            {
                _logger.LogError(sqlEx, "Date conversion error during student authentication");
                return new StudentLoginResponse
                {
                    Result = 6,
                    Message = "Invalid date format in request"
                };
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError(sqlEx, "Database error during student authentication");
                return new StudentLoginResponse
                {
                    Result = 3,
                    Message = "Database error occurred"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during student authentication");
                return new StudentLoginResponse
                {
                    Result = 4,
                    Message = "An unexpected error occurred"
                };
            }
        }

        #region Helper Methods

        private string GenerateSessionId()
        {
            return Guid.NewGuid().ToString("N");
        }

        private string GetClientIpAddress()
        {
            return _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "::1";
        }

        private string GetUserAgent()
        {
            return _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString() ?? "Unknown";
        }

        private string GetErrorMessage(int resultCode)
        {
            return resultCode switch
            {
                0 => "Invalid credentials",
                1 => "Authentication successful",
                2 => "Student not found",
                3 => "Database error",
                4 => "System error",
                5 => "Invalid date format",
                6 => "Date conversion error",
                _ => "Authentication failed"
            };
        }

        #endregion
    }
}

