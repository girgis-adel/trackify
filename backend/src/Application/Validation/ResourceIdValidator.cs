using FluentValidation;

namespace Application.Validation;

public sealed class ResourceIdValidator : AbstractValidator<string>
{
    public ResourceIdValidator()
    {
        RuleFor(id => id).NotEmpty().MaximumLength(450).OverridePropertyName("Id");
    }
}
