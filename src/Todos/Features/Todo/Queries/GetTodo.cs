﻿using BuildingBlocks.DomainEvents;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Todos.Infrastructure.Persistence;

namespace Todos.Features.Todo.Queries;

public class GetTodo : IFeatureModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/todos/{id}",
                async (Guid id, ISender mediator, CancellationToken cancellationToken) =>
                {
                    var request = new Query(id);

                    var result = await mediator.Send(request, cancellationToken);
                    return result is null ? Results.NotFound() : Results.Ok(result);
                })
            .WithName(nameof(GetTodo))
            .WithTags(nameof(Domain.Todo))
            .Produces<Response>()
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);
    }

    public sealed record Query(Guid TodoId) : IQuery<Response>, ICacheRequest
    {
        public string CacheKey => $"{nameof(Domain.Todo)}_{TodoId}";
        public DateTime? AbsoluteExpirationRelativeToNow { get; }
    }

    public sealed record Response(Guid Id, string Title, bool IsCompleted);

    internal class Handler(ReadOnlyTodoDbContext db) : IQueryHandler<Query, Response>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken = default)
        {
            return await db.Todos
                .AsNoTracking()
                .Where(t => t.Id == request.TodoId)
                .Select(td => new Response(td.Id, td.Title, td.Completed))
                .SingleOrDefaultAsync(cancellationToken);
        }
    }

    public class RequestValidator : AbstractValidator<Query>
    {
        public RequestValidator()
        {
            RuleFor(r => r.TodoId).NotEmpty();
        }
    }
}