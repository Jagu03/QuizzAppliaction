using Kahoot.Models.Assignment;
using Kahoot.Services.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Kahoot.Services.Implementations
{
    public class AssignmentService : IAssignmentService
    {
        private readonly string _cs;
        public AssignmentService(IConfiguration cfg) => _cs = cfg.GetConnectionString("DefaultConnection")!;

        public async Task<int> PublishAsync(PublishAssignmentDto dto)
        {
            using var con = new SqlConnection(_cs);
            using var cmd = new SqlCommand("Quizz.PublishAssignment", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@Title", dto.Title);
            cmd.Parameters.AddWithValue("@CategoryId", dto.CategoryId);
            cmd.Parameters.AddWithValue("@GroupId", dto.GroupId);
            cmd.Parameters.AddWithValue("@StartAt", dto.StartAt);
            cmd.Parameters.AddWithValue("@EndAt", dto.EndAt);
            cmd.Parameters.AddWithValue("@TimeLimitSeconds", (object?)dto.TimeLimitSeconds ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ShuffleQuestions", dto.ShuffleQuestions);
            cmd.Parameters.AddWithValue("@ShuffleOptions", dto.ShuffleOptions);
            cmd.Parameters.AddWithValue("@MaxAttempts", dto.MaxAttempts);
            cmd.Parameters.AddWithValue("@CreatedByStaffId", dto.CreatedByStaffId);
            var outId = new SqlParameter("@AssignmentId", SqlDbType.Int) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(outId);
            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            return (int)outId.Value;
        }

        public async Task<(int assignmentId, int groupId)> PublishForClassAsync(PublishForClassDto dto)
        {
            using var con = new SqlConnection(_cs);
            using var cmd = new SqlCommand("Quizz.PublishAssignmentForClass", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@ClassId", dto.ClassId);
            cmd.Parameters.AddWithValue("@GroupName", dto.GroupName);
            cmd.Parameters.AddWithValue("@Title", dto.Title);
            cmd.Parameters.AddWithValue("@CategoryId", dto.CategoryId);
            cmd.Parameters.AddWithValue("@StartAt", dto.StartAt);
            cmd.Parameters.AddWithValue("@EndAt", dto.EndAt);
            cmd.Parameters.AddWithValue("@TimeLimitSeconds", (object?)dto.TimeLimitSeconds ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ShuffleQuestions", dto.ShuffleQuestions);
            cmd.Parameters.AddWithValue("@ShuffleOptions", dto.ShuffleOptions);
            cmd.Parameters.AddWithValue("@MaxAttempts", dto.MaxAttempts);
            cmd.Parameters.AddWithValue("@CreatedByStaffId", dto.CreatedByStaffId);
            var outA = new SqlParameter("@AssignmentId", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var outG = new SqlParameter("@GroupId", SqlDbType.Int) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(outA); cmd.Parameters.Add(outG);
            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            return ((int)outA.Value, (int)outG.Value);
        }

    }
}
