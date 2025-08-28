using SQLite;
using PoligonMaui.Models;

namespace PoligonMaui.Data;

public class AppDbContext
{
    private SQLiteAsyncConnection? _database;

    public async Task<SQLiteAsyncConnection> GetDatabaseAsync()
    {
        if (_database != null)
            return _database;

        var databasePath = Path.Combine(FileSystem.AppDataDirectory, "PoligonMaui.db3");
        
        _database = new SQLiteAsyncConnection(databasePath, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.SharedCache);

        // Create tables
        await _database.CreateTableAsync<Target>();
        await _database.CreateTableAsync<TargetGroup>();

        return _database;
    }

    public async Task<List<Target>> GetTargetsAsync()
    {
        var db = await GetDatabaseAsync();
        return await db.Table<Target>().ToListAsync();
    }

    public async Task<List<Target>> GetTargetsByGroupAsync(int groupId)
    {
        var db = await GetDatabaseAsync();
        return await db.Table<Target>().Where(t => t.TargetGroupId == groupId).ToListAsync();
    }

    public async Task<Target?> GetTargetAsync(int id)
    {
        var db = await GetDatabaseAsync();
        return await db.Table<Target>().Where(t => t.Id == id).FirstOrDefaultAsync();
    }

    public async Task<int> SaveTargetAsync(Target target)
    {
        var db = await GetDatabaseAsync();
        if (target.Id != 0)
            return await db.UpdateAsync(target);
        else
            return await db.InsertAsync(target);
    }

    public async Task<int> DeleteTargetAsync(Target target)
    {
        var db = await GetDatabaseAsync();
        return await db.DeleteAsync(target);
    }

    public async Task<List<TargetGroup>> GetTargetGroupsAsync()
    {
        var db = await GetDatabaseAsync();
        return await db.Table<TargetGroup>().ToListAsync();
    }

    public async Task<TargetGroup?> GetTargetGroupAsync(int id)
    {
        var db = await GetDatabaseAsync();
        var group = await db.Table<TargetGroup>().Where(g => g.Id == id).FirstOrDefaultAsync();
        if (group != null)
        {
            group.Targets = await GetTargetsByGroupAsync(group.Id);
        }
        return group;
    }

    public async Task<int> SaveTargetGroupAsync(TargetGroup group)
    {
        var db = await GetDatabaseAsync();
        if (group.Id != 0)
            return await db.UpdateAsync(group);
        else
            return await db.InsertAsync(group);
    }

    public async Task<int> DeleteTargetGroupAsync(TargetGroup group)
    {
        var db = await GetDatabaseAsync();
        
        // Delete all targets in this group first
        var targets = await GetTargetsByGroupAsync(group.Id);
        foreach (var target in targets)
        {
            await db.DeleteAsync(target);
        }
        
        return await db.DeleteAsync(group);
    }

    public async Task ResetAllTargetsAsync()
    {
        var db = await GetDatabaseAsync();
        await db.ExecuteAsync("UPDATE Target SET IsReached = 0");
    }
}