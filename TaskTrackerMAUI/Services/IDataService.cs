using System.Collections.Generic;
using System.Threading.Tasks;
using TaskTrackerMAUI.Models;

namespace TaskTrackerMAUI.Services
{
    public interface IDataService
    {
        Task InitializeAsync();

        Task<List<TaskItem>> GetAllTasksAsync();
        Task<TaskItem> GetTaskByIdAsync(int id);
        Task<int> SaveTaskAsync(TaskItem task);
        Task<int> DeleteTaskAsync(TaskItem task);

        Task<List<Category>> GetAllCategoriesAsync();
        Task<int> SaveCategoryAsync(Category category);
        Task<int> DeleteCategoryAsync(Category category);
    }
}