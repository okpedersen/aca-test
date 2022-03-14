using System.Threading.Tasks;

namespace aca_todo.API.Repositories
{
    public class InMemoryTodoListRepository : ITodoListRepository
    {
        public static TodoList _todoList = new();

        public Task<TodoList> GetTodoListAsync()
        {
            return Task.FromResult(_todoList.Clone());
        }

        public Task UpdateTodoListAsync(TodoList todoList)
        {
            _todoList = todoList;
            return Task.CompletedTask;
        }
    }
}
