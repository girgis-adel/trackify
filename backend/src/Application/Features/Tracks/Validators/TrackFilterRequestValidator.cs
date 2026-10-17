using Application.Features.Tracks.Dtos;
using FluentValidation;

namespace Application.Features.Tracks.Validators;

public sealed class TrackFilterRequestValidator : AbstractValidator<TrackFilterRequest>
{
    public TrackFilterRequestValidator()
    {
        RuleFor(r => r.ArtistId).Null()
            .WithMessage("Artist filtering is unavailable because tracks do not yet have an artist relationship.");
        RuleFor(r => r.Genre).NotEmpty().MaximumLength(128).When(r => r.Genre is not null);
        RuleFor(r => r.Status).IsInEnum().When(r => r.Status.HasValue);
    }
}
