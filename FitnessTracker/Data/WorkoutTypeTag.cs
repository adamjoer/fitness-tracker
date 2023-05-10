using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessTracker.Data;

public class WorkoutTypeTag
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string? Id { get; set; }

    public string PlanId { get; set; } = string.Empty;

    public virtual FitnessPlan Plan { get; set; } = null!;

    public string TypeId { get; set; } = string.Empty;

    public virtual WorkoutType Type { get; set; } = null!;
}
