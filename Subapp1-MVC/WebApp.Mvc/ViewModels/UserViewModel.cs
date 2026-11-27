using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using WebApp.Models;

namespace WebApp.ViewModels;

// Display and edit account details without passing password hashes to the views.
public class UserViewModel
{
    public int Id { get; set; }
    public bool IsAdmin { get; set; }

    [Required(ErrorMessage = "Username is required.")]
    [StringLength(30, MinimumLength = 3, ErrorMessage = "Username must be 3–30 characters.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(50, ErrorMessage = "Name can be at most 50 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Program is required.")]
    [StringLength(100, ErrorMessage = "Program can be at most 100 characters.")]
    [Display(Name = "Program")]
    public string StudyProgram { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
    [Display(Name = "Created")]
    public DateTime CreatedAt { get; set; } = DateTime.Today;

    [BindNever]
    public int Points { get; set; }

    [BindNever]
    [Display(Name = "Quizzes completed")]
    public int QuizzesCompleted { get; set; }

    [BindNever]
    [Display(Name = "Last seen")]
    public DateTime? LastSeenAt { get; set; }

    [BindNever]
    public bool HasPassword { get; set; }

    public bool IsOnline => LastSeenAt >= DateTime.UtcNow - User.OnlineWindow;

    public static UserViewModel FromEntity(User user) => new()
    {
        Id = user.Id,
        IsAdmin = user.IsAdmin,
        Username = user.Username,
        Name = user.Name,
        StudyProgram = user.StudyProgram,
        CreatedAt = user.CreatedAt,
        Points = user.Points,
        QuizzesCompleted = user.QuizzesCompleted,
        LastSeenAt = user.LastSeenAt,
        HasPassword = user.PasswordHash is not null
    };
}
