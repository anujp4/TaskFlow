using System.ComponentModel.DataAnnotations;
using TaskFlow.Core.Entities;
using TaskFlow.Core.Enums;
using TaskStatus = TaskFlow.Core.Enums.TaskStatus;

namespace TaskFlow.Application.DTOs.Task
{
    public class CreateTaskDto
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, MinimumLength = 3)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Priority is required")]
        public TaskPriority Priority { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public TaskStatus Status { get; set; } = TaskStatus.ToDo;

        public DateTime? DueDate { get; set; }

        [Required(ErrorMessage = "Assigned user is required")]
        public string AssignedToId { get; set; } = string.Empty;
    }
}