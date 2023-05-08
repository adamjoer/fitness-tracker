using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FitnessTracker.Data;

public class FitnessInterestService
{
    private readonly IDbContextFactory<FitnessTrackerContext> _dbContextFactory;
    private IMemoryCache MemoryCache { get; }

    public FitnessInterestService(IDbContextFactory<FitnessTrackerContext> dbContextFactory, IMemoryCache memoryCache)
    {
        _dbContextFactory = dbContextFactory;
        MemoryCache = memoryCache;
    }

    public Task<List<FitnessInterest>> GetUserInterests(string userId)
    {
        return MemoryCache.GetOrCreateAsync(userId, async e =>
        {
            e.SetOptions(new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5)
            });

            await using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.FitnessInterests
                .Include(interest => interest.Type)
                .Where(interest => interest.UserId == userId)
                .ToListAsync();
        });
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

    public Task<List<WorkoutType>> SearchForWorkoutType(string searchQuery,
        List<WorkoutType>? exclusionList = null)
    {
        var lowerCaseSearchQuery = searchQuery.ToLowerInvariant();

        return MemoryCache.GetOrCreateAsync(lowerCaseSearchQuery, async e =>
        {
            e.SetOptions(new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5)
            });

            await using var context = await _dbContextFactory.CreateDbContextAsync();

            if (exclusionList != null)
                return await context.WorkoutTypes
                    .Where(type => !exclusionList.Contains(type))
                    .Where(type => type.Name.Contains(lowerCaseSearchQuery))
                    .ToListAsync();

            return await context.WorkoutTypes
                .Where(type => type.Name.Contains(lowerCaseSearchQuery))
                .ToListAsync();
        });
    }

    public async Task AddWorkoutType(WorkoutType type)
    {
        type.Name = type.Name.ToLowerInvariant();
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        context.WorkoutTypes.Add(type);
        await context.SaveChangesAsync();
    }
}
