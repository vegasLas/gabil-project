using PoligonMaui.Models;

namespace PoligonMaui.Services.Interfaces;

public interface IDatabaseService
{
    Task<List<Target>> GetAllTargetsAsync();
    Task<List<Target>> GetTargetsByGroupAsync(int groupId);
    Task<Target?> GetTargetAsync(int id);
    Task<int> SaveTargetAsync(Target target);
    Task<int> DeleteTargetAsync(Target target);
    
    Task<List<TargetGroup>> GetAllTargetGroupsAsync();
    Task<TargetGroup?> GetTargetGroupAsync(int id);
    Task<int> SaveTargetGroupAsync(TargetGroup group);
    Task<int> DeleteTargetGroupAsync(TargetGroup group);
    
    Task ResetAllTargetsAsync();
    Task<bool> InitializeDatabaseAsync();
}