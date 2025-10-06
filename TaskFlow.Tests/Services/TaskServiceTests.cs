using AutoMapper;
using FluentAssertions;
using Moq;
using TaskFlow.Application.DTOs.Task;
using TaskFlow.Application.Mappings;
using TaskFlow.Application.Services;
using TaskFlow.Core.Entities;
using TaskFlow.Core.Enums;
using TaskFlow.Core.Interfaces;
using TaskStatus = TaskFlow.Core.Enums.TaskStatus;

namespace TaskFlow.Tests.Services
{
    public class TaskServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly IMapper _mapper;
        private readonly TaskService _taskService;

        public TaskServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();

            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfile());
            });
            _mapper = mappingConfig.CreateMapper();

            _taskService = new TaskService(_mockUnitOfWork.Object, _mapper);
        }

        [Fact]
        public async Task CreateTaskAsync_ValidTask_ReturnsSuccess()
        {
            // Arrange
            var createTaskDto = new CreateTaskDto
            {
                Title = "Test Task",
                Description = "Test Description",
                Priority = TaskPriority.High,
                Status = TaskStatus.ToDo,
                AssignedToId = "user123"
            };

            var createdTask = new WorkspaceTask
            {
                Id = Guid.NewGuid(),
                Title = createTaskDto.Title,
                Description = createTaskDto.Description,
                Priority = createTaskDto.Priority,
                Status = createTaskDto.Status,
                AssignedToId = createTaskDto.AssignedToId,
                CreatedById = "creator123",
                CreatedAt = DateTime.UtcNow
            };

            _mockUnitOfWork.Setup(x => x.Tasks.AddAsync(It.IsAny<WorkspaceTask>()))
                .ReturnsAsync(createdTask);

            _mockUnitOfWork.Setup(x => x.Tasks.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(createdTask);

            _mockUnitOfWork.Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _taskService.CreateTaskAsync(createTaskDto, "creator123");

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Title.Should().Be(createTaskDto.Title);
            result.Message.Should().Be("Task created successfully");

            _mockUnitOfWork.Verify(x => x.Tasks.AddAsync(It.IsAny<WorkspaceTask>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetTaskByIdAsync_ExistingTask_ReturnsTask()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var existingTask = new WorkspaceTask
            {
                Id = taskId,
                Title = "Test Task",
                Description = "Test Description",
                Priority = TaskPriority.Medium,
                Status = TaskStatus.InProgress,
                AssignedToId = "user123",
                CreatedById = "creator123",
                CreatedAt = DateTime.UtcNow
            };

            _mockUnitOfWork.Setup(x => x.Tasks.GetByIdAsync(taskId))
                .ReturnsAsync(existingTask);

            // Act
            var result = await _taskService.GetTaskByIdAsync(taskId);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Id.Should().Be(taskId);
            result.Data.Title.Should().Be("Test Task");

            _mockUnitOfWork.Verify(x => x.Tasks.GetByIdAsync(taskId), Times.Once);
        }

        [Fact]
        public async Task GetTaskByIdAsync_NonExistingTask_ReturnsFailure()
        {
            // Arrange
            var taskId = Guid.NewGuid();

            _mockUnitOfWork.Setup(x => x.Tasks.GetByIdAsync(taskId))
                .ReturnsAsync((WorkspaceTask?)null);

            // Act
            var result = await _taskService.GetTaskByIdAsync(taskId);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Data.Should().BeNull();
            result.Message.Should().Be("Task not found");

            _mockUnitOfWork.Verify(x => x.Tasks.GetByIdAsync(taskId), Times.Once);
        }

        [Fact]
        public async Task UpdateTaskAsync_ValidTask_ReturnsSuccess()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var updateTaskDto = new UpdateTaskDto
            {
                Id = taskId,
                Title = "Updated Task",
                Description = "Updated Description",
                Priority = TaskPriority.Urgent,
                Status = TaskStatus.Completed,
                AssignedToId = "user123"
            };

            var existingTask = new WorkspaceTask
            {
                Id = taskId,
                Title = "Original Task",
                Description = "Original Description",
                Priority = TaskPriority.Low,
                Status = TaskStatus.ToDo,
                AssignedToId = "user123",
                CreatedById = "creator123",
                CreatedAt = DateTime.UtcNow
            };

            _mockUnitOfWork.Setup(x => x.Tasks.GetByIdAsync(taskId))
                .ReturnsAsync(existingTask);

            _mockUnitOfWork.Setup(x => x.Tasks.UpdateAsync(It.IsAny<WorkspaceTask>()))
                .Returns(Task.CompletedTask);

            _mockUnitOfWork.Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _taskService.UpdateTaskAsync(updateTaskDto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Title.Should().Be("Updated Task");
            result.Message.Should().Be("Task updated successfully");

            _mockUnitOfWork.Verify(x => x.Tasks.UpdateAsync(It.IsAny<WorkspaceTask>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteTaskAsync_ExistingTask_ReturnsSuccess()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var existingTask = new WorkspaceTask
            {
                Id = taskId,
                Title = "Task to Delete",
                Description = "Description",
                Priority = TaskPriority.Low,
                Status = TaskStatus.ToDo,
                AssignedToId = "user123",
                CreatedById = "creator123",
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _mockUnitOfWork.Setup(x => x.Tasks.GetByIdAsync(taskId))
                .ReturnsAsync(existingTask);

            _mockUnitOfWork.Setup(x => x.Tasks.UpdateAsync(It.IsAny<WorkspaceTask>()))
                .Returns(Task.CompletedTask);

            _mockUnitOfWork.Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _taskService.DeleteTaskAsync(taskId);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Data.Should().BeTrue();
            result.Message.Should().Be("Task deleted successfully");

            _mockUnitOfWork.Verify(x => x.Tasks.UpdateAsync(It.IsAny<WorkspaceTask>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllTasksAsync_ReturnsTasks()
        {
            // Arrange
            var tasks = new List<WorkspaceTask>
            {
                new WorkspaceTask
                {
                    Id = Guid.NewGuid(),
                    Title = "Task 1",
                    Description = "Description 1",
                    Priority = TaskPriority.High,
                    Status = TaskStatus.ToDo,
                    AssignedToId = "user123",
                    CreatedById = "creator123",
                    CreatedAt = DateTime.UtcNow
                },
                new WorkspaceTask
                {
                    Id = Guid.NewGuid(),
                    Title = "Task 2",
                    Description = "Description 2",
                    Priority = TaskPriority.Medium,
                    Status = TaskStatus.InProgress,
                    AssignedToId = "user456",
                    CreatedById = "creator123",
                    CreatedAt = DateTime.UtcNow
                }
            };

            _mockUnitOfWork.Setup(x => x.Tasks.GetAllAsync())
                .ReturnsAsync(tasks);

            // Act
            var result = await _taskService.GetAllTasksAsync();

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Should().HaveCount(2);

            _mockUnitOfWork.Verify(x => x.Tasks.GetAllAsync(), Times.Once);
        }
    }
}