using Application.Features.Distributions.Dtos;
using FluentValidation;

namespace Application.Features.Distributions.Validators;

public sealed class DistributeTrackRequestValidator : AbstractValidator<DistributeTrackRequest>
{
    public DistributeTrackRequestValidator()
    {
        RuleFor(r => r.DSPIds).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(ids => ids.Distinct(StringComparer.OrdinalIgnoreCase).Count() == ids.Length)
            .WithMessage("Each DSP may only be selected once.");
        RuleForEach(r => r.DSPIds).NotEmpty().MaximumLength(450);
    }
}
