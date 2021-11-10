﻿using FluentValidation;

namespace WorldescapeService.Core
{
    public class SaveUserCommandValidator : AbstractValidator<SaveUserCommand>
    {
		public SaveUserCommandValidator()
		{
			RuleFor(x => x.Name).NotNull().NotEmpty();
			RuleFor(x => x.Email).NotNull().NotEmpty();
			RuleFor(x => x.Phone).NotNull().NotEmpty();
			RuleFor(x => x.Password).NotNull().NotEmpty();
			RuleFor(x => x.ImageUrl).NotNull().NotEmpty();
		}
	}
}
