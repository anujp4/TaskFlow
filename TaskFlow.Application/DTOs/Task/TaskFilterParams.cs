using TaskFlow.Application.DTOs.Common;
using TaskFlow.Core.Entities;
using TaskFlow.Core.Enums;
using TaskStatus = TaskFlow.Core.Enums.TaskStatus;
namespace TaskFlow.Application.DTOs.Task
{
    public class TaskFilterParams : PaginationParams
    {
        public string? SearchTerm { get; set; }
        public TaskStatus? Status { get; set; }
        public TaskPriority? Priority { get; set; }
        public string? AssignedToId { get; set; }
        public bool? IsOverdue { get; set; }
        public string SortBy { get; set; } = "CreatedAt";
        public string SortOrder { get; set; } = "desc"; // asc or desc
    }
}