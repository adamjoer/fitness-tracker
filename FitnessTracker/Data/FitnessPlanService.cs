using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FitnessTracker.Data;

public class FitnessPlanService
{
    private readonly IDbContextFactory<FitnessTrackerContext> _dbContextFactory;
    private IMemoryCache MemoryCache { get; }

    public FitnessPlanService(IDbContextFactory<FitnessTrackerContext> dbContextFactory, IMemoryCache memoryCache)
    {
        _dbContextFactory = dbContextFactory;
        MemoryCache = memoryCache;
    }

    public Task<List<FitnessPlan>> GetUsersFitnessPlans(string userId)
    {
        return MemoryCache.GetOrCreateAsync($"{typeof(List<FitnessPlan>)}{userId}", async e =>
        {
            e.SetOptions(new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5)
            });

            await using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.FitnessPlans
                .Include(plan => plan.WorkoutItems
                    .OrderBy(item => item.Index))
                .Include(plan => plan.WorkoutTypeTags
                    .OrderBy(tag => tag.Type.Name))
                .ThenInclude(tag => tag.Type)
                .Where(plan => plan.UserId == userId)
                .AsSplitQuery()
                .OrderByDescending(plan => plan.Date)
                .ToListAsync();
        });
    }

    public Task<FitnessPlan?> GetFitnessPlan(string fitnessPlanId)
    {
        return MemoryCache.GetOrCreateAsync($"{typeof(FitnessPlan)}{fitnessPlanId}", async e =>
        {
            e.SetOptions(new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5)
            });

            await using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.FitnessPlans
                .Include(plan => plan.WorkoutItems
                    .OrderBy(item => item.Index))
                .Include(plan => plan.WorkoutTypeTags
                    .OrderBy(tag => tag.Type.Name))
                .ThenInclude(tag => tag.Type)
                .AsSplitQuery()
                .FirstOrDefaultAsync(plan => plan.Id == fitnessPlanId);
        });
    }

    public async Task<FitnessPlan> AddFitnessPlan(FitnessPlan plan)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var newFitnessPlan = context.FitnessPlans.Add(plan);
        await context.SaveChangesAsync();
        return newFitnessPlan.Entity;
    }

    public async Task UpdateFitnessPlan(FitnessPlan plan)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        context.FitnessPlans.Update(plan);
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
        context.WorkoutItems.Update(new WorkoutItem
        {
            Id = item.Id,
            Title = item.Title,
            Index = item.Index,
            IsCompleted = item.IsCompleted,
            FitnessPlanId = item.FitnessPlanId,
        });
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
        context.WorkoutItems.Update(new WorkoutItem
        {
            Id = item.Id,
            Title = item.Title,
            Index = item.Index,
            IsCompleted = item.IsCompleted,
            FitnessPlanId = item.FitnessPlanId,
        });
        context.WorkoutItems.Update(new WorkoutItem
        {
            Id = otherMovingItem.Id,
            Title = otherMovingItem.Title,
            Index = otherMovingItem.Index,
            IsCompleted = otherMovingItem.IsCompleted,
            FitnessPlanId = otherMovingItem.FitnessPlanId,
        });
        await context.SaveChangesAsync();
    }

    public async Task<List<WorkoutTypeTag>> GetWorkoutTypeTagsForPlan(FitnessPlan plan)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.WorkoutTypeTags.Where(tag => tag.PlanId == plan.Id).ToListAsync();
    }

    public async Task RemoveWorkoutItem(WorkoutItem item)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        context.WorkoutItems.Remove(item);
        await context.SaveChangesAsync();
    }

    public async Task AddWorkoutTypeTagsToPlan(FitnessPlan plan, IEnumerable<WorkoutType> workoutTypes)
    {
        var workoutTypeTags = workoutTypes.Select(type => new WorkoutTypeTag
        {
            TypeId = type.Id!,
            PlanId = plan.Id!
        });

        await using var context = await _dbContextFactory.CreateDbContextAsync();
        context.WorkoutTypeTags.AddRange(workoutTypeTags);
        await context.SaveChangesAsync();
    }

    public async Task RemoveWorkoutTypeTag(WorkoutTypeTag tag)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        context.WorkoutTypeTags.Remove(tag);
        await context.SaveChangesAsync();
    }

    public async Task RemoveWorkoutTypeTags(IEnumerable<WorkoutTypeTag> tags)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        context.WorkoutTypeTags.RemoveRange(tags);
        await context.SaveChangesAsync();
    }
}
