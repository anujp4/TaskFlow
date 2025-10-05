using TaskFlow.Core.Enums;

namespace TaskFlow.Core.Entities
{
    public class WorkspaceTask : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TaskPriority Priority { get; set; }
        public Enums.TaskStatus Status { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }

        // Foreign keys
        public string AssignedToId { get; set; } = string.Empty;
        public string CreatedById { get; set; } = string.Empty;

        // Navigation properties
        public User AssignedTo { get; set; } = null!;
        public User CreatedBy { get; set; } = null!;
    }
}