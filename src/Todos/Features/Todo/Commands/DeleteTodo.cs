﻿using BuildingBlocks.DomainEvents;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Todos.Infrastructure.Persistence;

namespace Todos.Features.Todo.Commands;

public class DeleteTodo : IFeatureModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/todos/{id}",
                async (Guid id, ISender mediator, CancellationToken cancellationToken) =>
                {
                    var request = new Command
                    {
                        TodoId = id
                    };

                    var result = await mediator.Send(request, cancellationToken);

                    return result.Match(Results.NoContent, ApiResults.Problem);
                })
            .WithName(nameof(DeleteTodo))
            .WithTags(nameof(Domain.Todo))
            .Produces(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);
    }

    public sealed record Command : ICommand, IInvalidateCacheRequest
    {
        public Guid TodoId { get; set; }
        public string PrefixCacheKey => nameof(Domain.Todo);
    }

    internal class CommandHandler(TodoDbContext db) :  ICommandHandler<Command>
    {
        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var todo = await db.Todos
                .SingleOrDefaultAsync(x => x.Id == request.TodoId, cancellationToken);

            if (todo == null)
            {
                return Result.Success();
            }

            db.Todos.Remove(todo);
            await db.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(r => r.TodoId).NotEmpty();
        }
    }
}