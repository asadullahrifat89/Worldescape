using FluentValidation;
using Worldescape.Data;

namespace WorldescapeWebService.Core;

public class AddConstructCommandValidator : AbstractValidator<AddConstructCommand>
{
    public AddConstructCommandValidator()
    {
        RuleFor(x => x.Construct).NotNull().NotEmpty();
        RuleFor(x => x.Construct.Creator).NotNull().NotEmpty();
        RuleFor(x => x.Construct.World).NotNull().NotEmpty();

        RuleFor(x => x.Construct.Id).GreaterThan(0);
        RuleFor(x => x.Construct.Name).NotNull().NotEmpty();        

        RuleFor(x => x.Construct.Creator.Id).GreaterThan(0);
        RuleFor(x => x.Construct.Creator.Name).NotNull().NotEmpty();

        RuleFor(x => x.Construct.World.Id).GreaterThan(0);
        RuleFor(x => x.Construct.World.Name).NotNull().NotEmpty();
    }
}

