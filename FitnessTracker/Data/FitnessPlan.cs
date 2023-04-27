using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessTracker.Data;

public class FitnessPlan
{
    public FitnessPlan()
    {
        WorkoutItems = new HashSet<WorkoutItem>();
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string? Id { get; set; }

    [Required]
    [MaxLength(256, ErrorMessage = "Title is too long")]
    public string Title { get; set; } = string.Empty;

    [Required]
    public DateTime Date { get; set; }

    [Required]
    [Range(1, 10, ErrorMessage = "Intensity must be between 1 and 10")]
    public int Intensity { get; set; }

    public string UserId { get; set; } = string.Empty;

    public virtual FitnessTrackerUser User { get; set; } = null!;

    public virtual ICollection<WorkoutItem> WorkoutItems { get; set; }
}
