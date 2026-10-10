using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace SBRS.Application.Common.Behaviors
{
    // Application/Common/Behaviors/ValidationBehavior.cs
    public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        public async Task<TResponse> Handle(
            TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
        {
            if (validators.Any())
            {
                var context = new ValidationContext<TRequest>(request);
                var results = await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, ct)));
                var failures = results.SelectMany(r => r.Errors).Where(f => f != null).ToList();

                if (failures.Count != 0)
                    throw new ValidationException(failures);
            }

            return await next();   // continue to the next behavior or the handler
        }
    }
}
