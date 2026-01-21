using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Kidzgo.API.Extensions;

public sealed class SchedulePatternSchemaFilter : ISchemaFilter
{
    private const string ExamplePattern = "RRULE:FREQ=WEEKLY;BYDAY=MO,WE,FR;BYHOUR=18;BYMINUTE=0";

    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema?.Properties is not { Count: > 0 })
        {
            return;
        }

        if (!schema.Properties.TryGetValue("schedulePattern", out var scheduleProperty) &&
            !schema.Properties.TryGetValue("SchedulePattern", out scheduleProperty))
        {
            return;
        }

        scheduleProperty.Example ??= new OpenApiString(ExamplePattern);
    }
}
