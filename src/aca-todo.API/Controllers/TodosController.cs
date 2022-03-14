﻿using System;
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
        const int RETRIES = 20;

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
            for (int i = 0; i < RETRIES; i++)
            {
                try
                {
                    var todoList = await _todoListRepository.GetTodoListAsync();
                    var todoId = todoList.AddTodo(request.Description);
                    await _todoListRepository.UpdateTodoListAsync(todoList);
                    return Ok(new AddTodoResponse(Id: todoId));
                }
                catch (InvalidOperationException)
                { }
            }

            return new ConflictResult();

        }

        [HttpGet]
        [Route("")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetTodosResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTodos()
        {
            for (int i = 0; i < RETRIES; i++)
            {
                try
                {
                    var todoList = await _todoListRepository.GetTodoListAsync();
                    var todoItems = todoList.GetTodoItems();
                    _logger.LogInformation($"Length: {todoItems.Count}");
                    return Ok(new GetTodosResponse(todoItems.Select(item => new TodoItem(item.Id, item.Description, item.Completed)).ToList()));
                }
                catch (InvalidOperationException)
                { }
            }

            return new ConflictResult();

        }


        [HttpPut]
        [Route("{todoId}/complete")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdateTodo([FromRoute] Guid todoId)
        {
            for (int i = 0; i < RETRIES; i++)
            {
                try
                {
                    var todoList = await _todoListRepository.GetTodoListAsync();

                    try
                    {
                        todoList.CompleteTodo(todoId);
                    }
                    catch (ArgumentException e)
                    {
                        _logger.LogError(e, "Got error");
                        return NotFound($"Todo with id {todoId} is unrecognized");
                    }

                    await _todoListRepository.UpdateTodoListAsync(todoList);

                    return NoContent();
                }
                catch (InvalidOperationException)
                { }
            }

            return new ConflictResult();
        }

        [HttpDelete]
        [Route("{todoId}")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteTodo([FromRoute] Guid todoId)
        {
            for (int i = 0; i < RETRIES; i++)
            {
                try
                {
                    var todoList = await _todoListRepository.GetTodoListAsync();

                    try
                    {
                        todoList.DeleteTodo(todoId);
                    }
                    catch (ArgumentException)
                    {
                        return NotFound($"Todo with id {todoId} is unrecognized");
                    }

                    await _todoListRepository.UpdateTodoListAsync(todoList);

                    return NoContent();
                }
                catch (InvalidOperationException)
                { }
            }

            return new ConflictResult();
        }
    }

    public record AddTodoRequest(string Description);
    public record AddTodoResponse(Guid Id);

    public record TodoItem(Guid Id, string Description, bool Completed);
    public record GetTodosResponse(List<TodoItem> todoItems);
}
