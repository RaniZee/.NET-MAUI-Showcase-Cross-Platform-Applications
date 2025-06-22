using HelloMauiApp.Models;
using SQLite;

namespace HelloMauiApp.Services;

public class BmiRepository
{
    private SQLiteAsyncConnection _database;

    public BmiRepository()
    {
    }

    private async Task Init()
    {
        if (_database is not null)
            return;

        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "BmiHistory.db");
        _database = new SQLiteAsyncConnection(dbPath);
        await _database.CreateTableAsync<BmiResultRecord>();
    }

    public async Task<List<BmiResultRecord>> GetResultsAsync()
    {
        await Init();
        return await _database.Table<BmiResultRecord>().OrderByDescending(r => r.Date).ToListAsync();
    }

    public async Task SaveResultAsync(BmiResultRecord record)
    {
        await Init();
        await _database.InsertAsync(record);
    }
}