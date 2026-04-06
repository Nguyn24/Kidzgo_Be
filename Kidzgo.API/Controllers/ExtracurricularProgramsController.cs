using Kidzgo.API.Infrastructure;
using Kidzgo.API.Requests;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Programs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.API.Controllers;

[Route("api/extracurricular-programs")]
[ApiController]
[Authorize(Roles = "Admin,ManagementStaff")]
public class ExtracurricularProgramsController : ControllerBase
{
    private readonly IDbContext _context;

    public ExtracurricularProgramsController(IDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IResult> GetExtracurricularPrograms(CancellationToken cancellationToken)
    {
        var items = await _context.ExtracurricularPrograms
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.Date)
            .ThenBy(x => x.Name)
            .Select(x => new
            {
                id = x.Id,
                name = x.Name,
                type = x.Type,
                date = x.Date,
                capacity = x.Capacity,
                registeredCount = x.RegisteredCount,
                fee = x.Fee,
                location = x.Location,
                status = x.IsActive ? "Active" : "Inactive"
            })
            .ToListAsync(cancellationToken);

        return Results.Ok(ApiResult<object>.Success(items));
    }

    [HttpPost]
    public async Task<IResult> CreateExtracurricularProgram(
        [FromBody] CreateExtracurricularProgramRequest request,
        CancellationToken cancellationToken)
    {
        var entity = new ExtracurricularProgram
        {
            Id = Guid.NewGuid(),
            BranchId = request.BranchId,
            Name = request.Name,
            Type = request.Type,
            Date = request.Date,
            Capacity = request.Capacity,
            RegisteredCount = request.RegisteredCount,
            Fee = request.Fee,
            Location = request.Location,
            IsActive = request.IsActive,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.ExtracurricularPrograms.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return Results.Created($"/api/extracurricular-programs/{entity.Id}", ApiResult<object>.Success(new
        {
            id = entity.Id,
            name = entity.Name,
            type = entity.Type,
            date = entity.Date,
            capacity = entity.Capacity,
            registeredCount = entity.RegisteredCount,
            fee = entity.Fee,
            location = entity.Location,
            status = entity.IsActive ? "Active" : "Inactive"
        }));
    }

    [HttpPut("{id:guid}")]
    public async Task<IResult> UpdateExtracurricularProgram(
        Guid id,
        [FromBody] UpdateExtracurricularProgramRequest request,
        CancellationToken cancellationToken)
    {
        var entity = await _context.ExtracurricularPrograms.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (entity is null)
        {
            return Results.Problem(title: "ExtracurricularProgram", detail: "Extracurricular program not found", statusCode: StatusCodes.Status404NotFound);
        }

        entity.BranchId = request.BranchId;
        entity.Name = request.Name;
        entity.Type = request.Type;
        entity.Date = request.Date;
        entity.Capacity = request.Capacity;
        entity.RegisteredCount = request.RegisteredCount;
        entity.Fee = request.Fee;
        entity.Location = request.Location;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Results.Ok(ApiResult<object>.Success(new
        {
            id = entity.Id,
            name = entity.Name,
            type = entity.Type,
            date = entity.Date,
            capacity = entity.Capacity,
            registeredCount = entity.RegisteredCount,
            fee = entity.Fee,
            location = entity.Location,
            status = entity.IsActive ? "Active" : "Inactive"
        }));
    }
}
