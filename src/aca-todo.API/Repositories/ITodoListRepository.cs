using System.Threading.Tasks;

namespace aca_todo.API.Repositories
{
    public interface ITodoListRepository
    {
        Task<TodoList> GetTodoListAsync();
        Task UpdateTodoListAsync(TodoList todoList);
    }
}

