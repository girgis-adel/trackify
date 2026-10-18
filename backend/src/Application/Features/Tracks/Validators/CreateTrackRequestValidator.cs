using Application.Features.Tracks.Dtos;
using FluentValidation;
using System.Text.RegularExpressions;

namespace Application.Features.Tracks.Validators;

public sealed class CreateTrackRequestValidator : AbstractValidator<CreateTrackRequest>
{
    public CreateTrackRequestValidator()
    {
        RuleFor(r => r.ArtistId).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(450)
            .Must(value => !string.IsNullOrWhiteSpace(value)).WithMessage("An artist is required.");
        RuleFor(r => r.Isrc).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(12)
            .Must(value => Regex.IsMatch(value.Trim(), "^[A-Za-z]{2}[A-Za-z0-9]{3}[0-9]{7}$"))
            .WithMessage("ISRC must contain 12 letters and digits in the standard format.");
        RuleFor(r => r.Title).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(256)
            .Must(value => !string.IsNullOrWhiteSpace(value)).WithMessage("A title is required.");
        RuleFor(r => r.Genre).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(128)
            .Must(value => !string.IsNullOrWhiteSpace(value)).WithMessage("A genre is required.");
    }
}
