using FluentValidation.Results;
using System;
using System.Linq;

namespace WorldescapeService.Core
{
	public static class ValidationResultExtensions
	{
		public static bool EnsureValidResult(this ValidationResult validationResult)
		{
			if (!validationResult.IsValid)
			{
				throw new Exception(string.Join(Environment.NewLine, validationResult.Errors.Select(x => x.ErrorMessage)));
			}
			else
			{
				return true;
			}
		}
	}
}
