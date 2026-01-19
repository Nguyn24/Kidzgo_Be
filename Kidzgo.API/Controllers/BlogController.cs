using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.Blogs.CreateBlog;
using Kidzgo.Application.Blogs.DeleteBlog;
using Kidzgo.Application.Blogs.GetBlogById;
using Kidzgo.Application.Blogs.GetBlogs;
using Kidzgo.Application.Blogs.GetPublishedBlogs;
using Kidzgo.Application.Blogs.PublishBlog;
using Kidzgo.Application.Blogs.UnpublishBlog;
using Kidzgo.Application.Blogs.UpdateBlog;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/blogs")]
[ApiController]
public class BlogController : ControllerBase
{
    private readonly ISender _mediator;

    public BlogController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// UC-349: Admin/Staff tạo Blog Post
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IResult> CreateBlog(
        [FromBody] CreateBlogRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateBlogCommand
        {
            Title = request.Title,
            Summary = request.Summary,
            Content = request.Content,
            FeaturedImageUrl = request.FeaturedImageUrl
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(b => $"/api/blogs/{b.Id}");
    }

    
    [HttpGet]
    [Authorize]
    public async Task<IResult> GetBlogs(
        [FromQuery] Guid? createdBy,
        [FromQuery] bool? isPublished,
        [FromQuery] bool? includeDeleted,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetBlogsQuery
        {
            CreatedBy = createdBy,
            IsPublished = isPublished,
            IncludeDeleted = includeDeleted,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

   
    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IResult> GetBlogById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetBlogByIdQuery
        {
            Id = id
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

 
    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IResult> UpdateBlog(
        Guid id,
        [FromBody] UpdateBlogRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateBlogCommand
        {
            Id = id,
            Title = request.Title,
            Summary = request.Summary,
            Content = request.Content,
            FeaturedImageUrl = request.FeaturedImageUrl
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

 
    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IResult> DeleteBlog(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteBlogCommand
        {
            Id = id
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-354: Publish Blog Post
    /// </summary>
    [HttpPatch("{id:guid}/publish")]
    [Authorize]
    public async Task<IResult> PublishBlog(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new PublishBlogCommand
        {
            Id = id
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-355: Unpublish Blog Post
    /// </summary>
    [HttpPatch("{id:guid}/unpublish")]
    [Authorize]
    public async Task<IResult> UnpublishBlog(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new UnpublishBlogCommand
        {
            Id = id
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-356: Hiển thị Blog Post trên Landing Page (Public endpoint - không cần auth)
    /// </summary>
    [HttpGet("published")]
    [AllowAnonymous]
    public async Task<IResult> GetPublishedBlogs(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPublishedBlogsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }
}

