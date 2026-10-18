using Application.Features.Tracks.Dtos;
using FluentValidation;

namespace Application.Features.Tracks.Validators;

public sealed class TrackFilterRequestValidator : AbstractValidator<TrackFilterRequest>
{
    public TrackFilterRequestValidator()
    {
        RuleFor(r => r.ArtistId).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(450)
            .Must(value => !string.IsNullOrWhiteSpace(value)).When(r => r.ArtistId is not null);
        RuleFor(r => r.Genre).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(128)
            .Must(value => !string.IsNullOrWhiteSpace(value)).When(r => r.Genre is not null);
        RuleFor(r => r.Status).IsInEnum().When(r => r.Status.HasValue);
    }
}
