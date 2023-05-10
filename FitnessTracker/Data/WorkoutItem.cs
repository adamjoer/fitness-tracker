using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessTracker.Data;

public class WorkoutItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string? Id { get; set; }

    [Required]
    [MaxLength(256)]
    public string Title { get; set; } = string.Empty;

    public int Index { get; set; }

    [Required]
    public bool IsCompleted { get; set; }

    public string FitnessPlanId { get; set; } = string.Empty;

    public FitnessPlan FitnessPlan { get; set; } = null!;
}
