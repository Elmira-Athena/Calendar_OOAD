using FluentValidation;

namespace Calendar.Shared.Validators;

public class CreateAppointmentValidator : AbstractValidator<DTOs.Appointments.CreateAppointmentRequest>
{
    public CreateAppointmentValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Tên cuộc hẹn không được để trống.");
        RuleFor(x => x.StartTime).NotEmpty().WithMessage("Thời gian bắt đầu không được để trống.");
        RuleFor(x => x.EndTime).NotEmpty().WithMessage("Thời gian kết thúc không được để trống.");
        
        RuleFor(x => x)
            .Must(x => x.EndTime >= x.StartTime)
            .WithMessage("Thời gian kết thúc phải sau hoặc bằng thời gian bắt đầu.");
    }
}

public class UpdateAppointmentValidator : AbstractValidator<DTOs.Appointments.UpdateAppointmentRequest>
{
    public UpdateAppointmentValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Tên cuộc hẹn không được để trống.");
        RuleFor(x => x.StartTime).NotEmpty().WithMessage("Thời gian bắt đầu không được để trống.");
        RuleFor(x => x.EndTime).NotEmpty().WithMessage("Thời gian kết thúc không được để trống.");
        
        RuleFor(x => x)
            .Must(x => x.EndTime >= x.StartTime)
            .WithMessage("Thời gian kết thúc phải sau hoặc bằng thời gian bắt đầu.");
    }
}
