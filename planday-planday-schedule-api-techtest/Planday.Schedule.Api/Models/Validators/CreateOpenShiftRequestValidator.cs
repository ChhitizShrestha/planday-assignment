using FluentValidation;

namespace Planday.Schedule.Api.Models.Validators;

public class CreateOpenShiftRequestValidator : AbstractValidator<CreateOpenShiftRequest>
{
    public CreateOpenShiftRequestValidator()
    {
        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("Start time is required")
            .Must(x => x > DateTime.Now).WithMessage("Start time must be in the future")
            .Must((request, startTime) => startTime.Date == request.EndTime.Date)
            .WithMessage("Start time and end time must be on the same day");

        RuleFor(x => x.EndTime)
            .NotEmpty().WithMessage("End time is required")
            .GreaterThan(x => x.StartTime)
            .WithMessage("End time must be after start time")
            .Must(x => x > DateTime.Now)
            .WithMessage("End time must be in the future");

        RuleFor(x => new { x.StartTime, x.EndTime })
            .Must(x => (x.EndTime - x.StartTime).TotalHours <= 12)
            .WithMessage("Shift duration cannot exceed 12 hours")
            .OverridePropertyName("Duration");
    }
}