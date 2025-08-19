using Kahoot.Models.Group;

namespace Kahoot.Services.Interfaces
{
    public interface IGroupService
    {
        Task<int> CreateGroupForClassAsync(CreateClassGroupDto dto);
        Task<int> CreateCustomGroupAsync(CreateCustomGroupDto dto);
    }
}
