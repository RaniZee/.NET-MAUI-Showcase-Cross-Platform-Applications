using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TaskTrackerMAUI.Models;

namespace TaskTrackerMAUI.Services
{
    public class SQLiteDataService : IDataService
    {
        private SQLiteAsyncConnection _database;
        private bool _initialized = false;

        private static string DatabasePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TaskTrackerSQLite.db3");

        private async Task Init()
        {
            if (_database != null && _initialized) return;

            _database = new SQLiteAsyncConnection(DatabasePath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);

            await _database.CreateTableAsync<TaskItem>();
            await _database.CreateTableAsync<Category>();    

            if (await _database.Table<Category>().CountAsync() == 0)
            {
                await _database.InsertAsync(new Category { Name = "Без категории" });
            }

            _initialized = true;
        }

        public async Task InitializeAsync() => await Init();

        public async Task<List<TaskItem>> GetAllTasksAsync()
        {
            await Init();
            var tasks = await _database.Table<TaskItem>().ToListAsync();
            var categories = await GetAllCategoriesAsync();
            var categoryDict = categories.ToDictionary(c => c.Id, c => c.Name);

            foreach (var task in tasks)
            {
                if (categoryDict.TryGetValue(task.CategoryId, out var categoryName))
                {
                    task.CategoryName = categoryName;
                }
                else
                {
                    task.CategoryName = "Без категории";  
                }
            }
            return tasks;
        }

        public async Task<TaskItem> GetTaskByIdAsync(int id)
        {
            await Init();
            var task = await _database.Table<TaskItem>().Where(i => i.Id == id).FirstOrDefaultAsync();
            if (task != null)
            {
                var category = await _database.Table<Category>().Where(c => c.Id == task.CategoryId).FirstOrDefaultAsync();
                task.CategoryName = category?.Name ?? "Без категории";
            }
            return task;
        }

        public async Task<int> SaveTaskAsync(TaskItem task)
        {
            await Init();
            if (task.Id != 0)
            {
                task.ModifiedDate = DateTime.Now;
                return await _database.UpdateAsync(task);
            }
            else
            {
                task.CreatedDate = DateTime.Now; task.ModifiedDate = DateTime.Now;
                return await _database.InsertAsync(task);
            }
        }

        public async Task<int> DeleteTaskAsync(TaskItem task)
        {
            await Init();
            return await _database.DeleteAsync(task);
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            await Init();
            return await _database.Table<Category>().OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<int> SaveCategoryAsync(Category category)
        {
            await Init();
            if (category.Id != 0) return await _database.UpdateAsync(category);
            else return await _database.InsertAsync(category);
        }

        public async Task<int> DeleteCategoryAsync(Category category)
        {
            await Init();
            return await _database.DeleteAsync(category);
        }
    }
}