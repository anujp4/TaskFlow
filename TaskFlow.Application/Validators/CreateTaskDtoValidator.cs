using FluentValidation;
using TaskFlow.Application.DTOs.Task;
using TaskFlow.Core.Entities;
using TaskFlow.Core.Enums;

namespace TaskFlow.Application.Validators
{
    public class CreateTaskDtoValidator : AbstractValidator<CreateTaskDto>
    {
        public CreateTaskDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required")
                .MinimumLength(3).WithMessage("Title must be at least 3 characters")
                .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");

            RuleFor(x => x.Priority)
                .IsInEnum().WithMessage("Invalid priority value")
                .NotEqual(TaskPriority.Low).When(x => x.Title.Contains("urgent", StringComparison.OrdinalIgnoreCase))
                .WithMessage("Urgent tasks cannot have low priority");

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Invalid status value");

            RuleFor(x => x.DueDate)
                .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
                .When(x => x.DueDate.HasValue)
                .WithMessage("Due date cannot be in the past");

            RuleFor(x => x.AssignedToId)
                .NotEmpty().WithMessage("Assigned user is required");
        }
    }
}