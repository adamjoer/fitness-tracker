using Microsoft.EntityFrameworkCore;

namespace FitnessTracker.Data;

public class FitnessInterestService
{
    private readonly IDbContextFactory<FitnessTrackerContext> _dbContextFactory;

    public FitnessInterestService(IDbContextFactory<FitnessTrackerContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<List<FitnessInterest>> GetUserInterests(string userId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.FitnessInterests
            .Include(interest => interest.Type)
            .Where(interest => interest.UserId == userId)
            .ToListAsync();
    }

    public async Task AddUserInterest(FitnessInterest interest)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        context.FitnessInterests.Add(interest);
        await context.SaveChangesAsync();
    }

    public async Task UpdateUserInterest(FitnessInterest interest)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        context.FitnessInterests.Update(interest);
        await context.SaveChangesAsync();
    }

    public async Task<List<WorkoutType>> SearchForWorkoutType(string searchQuery)
    {
        var lowerCaseSearchQuery = searchQuery.ToLowerInvariant();

        await using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.WorkoutTypes
            .Where(type => type.Name.Contains(lowerCaseSearchQuery))
            .ToListAsync();
    }

    public async Task AddWorkoutType(WorkoutType type)
    {
        type.Name = type.Name.ToLowerInvariant();
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        context.WorkoutTypes.Add(type);
        await context.SaveChangesAsync();
    }
}
