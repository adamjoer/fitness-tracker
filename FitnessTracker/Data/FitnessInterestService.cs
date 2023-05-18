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

    public async Task AddUserInterestTypes(string userId, IEnumerable<WorkoutType> types)
    {
        var newInterests = types.Select(type => new FitnessInterest()
        {
            UserId = userId,
            TypeId = type.Id!,
            Intensity = 1,
        });

        await using var context = await _dbContextFactory.CreateDbContextAsync();
        context.FitnessInterests.AddRange(newInterests);
        await context.SaveChangesAsync();
    }

    public async Task UpdateUserInterest(FitnessInterest interest)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        context.FitnessInterests.Update(new FitnessInterest()
        {
            Id = interest.Id,
            UserId = interest.UserId,
            TypeId = interest.TypeId,
            Intensity = interest.Intensity,
        });
        await context.SaveChangesAsync();
    }

    public async Task RemoveUserInterest(FitnessInterest interest)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        context.FitnessInterests.Remove(interest);
        await context.SaveChangesAsync();
    }

    public async Task RemoveUserInterests(IEnumerable<FitnessInterest> interests)
    {
        interests = interests.Select(interest => new FitnessInterest()
        {
            Id = interest.Id,
            TypeId = interest.TypeId,
            UserId = interest.UserId,
            Intensity = interest.Intensity,
        });

        await using var context = await _dbContextFactory.CreateDbContextAsync();
        context.FitnessInterests.RemoveRange(interests);
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
        var duplicateTypes = alreadyExistingTypes.Where(type => lowerCaseTypes.Exists(x => x.Name == type.Name))
            .ToList();
        lowerCaseTypes.RemoveAll(type => duplicateTypes.Any(x => x.Name == type.Name));

        context.AddRange(lowerCaseTypes);
        await context.SaveChangesAsync();
        return lowerCaseTypes.Concat(duplicateTypes);
    }
}
