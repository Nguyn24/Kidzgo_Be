using Kidzgo.Application.Time;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Kidzgo.API.ModelBinding;

public class DateTimeModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        string? value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).FirstValue;
        bool isNullable = bindingContext.ModelMetadata.IsReferenceOrNullableType;

        if (string.IsNullOrWhiteSpace(value))
        {
            if (isNullable)
            {
                bindingContext.Result = ModelBindingResult.Success(null);
            }

            return Task.CompletedTask;
        }

        if (VietnamTime.TryParseApiDateTime(value, out var utcValue))
        {
            bindingContext.Result = ModelBindingResult.Success(utcValue);
            return Task.CompletedTask;
        }

        bindingContext.ModelState.AddModelError(
            bindingContext.ModelName,
            "Invalid date-time format. Use ISO 8601 with offset, e.g. 2026-03-24T22:22:24+07:00.");

        return Task.CompletedTask;
    }
}
