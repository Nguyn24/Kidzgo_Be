using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Kidzgo.API.Extensions;

public class SwaggerParameterFilter : IParameterFilter
{
    public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
    {
        // Only add examples if not already set, preserve XML comments descriptions
        if (parameter.Example != null) return;

        // Add examples based on parameter name
        switch (parameter.Name?.ToLower())
        {
            case "pagenumber":
                parameter.Example = new OpenApiInteger(1);
                if (string.IsNullOrEmpty(parameter.Description))
                    parameter.Description = "Page number (default: 1)";
                break;
            case "pagesize":
                parameter.Example = new OpenApiInteger(10);
                if (string.IsNullOrEmpty(parameter.Description))
                    parameter.Description = "Page size (default: 10)";
                break;
            case "searchterm":
                parameter.Example = new OpenApiString("example@email.com");
                if (string.IsNullOrEmpty(parameter.Description))
                    parameter.Description = "Search by username, email, or transaction code";
                break;
            case "status":
                parameter.Example = new OpenApiInteger(2);
                if (string.IsNullOrEmpty(parameter.Description))
                    parameter.Description = "Status: 1=Pending, 2=Success, 3=Failed";
                break;
            case "createdfrom":
            case "createdto":
            case "startdatefrom":
            case "startdateto":
                parameter.Example = new OpenApiString("2024-01-01");
                if (string.IsNullOrEmpty(parameter.Description))
                    parameter.Description = "Date format: YYYY-MM-DD";
                break;
            case "minamount":
                parameter.Example = new OpenApiDouble(1000);
                if (string.IsNullOrEmpty(parameter.Description))
                    parameter.Description = "Minimum amount (VND)";
                break;
            case "maxamount":
                parameter.Example = new OpenApiDouble(100000);
                if (string.IsNullOrEmpty(parameter.Description))
                    parameter.Description = "Maximum amount (VND)";
                break;
            case "sortby":
                parameter.Example = new OpenApiString("createdAt");
                if (string.IsNullOrEmpty(parameter.Description))
                    parameter.Description = "Sort field: amount, createdAt, status, paymentMethod, paymentType";
                break;
            case "sortorder":
                parameter.Example = new OpenApiInteger(1);
                if (string.IsNullOrEmpty(parameter.Description))
                    parameter.Description = "Sort order: 0=Ascending, 1=Descending (default: 1)";
                break;
            case "isactive":
                parameter.Example = new OpenApiBoolean(true);
                if (string.IsNullOrEmpty(parameter.Description))
                    parameter.Description = "Filter by active status (true/false)";
                break;
            case "paymenttype":
                parameter.Example = new OpenApiString("TOP_UP");
                if (string.IsNullOrEmpty(parameter.Description))
                    parameter.Description = "Filter by payment type: TOP_UP, MEMBERSHIP, or null (all)";
                break;
        }
    }
}

