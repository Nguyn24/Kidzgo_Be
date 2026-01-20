using System.Reflection;
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
                // Detect controller name if available
                var controllerName = context.ParameterInfo?.Member?.DeclaringType?.Name ?? string.Empty;

                var isClassController = controllerName.Contains("ClassController", StringComparison.OrdinalIgnoreCase);
                var isEnrollmentController = controllerName.Contains("EnrollmentController", StringComparison.OrdinalIgnoreCase);
                var isInvoiceController = controllerName.Contains("InvoiceController", StringComparison.OrdinalIgnoreCase);
                var isSessionController = controllerName.Contains("SessionController", StringComparison.OrdinalIgnoreCase);
                var isTicketController = controllerName.Contains("TicketController", StringComparison.OrdinalIgnoreCase);
                
                // Also check XML comment descriptions as fallback
                var hasClassStatusDescription = parameter.Description?.Contains("Class status", StringComparison.OrdinalIgnoreCase) == true ||
                                                parameter.Description?.Contains("Planned", StringComparison.OrdinalIgnoreCase) == true ||
                                                parameter.Description?.Contains("Active", StringComparison.OrdinalIgnoreCase) == true ||
                                                parameter.Description?.Contains("Closed", StringComparison.OrdinalIgnoreCase) == true;

                var hasEnrollmentStatusDescription = parameter.Description?.Contains("Enrollment status", StringComparison.OrdinalIgnoreCase) == true ||
                                                     parameter.Description?.Contains("Active, Paused, or Dropped", StringComparison.OrdinalIgnoreCase) == true;

                var hasInvoiceStatusDescription = parameter.Description?.Contains("Invoice status", StringComparison.OrdinalIgnoreCase) == true ||
                                                   parameter.Description?.Contains("Pending, Paid, Overdue, or Cancelled", StringComparison.OrdinalIgnoreCase) == true;

                var hasSessionStatusDescription = parameter.Description?.Contains("Session status", StringComparison.OrdinalIgnoreCase) == true ||
                                                  parameter.Description?.Contains("Scheduled, Completed, or Cancelled", StringComparison.OrdinalIgnoreCase) == true;

                var hasTicketStatusDescription = parameter.Description?.Contains("Ticket status", StringComparison.OrdinalIgnoreCase) == true ||
                                                 parameter.Description?.Contains("Open, InProgress, Resolved, or Closed", StringComparison.OrdinalIgnoreCase) == true;
                
                if (isClassController || hasClassStatusDescription)
                {
                    // This is ClassStatus enum - use string example
                    parameter.Example = new OpenApiString("Active");
                    // Don't override description if XML comment already set it
                    if (string.IsNullOrEmpty(parameter.Description))
                        parameter.Description = "Class status: Planned, Active, or Closed";
                }
                else if (isEnrollmentController || hasEnrollmentStatusDescription)
                {
                    // This is EnrollmentStatus enum - use string example
                    parameter.Example = new OpenApiString("Active");
                    if (string.IsNullOrEmpty(parameter.Description))
                        parameter.Description = "Enrollment status: Active, Paused, or Dropped";
                }
                else if (isInvoiceController || hasInvoiceStatusDescription)
                {
                    // This is InvoiceStatus enum - use string example
                    parameter.Example = new OpenApiString("Pending");
                    if (string.IsNullOrEmpty(parameter.Description))
                        parameter.Description = "Invoice status: Pending, Paid, Overdue, or Cancelled";
                }
                else if (isSessionController || hasSessionStatusDescription)
                {
                    // This is SessionStatus enum - use string example
                    parameter.Example = new OpenApiString("Scheduled");
                    if (string.IsNullOrEmpty(parameter.Description))
                        parameter.Description = "Session status: Scheduled, Completed, or Cancelled";
                }
                else if (isTicketController || hasTicketStatusDescription)
                {
                    // This is TicketStatus enum - use string example
                    parameter.Example = new OpenApiString("Open");
                    if (string.IsNullOrEmpty(parameter.Description))
                        parameter.Description = "Ticket status: Open, InProgress, Resolved, or Closed";
                }
                else if (string.IsNullOrEmpty(parameter.Description))
                {
                    // Default status (for other controllers like Payment, etc.)
                    parameter.Example = new OpenApiInteger(2);
                    parameter.Description = "Status: 1=Pending, 2=Success, 3=Failed";
                }
                break;
            case "category":
                // Detect controller name if available
                var categoryControllerName = context.ParameterInfo?.Member?.DeclaringType?.Name ?? string.Empty;
                var isTicketCategory = categoryControllerName.Contains("TicketController", StringComparison.OrdinalIgnoreCase) ||
                                       parameter.Description?.Contains("Ticket category", StringComparison.OrdinalIgnoreCase) == true ||
                                       parameter.Description?.Contains("Homework, Finance, Schedule, or Tech", StringComparison.OrdinalIgnoreCase) == true;

                if (isTicketCategory)
                {
                    parameter.Example = new OpenApiString("Homework");
                    if (string.IsNullOrEmpty(parameter.Description))
                        parameter.Description = "Ticket category: Homework, Finance, Schedule, or Tech";
                }
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

