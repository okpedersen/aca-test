using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using aca_todo.API.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace aca_todo.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TodosController : ControllerBase
    {
        private readonly ILogger<TodosController> _logger;
        private readonly ITodoListRepository _todoListRepository;

        public TodosController(ILogger<TodosController> logger, ITodoListRepository todoListRepository)
        {
            _logger = logger;
            _todoListRepository = todoListRepository;
        }


        [HttpPost]
        [Route("")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(AddTodoResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> AddTodo([FromBody] AddTodoRequest request)
        {
            var todoList = await _todoListRepository.GetTodoListAsync();
            var todoId = todoList.AddTodo(request.Description);
            await _todoListRepository.UpdateTodoListAsync(todoList);

            return Ok(new AddTodoResponse(Id: todoId));
        }

        [HttpGet]
        [Route("")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetTodosResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTodos()
        {
            var todoList = await _todoListRepository.GetTodoListAsync();
            var todoItems = todoList.GetTodoItems();
            _logger.LogInformation($"Length: {todoItems.Count}");
            return Ok(new GetTodosResponse(todoItems.Select(item => new TodoItem(item.Id, item.Description, item.Completed)).ToList()));
        }


        [HttpPut]
        [Route("{todoId}/complete")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdateTodo([FromRoute] Guid todoId)
        {
            var todoList = await _todoListRepository.GetTodoListAsync();

            try
            {
                await Task.Delay(100);
                todoList.CompleteTodo(todoId);
                await Task.Delay(100);
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e, "Got error");
                return NotFound($"Todo with id {todoId} is unrecognized");
            }

            await _todoListRepository.UpdateTodoListAsync(todoList);

            return NoContent();
        }

        [HttpDelete]
        [Route("{todoId}")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteTodo([FromRoute] Guid todoId)
        {
            var todoList = await _todoListRepository.GetTodoListAsync();

            try
            {
                await Task.Delay(100);
                todoList.DeleteTodo(todoId);
                await Task.Delay(100);
            }
            catch (ArgumentException)
            {
                return NotFound($"Todo with id {todoId} is unrecognized");
            }

            await _todoListRepository.UpdateTodoListAsync(todoList);

            return NoContent();
        }
    }

    public record AddTodoRequest(string Description);
    public record AddTodoResponse(Guid Id);

    public record TodoItem(Guid Id, string Description, bool Completed);
    public record GetTodosResponse(List<TodoItem> todoItems);
}
