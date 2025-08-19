using Kahoot.Models.Group;
using Kahoot.Services.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Kahoot.Services.Implementations
{
    public class GroupService : IGroupService
    {
        private readonly string _cs;
        public GroupService(IConfiguration cfg) => _cs = cfg.GetConnectionString("DefaultConnection")!;

        public async Task<int> CreateGroupForClassAsync(CreateClassGroupDto dto)
        {
            using var con = new SqlConnection(_cs);
            using var cmd = new SqlCommand("Quizz.CreateGroupForClass", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@ClassId", dto.ClassId);
            cmd.Parameters.AddWithValue("@GroupName", dto.GroupName);
            cmd.Parameters.AddWithValue("@CreatedByStaffId", dto.CreatedByStaffId);
            var outId = new SqlParameter("@GroupId", SqlDbType.Int) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(outId);
            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            return (int)outId.Value;
        }

        public async Task<int> CreateCustomGroupAsync(CreateCustomGroupDto dto)
        {
            using var con = new SqlConnection(_cs);
            using var cmd = new SqlCommand("Quizz.CreateCustomGroup", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@GroupName", dto.GroupName);
            cmd.Parameters.AddWithValue("@CreatedByStaffId", dto.CreatedByStaffId);
            cmd.Parameters.AddWithValue("@StudentIdsCSV", dto.StudentIdsCsv);
            var outId = new SqlParameter("@GroupId", SqlDbType.Int) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(outId);
            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            return (int)outId.Value;
        }
    }
}