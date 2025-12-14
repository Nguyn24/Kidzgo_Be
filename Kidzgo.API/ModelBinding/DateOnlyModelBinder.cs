using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Kidzgo.API.ModelBinding;

public class DateOnlyModelBinder : IModelBinder
{
    private static readonly string[] SupportedFormats = [
        "dd-MM-yyyy",
        "yyyy-MM-dd"
    ];

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null) throw new ArgumentNullException(nameof(bindingContext));

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

        if (DateOnly.TryParseExact(value, SupportedFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly date))
        {
            bindingContext.Result = ModelBindingResult.Success(date);
            return Task.CompletedTask;
        }

        bindingContext.ModelState.AddModelError(bindingContext.ModelName, "Invalid date format. Use dd-MM-yyyy or yyyy-MM-dd.");
        return Task.CompletedTask;
    }
}


