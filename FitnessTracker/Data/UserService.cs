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

    public Task<string?> GetUserFullName(string id)
    {
        return MemoryCache.GetOrCreateAsync($"GetUserFullName:{id}", async e =>
        {
            e.SetOptions(new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5)
            });
            await using var context = await _dbContextFactory.CreateDbContextAsync();

            var user = await context.Users.Where(user => user.Id == id)
                .Select(user => new FitnessTrackerUser()
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                }).FirstOrDefaultAsync();

            return user == null ? null : $"{user.FirstName} {user.LastName}";
        });
    }
}
