using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessTracker.Data;

public class WorkoutType
{
    public WorkoutType()
    {
        FitnessInterests = new HashSet<FitnessInterest>();
        WorkoutTypeTags = new HashSet<WorkoutTypeTag>();
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string? Id { get; set; }

    [Required]
    [MaxLength(256)]
    public string Name { get; set; } = string.Empty;
    
    public virtual ICollection<FitnessInterest> FitnessInterests { get; set; }
    
    public virtual ICollection<WorkoutTypeTag> WorkoutTypeTags { get; set; }
}
