using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Kidzgo.API.ModelBinding;

public class DateTimeModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        Type modelType = context.Metadata.ModelType;
        if (modelType == typeof(DateTime) || modelType == typeof(DateTime?))
        {
            return new DateTimeModelBinder();
        }

        return null;
    }
}
