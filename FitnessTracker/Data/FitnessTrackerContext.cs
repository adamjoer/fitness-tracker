using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FitnessTracker.Data
{
    public class FitnessTrackerContext : IdentityDbContext<FitnessTrackerUser>
    {
        public FitnessTrackerContext(DbContextOptions<FitnessTrackerContext> options)
            : base(options)
        {
        }

        public DbSet<FitnessPlan> FitnessPlans { get; set; } = null!;

        public DbSet<WorkoutItem> WorkoutItems { get; set; } = null!;
    }
}
