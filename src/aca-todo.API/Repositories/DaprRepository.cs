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
        const string DAPR_STORE_NAME = "statestore";
        private readonly ILogger<DaprRepository> _logger;
        private readonly DaprClient _daprClient;

        private record TodoItemDto(Guid Id, string Description, bool Completed);
        private record TodoListDto(List<TodoItemDto> TodoItems);

        public DaprRepository(ILogger<DaprRepository> logger, DaprClient daprClient)
        {
            _logger = logger;
            _daprClient = daprClient;
        }

        public async Task<TodoList> GetTodoListAsync()
        {
            var dto = await _daprClient.GetStateAsync<TodoListDto?>(storeName: DAPR_STORE_NAME, key: "todos");
            if (dto is null || dto.TodoItems is null) // TODO: Workaround for first time init
            {
                return new TodoList();
            }
            return new TodoList(dto.TodoItems.Select(item => new TodoItem(item.Id, item.Description, item.Completed)).ToList());
        }

        public async Task UpdateTodoListAsync(TodoList todoList)
        {
            var dto = new TodoListDto(TodoItems: todoList.GetTodoItems().Select(item => new TodoItemDto(item.Id, item.Description, item.Completed)).ToList());
            await _daprClient.SaveStateAsync<TodoListDto>(storeName: DAPR_STORE_NAME, key: "todos", value: dto);
        }
    }

}
