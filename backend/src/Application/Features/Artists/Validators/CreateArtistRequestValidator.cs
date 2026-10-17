using Application.Features.Artists.Dtos;
using FluentValidation;

namespace Application.Features.Artists.Validators;

public sealed class CreateArtistRequestValidator : AbstractValidator<CreateArtistRequest>
{
    public CreateArtistRequestValidator()
    {
        RuleFor(r => r.UserId).NotEmpty().MaximumLength(450);
        RuleFor(r => r.Name).NotEmpty().MaximumLength(256);
        RuleFor(r => r.Country).MaximumLength(256);
    }
}
