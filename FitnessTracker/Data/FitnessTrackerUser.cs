using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace FitnessTracker.Data;

public class FitnessTrackerUser : IdentityUser
{
    public FitnessTrackerUser()
    {
        FitnessPlans = new HashSet<FitnessPlan>();
        FitnessInterests = new HashSet<FitnessInterest>();
    }

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    public ICollection<FitnessPlan> FitnessPlans { get; set; }

    public ICollection<FitnessInterest> FitnessInterests { get; set; }
}
