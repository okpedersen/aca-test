using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using aca_todo.API.Repositories;
using Dapr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace aca_todo.API.Controllers
{
    [ApiController]
    //[Route("/users/{userId}/[controller]")]
    [Route("/users")] // Route cannot contain parameter for Dapr pubsub
    public class TodosController : ControllerBase
    {
        private readonly ILogger<TodosController> _logger;
        private readonly ITodoListRepository _todoListRepository;
        private const int RETRIES = 20;

        public TodosController(ILogger<TodosController> logger, ITodoListRepository todoListRepository)
        {
            _logger = logger;
            _todoListRepository = todoListRepository;
        }

        [Topic("pubsub", "users")]
        [HttpPost]
        [Route("")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> AddNewList([FromBody] NewListRequest request)
        {
            var userId = request.UserId;
            _logger.LogInformation("New list");
            for (int i = 0; i < RETRIES; i++)
            {
                try
                {
                    _logger.LogInformation("New list");
                    var todoList = new TodoList();
                    await _todoListRepository.UpdateTodoListAsync(userId, todoList);
                    return NoContent();
                }
                catch (InvalidOperationException)
                { }
            }

            return new ConflictResult();
        }

        [HttpPost]
        [Route("{userId}/todos")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(AddTodoResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> AddTodo([FromRoute] Guid userId, [FromBody] AddTodoRequest request)
        {
            for (int i = 0; i < RETRIES; i++)
            {
                try
                {
                    var todoList = await _todoListRepository.GetTodoListAsync(userId);
                    var todoId = todoList.AddTodo(request.Description);
                    await _todoListRepository.UpdateTodoListAsync(userId, todoList);
                    return Ok(new AddTodoResponse(Id: todoId));
                }
                catch (InvalidOperationException)
                { }
            }

            return new ConflictResult();
        }

        [HttpGet]
        [Route("{userId}/todos")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetTodosResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTodos([FromRoute] Guid userId)
        {
            for (int i = 0; i < RETRIES; i++)
            {
                try
                {
                    var todoList = await _todoListRepository.GetTodoListAsync(userId);
                    var todoItems = todoList.GetTodoItems();
                    return Ok(new GetTodosResponse(todoItems.Select(item => new TodoItem(item.Id, item.Description, item.Completed)).ToList()));
                }
                catch (InvalidOperationException)
                { }
            }

            return new ConflictResult();
        }

        [HttpPut]
        [Route("{userId}/todos/{todoId}/complete")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdateTodo([FromRoute] Guid userId, [FromRoute] Guid todoId)
        {
            for (int i = 0; i < RETRIES; i++)
            {
                try
                {
                    var todoList = await _todoListRepository.GetTodoListAsync(userId);

                    try
                    {
                        todoList.CompleteTodo(todoId);
                    }
                    catch (ArgumentException e)
                    {
                        _logger.LogError(e, "Got error");
                        return NotFound($"Todo with id {todoId} is unrecognized");
                    }

                    await _todoListRepository.UpdateTodoListAsync(userId, todoList);

                    return NoContent();
                }
                catch (InvalidOperationException)
                { }
            }

            return new ConflictResult();
        }

        [HttpDelete]
        [Route("{userId}/todos/{todoId}")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteTodo([FromRoute] Guid userId, [FromRoute] Guid todoId)
        {
            for (int i = 0; i < RETRIES; i++)
            {
                try
                {
                    var todoList = await _todoListRepository.GetTodoListAsync(userId);

                    try
                    {
                        todoList.DeleteTodo(todoId);
                    }
                    catch (ArgumentException)
                    {
                        return NotFound($"Todo with id {todoId} is unrecognized");
                    }

                    await _todoListRepository.UpdateTodoListAsync(userId, todoList);

                    return NoContent();
                }
                catch (InvalidOperationException)
                { }
            }

            return new ConflictResult();
        }
    }

    public record NewListRequest(Guid UserId);

    public record AddTodoRequest(string Description);
    public record AddTodoResponse(Guid Id);

    public record TodoItem(Guid Id, string Description, bool Completed);
    public record GetTodosResponse(List<TodoItem> todoItems);
}
