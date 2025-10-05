using TaskFlow.Core.Entities;

namespace TaskFlow.Core.Interfaces
{
    public interface ITaskRepository : IRepository<WorkspaceTask>
    {
        Task<IEnumerable<WorkspaceTask>> GetTasksByUserIdAsync(string userId);
        Task<IEnumerable<WorkspaceTask>> GetTasksByStatusAsync(Core.Enums.TaskStatus status);
        Task<IEnumerable<WorkspaceTask>> GetOverdueTasksAsync();
    }
}