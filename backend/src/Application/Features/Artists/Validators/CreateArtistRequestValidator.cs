using Application.Features.Artists.Dtos;
using FluentValidation;

namespace Application.Features.Artists.Validators;

public sealed class CreateArtistRequestValidator : AbstractValidator<CreateArtistRequest>
{
    public CreateArtistRequestValidator()
    {
        RuleFor(r => r.UserName).Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("A user name is required.")
            .MaximumLength(256)
            .Matches("^[a-zA-Z0-9._@+-]+$")
            .WithMessage("Use only letters, numbers, and -._@+ in the user name.");
        RuleFor(r => r.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(r => r.Password).NotEmpty().MinimumLength(8);
        RuleFor(r => r.Name).Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("An artist name is required.")
            .MaximumLength(256);
        RuleFor(r => r.Country).MaximumLength(256);
    }
}
