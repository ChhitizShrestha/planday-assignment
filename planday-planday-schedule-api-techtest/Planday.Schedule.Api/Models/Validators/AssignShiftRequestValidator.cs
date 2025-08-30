using FluentValidation;

namespace Planday.Schedule.Api.Models.Validators;

public class AssignShiftRequestValidator : AbstractValidator<AssignShiftRequest>
{
    public AssignShiftRequestValidator()
    {
        RuleFor(x => x.ShiftId)
            .GreaterThan(0)
            .WithMessage("Invalid shift ID");

        RuleFor(x => x.EmployeeId)
            .GreaterThan(0)
            .WithMessage("Invalid employee ID");
    }
}