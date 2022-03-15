using System;
using System.Threading.Tasks;

namespace aca_todo.API.Repositories
{
    public interface ITodoListRepository
    {
        Task<TodoList> GetTodoListAsync(Guid userId);
        Task UpdateTodoListAsync(Guid userId, TodoList todoList);
    }
}
