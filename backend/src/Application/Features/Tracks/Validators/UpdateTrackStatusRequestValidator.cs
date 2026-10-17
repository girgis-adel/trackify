using Application.Features.Tracks.Dtos;
using FluentValidation;

namespace Application.Features.Tracks.Validators;

public sealed class UpdateTrackStatusRequestValidator : AbstractValidator<UpdateTrackStatusRequest>
{
    public UpdateTrackStatusRequestValidator()
    {
        RuleFor(r => r.Status).IsInEnum();
    }
}
