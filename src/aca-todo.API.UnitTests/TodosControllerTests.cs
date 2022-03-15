using System;
using System.Linq;
using System.Threading.Tasks;
using aca_todo.API.Controllers;
using aca_todo.API.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace aca_todo.API.UnitTests
{
    public class TodosControllerTests
    {
        private readonly Guid USER_ID = Guid.Empty;

        [Fact]
        public async Task AddTodo_Should_AddTodoToList()
        {
            var repository = new InMemoryTodoListRepository();
            var todoController = new TodosController(new NullLogger<TodosController>(), repository);

            var addNewListResult = await todoController.AddNewList(USER_ID);
            Assert.IsType<NoContentResult>(addNewListResult);

            const string description = "Test todo";
            var todoRequest = new AddTodoRequest(description);
            var addTodoResult = await todoController.AddTodo(USER_ID, todoRequest);
            var addTodoOkResult = Assert.IsType<OkObjectResult>(addTodoResult);
            var addTodoResponse = Assert.IsType<AddTodoResponse>(addTodoOkResult.Value);
            var id = addTodoResponse.Id;

            var todosResult = await todoController.GetTodos(USER_ID);
            var todosOkResult = Assert.IsType<OkObjectResult>(todosResult);
            var todosResponse = Assert.IsType<GetTodosResponse>(todosOkResult.Value);

            var item = todosResponse.todoItems.First(item => item.Id == id);
            Assert.Equal(description, item.Description);
            Assert.False(item.Completed);
        }

        [Fact]
        public async Task CompleteTodo_Should_CompleteTodo()
        {
            var repository = new InMemoryTodoListRepository();
            var todoController = new TodosController(new NullLogger<TodosController>(), repository);

            var addNewListResult = await todoController.AddNewList(USER_ID);
            Assert.IsType<NoContentResult>(addNewListResult);

            const string description = "Test todo";
            var todoRequest = new AddTodoRequest(description);
            var addTodoResult = await todoController.AddTodo(USER_ID, todoRequest);
            var addTodoOkResult = Assert.IsType<OkObjectResult>(addTodoResult);
            var addTodoResponse = Assert.IsType<AddTodoResponse>(addTodoOkResult.Value);
            var id = addTodoResponse.Id;

            await todoController.UpdateTodo(USER_ID, id);

            var todosResult = await todoController.GetTodos(USER_ID);
            var todosOkResult = Assert.IsType<OkObjectResult>(todosResult);
            var todosResponse = Assert.IsType<GetTodosResponse>(todosOkResult.Value);

            var item = todosResponse.todoItems.First(item => item.Id == id);
            Assert.Equal(description, item.Description);
            Assert.True(item.Completed);
        }

        [Fact]
        public async Task DeleteTodo_Should_DeleteTodo()
        {
            var repository = new InMemoryTodoListRepository();
            var todoController = new TodosController(new NullLogger<TodosController>(), repository);

            var addNewListResult = await todoController.AddNewList(USER_ID);
            Assert.IsType<NoContentResult>(addNewListResult);

            const string description = "Test todo";
            var todoRequest = new AddTodoRequest(description);
            var addTodoResult = await todoController.AddTodo(USER_ID, todoRequest);
            var addTodoOkResult = Assert.IsType<OkObjectResult>(addTodoResult);
            var addTodoResponse = Assert.IsType<AddTodoResponse>(addTodoOkResult.Value);
            var id = addTodoResponse.Id;

            await todoController.DeleteTodo(USER_ID, id);

            var todosResult = await todoController.GetTodos(USER_ID);
            var todosOkResult = Assert.IsType<OkObjectResult>(todosResult);
            var todosResponse = Assert.IsType<GetTodosResponse>(todosOkResult.Value);

            var item = todosResponse.todoItems.FirstOrDefault(item => item.Id == id);
            Assert.Null(item);
        }
    }
}
