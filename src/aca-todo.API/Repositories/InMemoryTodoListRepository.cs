using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace aca_todo.API.Repositories
{
    public class InMemoryTodoListRepository : ITodoListRepository
    {
        private static readonly Dictionary<Guid, TodoList> _todoLists = new();

        public Task<TodoList> GetTodoListAsync(Guid userId)
        {
            return Task.FromResult(_todoLists[userId].Clone());
        }

        public Task UpdateTodoListAsync(Guid userId, TodoList todoList)
        {
            _todoLists[userId] = todoList;
            return Task.CompletedTask;
        }
    }
}
