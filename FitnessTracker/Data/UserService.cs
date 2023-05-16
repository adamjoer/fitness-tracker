using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FitnessTracker.Data;

public class UserService
{
    private readonly IDbContextFactory<FitnessTrackerContext> _dbContextFactory;
    private IMemoryCache MemoryCache { get; }

    public UserService(IDbContextFactory<FitnessTrackerContext> dbContextFactory, IMemoryCache memoryCache)
    {
        _dbContextFactory = dbContextFactory;
        MemoryCache = memoryCache;
    }

    public Task<FitnessTrackerUser?> GetUser(string id)
    {
        return MemoryCache.GetOrCreateAsync($"GetUser:{id}", async e =>
        {
            e.SetOptions(new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5)
            });

            await using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.Users.FindAsync(id);
        });
    }
}
