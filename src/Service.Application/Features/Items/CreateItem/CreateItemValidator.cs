using FluentValidation;

namespace Service.Application.Features.Items.CreateItem;

internal sealed class CreateItemValidator : AbstractValidator<CreateItemCommand>
{
    public CreateItemValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome do item é obrigatório.")
            .MaximumLength(150).WithMessage("O nome do item deve ter no máximo 150 caracteres.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("O preço deve ser maior que zero.");
    }
}