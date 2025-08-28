using PoligonMaui.Data;
using PoligonMaui.Models;
using PoligonMaui.Services.Interfaces;

namespace PoligonMaui.Services;

public class DatabaseService : IDatabaseService
{
    private readonly AppDbContext _dbContext;

    public DatabaseService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> InitializeDatabaseAsync()
    {
        try
        {
            await _dbContext.GetDatabaseAsync();
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Database initialization failed: {ex.Message}");
            return false;
        }
    }

    public async Task<List<Target>> GetAllTargetsAsync()
    {
        return await _dbContext.GetTargetsAsync();
    }

    public async Task<List<Target>> GetTargetsByGroupAsync(int groupId)
    {
        return await _dbContext.GetTargetsByGroupAsync(groupId);
    }

    public async Task<Target?> GetTargetAsync(int id)
    {
        return await _dbContext.GetTargetAsync(id);
    }

    public async Task<int> SaveTargetAsync(Target target)
    {
        return await _dbContext.SaveTargetAsync(target);
    }

    public async Task<int> DeleteTargetAsync(Target target)
    {
        return await _dbContext.DeleteTargetAsync(target);
    }

    public async Task<List<TargetGroup>> GetAllTargetGroupsAsync()
    {
        return await _dbContext.GetTargetGroupsAsync();
    }

    public async Task<TargetGroup?> GetTargetGroupAsync(int id)
    {
        return await _dbContext.GetTargetGroupAsync(id);
    }

    public async Task<int> SaveTargetGroupAsync(TargetGroup group)
    {
        return await _dbContext.SaveTargetGroupAsync(group);
    }

    public async Task<int> DeleteTargetGroupAsync(TargetGroup group)
    {
        return await _dbContext.DeleteTargetGroupAsync(group);
    }

    public async Task ResetAllTargetsAsync()
    {
        await _dbContext.ResetAllTargetsAsync();
    }
}