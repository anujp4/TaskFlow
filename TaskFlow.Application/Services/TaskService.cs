using AutoMapper;
using TaskFlow.Application.DTOs.Common;
using TaskFlow.Application.DTOs.Task;
using TaskFlow.Application.Interfaces;
using TaskFlow.Core.Entities;
using TaskFlow.Core.Interfaces;
using TaskStatus = TaskFlow.Core.Enums.TaskStatus;

namespace TaskFlow.Application.Services
{
    public class TaskService : ITaskService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TaskService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ResponseDto<TaskDto>> CreateTaskAsync(CreateTaskDto createTaskDto, string createdById)
        {
            try
            {
                var task = _mapper.Map<WorkspaceTask>(createTaskDto);
                task.Id = Guid.NewGuid();
                task.CreatedById = createdById;
                task.CreatedAt = DateTime.UtcNow;

                await _unitOfWork.Tasks.AddAsync(task);
                await _unitOfWork.SaveChangesAsync();

                // Reload with navigation properties
                var createdTask = await _unitOfWork.Tasks.GetByIdAsync(task.Id);
                var taskDto = _mapper.Map<TaskDto>(createdTask);

                return ResponseDto<TaskDto>.SuccessResponse(taskDto, "Task created successfully");
            }
            catch (Exception ex)
            {
                return ResponseDto<TaskDto>.FailureResponse(
                    "Failed to create task",
                    new List<string> { ex.Message });
            }
        }

        public async Task<ResponseDto<TaskDto>> UpdateTaskAsync(UpdateTaskDto updateTaskDto)
        {
            try
            {
                var existingTask = await _unitOfWork.Tasks.GetByIdAsync(updateTaskDto.Id);

                if (existingTask == null)
                {
                    return ResponseDto<TaskDto>.FailureResponse(
                        "Task not found",
                        new List<string> { "No task found with the provided ID" });
                }

                _mapper.Map(updateTaskDto, existingTask);
                existingTask.UpdatedAt = DateTime.UtcNow;

                // Mark as completed if status changed to Completed
                if (updateTaskDto.Status == TaskStatus.Completed && existingTask.CompletedAt == null)
                {
                    existingTask.CompletedAt = DateTime.UtcNow;
                }

                await _unitOfWork.Tasks.UpdateAsync(existingTask);
                await _unitOfWork.SaveChangesAsync();

                // Reload with navigation properties
                var updatedTask = await _unitOfWork.Tasks.GetByIdAsync(existingTask.Id);
                var taskDto = _mapper.Map<TaskDto>(updatedTask);

                return ResponseDto<TaskDto>.SuccessResponse(taskDto, "Task updated successfully");
            }
            catch (Exception ex)
            {
                return ResponseDto<TaskDto>.FailureResponse(
                    "Failed to update task",
                    new List<string> { ex.Message });
            }
        }

        public async Task<ResponseDto<bool>> DeleteTaskAsync(Guid taskId)
        {
            try
            {
                var task = await _unitOfWork.Tasks.GetByIdAsync(taskId);

                if (task == null)
                {
                    return ResponseDto<bool>.FailureResponse(
                        "Task not found",
                        new List<string> { "No task found with the provided ID" });
                }

                // Soft delete
                task.IsDeleted = true;
                task.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Tasks.UpdateAsync(task);
                await _unitOfWork.SaveChangesAsync();

                return ResponseDto<bool>.SuccessResponse(true, "Task deleted successfully");
            }
            catch (Exception ex)
            {
                return ResponseDto<bool>.FailureResponse(
                    "Failed to delete task",
                    new List<string> { ex.Message });
            }
        }

        public async Task<ResponseDto<TaskDto>> GetTaskByIdAsync(Guid taskId)
        {
            try
            {
                var task = await _unitOfWork.Tasks.GetByIdAsync(taskId);

                if (task == null)
                {
                    return ResponseDto<TaskDto>.FailureResponse(
                        "Task not found",
                        new List<string> { "No task found with the provided ID" });
                }

                var taskDto = _mapper.Map<TaskDto>(task);

                return ResponseDto<TaskDto>.SuccessResponse(taskDto);
            }
            catch (Exception ex)
            {
                return ResponseDto<TaskDto>.FailureResponse(
                    "Failed to retrieve task",
                    new List<string> { ex.Message });
            }
        }

        public async Task<ResponseDto<IEnumerable<TaskDto>>> GetAllTasksAsync()
        {
            try
            {
                var tasks = await _unitOfWork.Tasks.GetAllAsync();
                var taskDtos = _mapper.Map<IEnumerable<TaskDto>>(tasks);

                return ResponseDto<IEnumerable<TaskDto>>.SuccessResponse(taskDtos);
            }
            catch (Exception ex)
            {
                return ResponseDto<IEnumerable<TaskDto>>.FailureResponse(
                    "Failed to retrieve tasks",
                    new List<string> { ex.Message });
            }
        }

        public async Task<ResponseDto<IEnumerable<TaskDto>>> GetTasksByUserIdAsync(string userId)
        {
            try
            {
                var tasks = await _unitOfWork.Tasks.GetTasksByUserIdAsync(userId);
                var taskDtos = _mapper.Map<IEnumerable<TaskDto>>(tasks);

                return ResponseDto<IEnumerable<TaskDto>>.SuccessResponse(taskDtos);
            }
            catch (Exception ex)
            {
                return ResponseDto<IEnumerable<TaskDto>>.FailureResponse(
                    "Failed to retrieve user tasks",
                    new List<string> { ex.Message });
            }
        }

        public async Task<ResponseDto<IEnumerable<TaskDto>>> GetTasksByStatusAsync(TaskStatus status)
        {
            try
            {
                var tasks = await _unitOfWork.Tasks.GetTasksByStatusAsync(status);
                var taskDtos = _mapper.Map<IEnumerable<TaskDto>>(tasks);

                return ResponseDto<IEnumerable<TaskDto>>.SuccessResponse(taskDtos);
            }
            catch (Exception ex)
            {
                return ResponseDto<IEnumerable<TaskDto>>.FailureResponse(
                    "Failed to retrieve tasks by status",
                    new List<string> { ex.Message });
            }
        }

        public async Task<ResponseDto<IEnumerable<TaskDto>>> GetOverdueTasksAsync()
        {
            try
            {
                var tasks = await _unitOfWork.Tasks.GetOverdueTasksAsync();
                var taskDtos = _mapper.Map<IEnumerable<TaskDto>>(tasks);

                return ResponseDto<IEnumerable<TaskDto>>.SuccessResponse(taskDtos);
            }
            catch (Exception ex)
            {
                return ResponseDto<IEnumerable<TaskDto>>.FailureResponse(
                    "Failed to retrieve overdue tasks",
                    new List<string> { ex.Message });
            }
        }
        public async Task<ResponseDto<PagedResult<TaskDto>>> GetTasksPagedAsync(TaskFilterParams filterParams)
        {
            try
            {
                var tasks = await _unitOfWork.Tasks.GetAllAsync();
                var query = tasks.AsQueryable();

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(filterParams.SearchTerm))
                {
                    var searchTerm = filterParams.SearchTerm.ToLower();
                    query = query.Where(t =>
                        t.Title.ToLower().Contains(searchTerm) ||
                        t.Description.ToLower().Contains(searchTerm));
                }

                // Apply status filter
                if (filterParams.Status.HasValue)
                {
                    query = query.Where(t => t.Status == filterParams.Status.Value);
                }

                // Apply priority filter
                if (filterParams.Priority.HasValue)
                {
                    query = query.Where(t => t.Priority == filterParams.Priority.Value);
                }

                // Apply assigned user filter
                if (!string.IsNullOrWhiteSpace(filterParams.AssignedToId))
                {
                    query = query.Where(t => t.AssignedToId == filterParams.AssignedToId);
                }

                // Apply overdue filter
                if (filterParams.IsOverdue.HasValue && filterParams.IsOverdue.Value)
                {
                    query = query.Where(t =>
                        t.DueDate < DateTime.UtcNow &&
                        t.Status != TaskStatus.Completed &&
                        t.Status != TaskStatus.Cancelled);
                }

                // Apply sorting
                query = filterParams.SortBy.ToLower() switch
                {
                    "title" => filterParams.SortOrder.ToLower() == "asc"
                        ? query.OrderBy(t => t.Title)
                        : query.OrderByDescending(t => t.Title),
                    "priority" => filterParams.SortOrder.ToLower() == "asc"
                        ? query.OrderBy(t => t.Priority)
                        : query.OrderByDescending(t => t.Priority),
                    "status" => filterParams.SortOrder.ToLower() == "asc"
                        ? query.OrderBy(t => t.Status)
                        : query.OrderByDescending(t => t.Status),
                    "duedate" => filterParams.SortOrder.ToLower() == "asc"
                        ? query.OrderBy(t => t.DueDate)
                        : query.OrderByDescending(t => t.DueDate),
                    _ => filterParams.SortOrder.ToLower() == "asc"
                        ? query.OrderBy(t => t.CreatedAt)
                        : query.OrderByDescending(t => t.CreatedAt)
                };

                // Get total count
                var totalCount = query.Count();

                // Apply pagination
                var items = query
                    .Skip((filterParams.PageNumber - 1) * filterParams.PageSize)
                    .Take(filterParams.PageSize)
                    .ToList();

                var taskDtos = _mapper.Map<List<TaskDto>>(items);
                var pagedResult = new PagedResult<TaskDto>(
                    taskDtos,
                    totalCount,
                    filterParams.PageNumber,
                    filterParams.PageSize);

                return ResponseDto<PagedResult<TaskDto>>.SuccessResponse(pagedResult);
            }
            catch (Exception ex)
            {
                return ResponseDto<PagedResult<TaskDto>>.FailureResponse(
                    "Failed to retrieve tasks",
                    new List<string> { ex.Message });
            }
        }
    }
}