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
        return MemoryCache.GetOrCreateAsync($"GetUserInterests{userId}", async e =>
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

    public Task<List<WorkoutType>> SearchForWorkoutType(string searchQuery)
    {
        var lowerCaseSearchQuery = searchQuery.ToLowerInvariant();

        return MemoryCache.GetOrCreateAsync(lowerCaseSearchQuery, async e =>
        {
            e.SetOptions(new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5)
            });

            await using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.WorkoutTypes
                .Where(type => type.Name.Contains(lowerCaseSearchQuery))
                .ToListAsync();
        });
    }

    public async Task<WorkoutType> AddWorkoutType(WorkoutType type)
    {
        type.Name = type.Name.ToLowerInvariant();
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var newWorkoutType = context.WorkoutTypes.Add(type);
        await context.SaveChangesAsync();
        return newWorkoutType.Entity;
    }

    public async Task<IEnumerable<WorkoutType>> AddWorkoutTypes(IEnumerable<WorkoutType> types)
    {
        var lowerCaseTypes = types.Select(type => new WorkoutType()
        {
            Name = type.Name.ToLowerInvariant()
        }).ToList();

        await using var context = await _dbContextFactory.CreateDbContextAsync();

        // FIXME: This is a pretty inefficient way to check for already added workout types.
        //        However, EF can't translate `.Where(type => lowerCaseTypes.Exists(x => x.Name == type.Name))`
        //        to a database operation, so for now we just query for all types, and check against that list.
        var alreadyExistingTypes = await context.WorkoutTypes.ToListAsync();
        var duplicateTypes = alreadyExistingTypes.Where(type => lowerCaseTypes.Exists(x => x.Name == type.Name)).ToList();
        lowerCaseTypes.RemoveAll(type => duplicateTypes.Any(x => x.Name == type.Name));

        context.AddRange(lowerCaseTypes);
        await context.SaveChangesAsync();
        return lowerCaseTypes.Concat(duplicateTypes);
    }
}
