using FluentValidation;
using SimpleApi.Models;

namespace SimpleApi.Validation;

public class CustomerValidator : AbstractValidator<Customer>
{
    public CustomerValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MinimumLength(2);
    }
}