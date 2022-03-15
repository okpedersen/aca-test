using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapr.Client;
using Microsoft.Extensions.Logging;

namespace aca_todo.API.Repositories
{
    public class DaprRepository : ITodoListRepository
    {
        private const string DAPR_STORE_NAME = "statestore";
        private readonly ILogger<DaprRepository> _logger;
        private readonly DaprClient _daprClient;

        private string? _etag;

        private record TodoItemDto(Guid Id, string Description, bool Completed);
        private record TodoListDto(List<TodoItemDto> TodoItems);

        public DaprRepository(ILogger<DaprRepository> logger, DaprClient daprClient)
        {
            _logger = logger;
            _daprClient = daprClient;
        }

        public async Task<TodoList> GetTodoListAsync(Guid userId)
        {
            (TodoListDto? dto, string? etag) = await _daprClient.GetStateAndETagAsync<TodoListDto?>(storeName: DAPR_STORE_NAME, key: $"todos-{userId}");
            _etag = etag;
            if (dto is null || dto.TodoItems is null) // TODO: Workaround for first time init
            {
                throw new InvalidOperationException();
            }
            return new TodoList(dto.TodoItems.ConvertAll(item => new TodoItem(item.Id, item.Description, item.Completed)));
        }

        public async Task UpdateTodoListAsync(Guid userId, TodoList todoList)
        {
            var dto = new TodoListDto(TodoItems: todoList.GetTodoItems().Select(item => new TodoItemDto(item.Id, item.Description, item.Completed)).ToList());

            if (_etag != null)
            {
                var success = await _daprClient.TrySaveStateAsync<TodoListDto>(storeName: DAPR_STORE_NAME, etag: _etag, key: $"todos-{userId}", value: dto);
                if (!success)
                {
                    throw new InvalidOperationException();
                }
            }
            else
            {
                await _daprClient.SaveStateAsync(storeName: DAPR_STORE_NAME, key: $"todos-{userId}", value: dto);
            }
        }
    }

}
