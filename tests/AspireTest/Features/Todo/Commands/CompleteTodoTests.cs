﻿using Microsoft.EntityFrameworkCore;

namespace AspireTest.Features.Todo.Commands;

public class CompleteTodoTestsShould(ApiServiceFixture given) : ApiTestBase(given)
{
    [Fact]
    public async Task response_ok_when_complete_one_todo()
    {
        // Arrange
        var todo = await Given.CreateDefaultTodo();

        // Act
        var response = await Given.TodoClient.PutAsync(ApiDefinition.V1.Todo.CompleteTodo(todo.Id), null);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        await Given.ExecuteDbContextAsync(async context =>
        {
            var dbTodo = await context.Todos.FirstOrDefaultAsync();
            dbTodo.Should().NotBeNull();
            dbTodo!.Completed.Should().Be(true);
        });
    }
}