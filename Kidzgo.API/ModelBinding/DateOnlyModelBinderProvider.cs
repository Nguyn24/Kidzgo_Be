using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Kidzgo.API.ModelBinding;

public class DateOnlyModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        Type modelType = context.Metadata.ModelType;
        if (modelType == typeof(DateOnly) || modelType == typeof(DateOnly?))
        {
            return new DateOnlyModelBinder();
        }

        return null;
    }
}


