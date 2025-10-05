using Microsoft.AspNetCore.Identity;
namespace TaskFlow.Core.Entities
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public ICollection<WorkspaceTask> AssignedTasks { get; set; } = new List<WorkspaceTask>();
        public ICollection<WorkspaceTask> CreatedTasks { get; set; } = new List<WorkspaceTask>();
    }
}