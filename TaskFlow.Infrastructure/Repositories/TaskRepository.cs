using Microsoft.EntityFrameworkCore;
using TaskFlow.Core.Entities;
using TaskFlow.Core.Interfaces;
using TaskFlow.Infrastructure.Data;

namespace TaskFlow.Infrastructure.Repositories
{
    public class TaskRepository : Repository<WorkspaceTask>, ITaskRepository
    {
        public TaskRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<WorkspaceTask>> GetTasksByUserIdAsync(string userId)
        {
            return await _dbSet
                .Include(t => t.AssignedTo)
                .Include(t => t.CreatedBy)
                .Where(t => t.AssignedToId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<WorkspaceTask>> GetTasksByStatusAsync(Core.Enums.TaskStatus status)
        {
            return await _dbSet
                .Include(t => t.AssignedTo)
                .Include(t => t.CreatedBy)
                .Where(t => t.Status == status)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<WorkspaceTask>> GetOverdueTasksAsync()
        {
            return await _dbSet
                .Include(t => t.AssignedTo)
                .Include(t => t.CreatedBy)
                .Where(t => t.DueDate < DateTime.UtcNow
                    && t.Status != Core.Enums.TaskStatus.Completed
                    && t.Status != Core.Enums.TaskStatus.Cancelled)
                .OrderBy(t => t.DueDate)
                .ToListAsync();
        }
    }
}