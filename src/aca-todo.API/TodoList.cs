using System;
using System.Collections.Generic;

namespace aca_todo.API
{
    public class TodoList
    {
        private readonly List<TodoItem> _todoItems;

        public TodoList()
        {
            _todoItems = new();
        }

        public TodoList(List<TodoItem> todoItems)
        {
            _todoItems = todoItems;
        }

        public IReadOnlyList<TodoItem> GetTodoItems()
        {
            return _todoItems;
        }


        public Guid AddTodo(string description)
        {
            var todoItem = new TodoItem(description);
            _todoItems.Add(todoItem);
            return todoItem.Id;
        }

        public void CompleteTodo(Guid todoId)
        {
            var index = _todoItems.FindIndex(item => item.Id == todoId);
            if (index == -1)
            {
              throw new ArgumentException("Invalid todoId");
            }
            _todoItems[index].Complete();
        }

    }

    public class TodoItem
    {

        public TodoItem(Guid id, string description, bool completed)
        {
            Id = id;
            Description = description;
            Completed = completed;
        }

        public TodoItem(string description)
          : this(Guid.NewGuid(), description, false)
        {
        }

        public Guid Id { get; }
        public string Description { get; }
        public bool Completed { get; private set; }

        public void Complete()
        {
            Completed = true;
        }
    }
}
