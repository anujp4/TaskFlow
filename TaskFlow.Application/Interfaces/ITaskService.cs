using TaskFlow.Application.DTOs.Common;
using TaskFlow.Application.DTOs.Task;
using TaskFlow.Core.Entities;
using TaskStatus = TaskFlow.Core.Enums.TaskStatus;

namespace TaskFlow.Application.Interfaces
{
    public interface ITaskService
    {
        Task<ResponseDto<TaskDto>> CreateTaskAsync(CreateTaskDto createTaskDto, string createdById);
        Task<ResponseDto<TaskDto>> UpdateTaskAsync(UpdateTaskDto updateTaskDto);
        Task<ResponseDto<bool>> DeleteTaskAsync(Guid taskId);
        Task<ResponseDto<TaskDto>> GetTaskByIdAsync(Guid taskId);
        Task<ResponseDto<IEnumerable<TaskDto>>> GetAllTasksAsync();
        Task<ResponseDto<IEnumerable<TaskDto>>> GetTasksByUserIdAsync(string userId);
        Task<ResponseDto<IEnumerable<TaskDto>>> GetTasksByStatusAsync(TaskStatus status);
        Task<ResponseDto<IEnumerable<TaskDto>>> GetOverdueTasksAsync();
    }
}