using TaskFlow.Application.DTOs.Auth;
using TaskFlow.Core.Entities;
using TaskFlow.Core.Enums;
using TaskStatus = TaskFlow.Core.Enums.TaskStatus;

namespace TaskFlow.Application.DTOs.Task
{
    public class TaskDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TaskPriority Priority { get; set; }
        public string PriorityName { get; set; } = string.Empty;
        public TaskStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string AssignedToId { get; set; } = string.Empty;
        public UserDto? AssignedTo { get; set; }

        public string CreatedById { get; set; } = string.Empty;
        public UserDto? CreatedBy { get; set; }

        public bool IsOverdue => DueDate.HasValue
            && DueDate.Value < DateTime.UtcNow
            && Status != TaskStatus.Completed
            && Status != TaskStatus.Cancelled;
    }
}