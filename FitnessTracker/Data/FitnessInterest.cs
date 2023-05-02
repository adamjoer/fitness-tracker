using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessTracker.Data;

public class FitnessInterest
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string? Id { get; set; }

    [Required]
    [Range(1, 10, ErrorMessage = "Intensity must be between 1 and 10")]
    public int Intensity { get; set; }

    public string UserId { get; set; } = string.Empty;

    public virtual FitnessTrackerUser User { get; set; } = null!;

    public string TypeId { get; set; } = string.Empty;

    public virtual WorkoutType Type { get; set; } = null!;
}
