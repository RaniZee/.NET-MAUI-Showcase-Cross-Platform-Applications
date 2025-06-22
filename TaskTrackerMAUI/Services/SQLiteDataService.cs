using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TaskTrackerMAUI.Models;

namespace TaskTrackerMAUI.Services
{
    public class SQLiteDataService : IDataService
    {
        private SQLiteAsyncConnection _database;
        private bool _initialized = false;

        private static string DatabasePath
        {
            get
            {
                var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(basePath, "TaskTrackerSQLite.db3");
            }
        }

        public SQLiteDataService()
        {
        }

        private async Task Init()
        {
            if (_database != null && _initialized)
                return;

            _database = new SQLiteAsyncConnection(DatabasePath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);

            await _database.CreateTableAsync<TaskItem>();
            _initialized = true;
            System.Diagnostics.Debug.WriteLine($"[DEBUG] SQLite Database Initialized. Path: {DatabasePath}");
        }

        public async Task InitializeAsync()
        {
            await Init();
        }

        public async Task<List<TaskItem>> GetAllTasksAsync()
        {
            await Init();
            return await _database.Table<TaskItem>().ToListAsync();
        }

        public async Task<TaskItem> GetTaskByIdAsync(int id)
        {
            await Init();
            return await _database.Table<TaskItem>().Where(i => i.Id == id).FirstOrDefaultAsync();
        }

        public async Task<int> SaveTaskAsync(TaskItem task)
        {
            await Init();
            if (task.Id != 0)           
            {
                task.ModifiedDate = DateTime.Now;
                await _database.UpdateAsync(task);
                System.Diagnostics.Debug.WriteLine($"[DEBUG] SQLite: Updated Task ID: {task.Id}, Title: {task.Title}");
                return task.Id;
            }
            else     
            {
                task.CreatedDate = DateTime.Now;
                task.ModifiedDate = DateTime.Now;     
                await _database.InsertAsync(task);
                System.Diagnostics.Debug.WriteLine($"[DEBUG] SQLite: Inserted New Task. Assigned ID: {task.Id}, Title: {task.Title}");      
                return task.Id;         
            }
        }

        public async Task<int> DeleteTaskAsync(TaskItem task)
        {
            await Init();
            int result = await _database.DeleteAsync(task);
            System.Diagnostics.Debug.WriteLine($"[DEBUG] SQLite: Deleted Task ID: {task.Id}. Result: {result}");
            return result;    
        }
    }
}