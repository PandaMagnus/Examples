using System.ComponentModel.DataAnnotations;

namespace ExampleBlazorApp.Shared;

public class Player
{
    [Required]
    [StringLength(10, ErrorMessage = "Selection is too long.")]
    public string? PlayerChoice { get; set; }
}
