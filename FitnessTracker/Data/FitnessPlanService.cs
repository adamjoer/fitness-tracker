using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FitnessTracker.Data;

public class FitnessPlanService
{
    private IMemoryCache MemoryCache { get; }
    private readonly IDbContextFactory<FitnessTrackerContext> _dbContextFactory;

    public FitnessPlanService(IDbContextFactory<FitnessTrackerContext> dbContextFactory, IMemoryCache memoryCache)
    {
        _dbContextFactory = dbContextFactory;
        MemoryCache = memoryCache;
    }

    public Task<List<FitnessPlan>> GetUsersFitnessPlans(string userId)
    {
        return MemoryCache.GetOrCreateAsync(userId, async e =>
        {
            e.SetOptions(new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5)
            });

            await using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.FitnessPlans
                .Include(plan => plan.WorkoutItems
                    .OrderBy(item => item.Index))
                .Where(plan => plan.UserId == userId)
                .ToListAsync();
        });
    }

    public Task<FitnessPlan?> GetFitnessPlan(string fitnessPlanId)
    {
        return MemoryCache.GetOrCreateAsync(fitnessPlanId, async e =>
        {
            e.SetOptions(new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5)
            });

            await using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.FitnessPlans
                .Include(plan => plan.WorkoutItems
                    .OrderBy(item => item.Index))
                .FirstOrDefaultAsync(plan => plan.Id == fitnessPlanId);
        });
    }

    public async Task AddFitnessPlan(FitnessPlan plan)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        context.FitnessPlans.Add(plan);
        await context.SaveChangesAsync();
    }

    public async Task AddWorkoutItemToPlan(FitnessPlan plan, WorkoutItem item)
    {
        item.FitnessPlanId = plan.Id!;

        var lastItem = plan.WorkoutItems.MaxBy(x => x.Index);
        item.Index = lastItem != null ? lastItem.Index + 1 : 0;

        await using (var context = await _dbContextFactory.CreateDbContextAsync())
        {
            context.WorkoutItems.Add(item);
            await context.SaveChangesAsync();
        }

        plan.WorkoutItems.Add(item);
    }

    public async Task UpdateWorkoutItem(WorkoutItem item)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        context.WorkoutItems.Update(item);
        await context.SaveChangesAsync();
    }

    public async Task MoveWorkoutItem(FitnessPlan plan, WorkoutItem item, bool moveUp)
    {
        if (plan.WorkoutItems.Count <= 1)
            return;

        var otherMovingItem = moveUp
            ? plan.WorkoutItems.TakeWhile(x => x.Index != item.Index).LastOrDefault()
            : plan.WorkoutItems.SkipWhile(x => x.Index != item.Index).Skip(1).FirstOrDefault();

        if (otherMovingItem == null)
            return;

        (item.Index, otherMovingItem.Index) = (otherMovingItem.Index, item.Index);

        await using var context = await _dbContextFactory.CreateDbContextAsync();
        context.WorkoutItems.Update(item);
        context.WorkoutItems.Update(otherMovingItem);
        await context.SaveChangesAsync();
    }

    public async Task RemoveWorkoutItem(WorkoutItem item)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        context.WorkoutItems.Remove(item);
        await context.SaveChangesAsync();
    }
}
