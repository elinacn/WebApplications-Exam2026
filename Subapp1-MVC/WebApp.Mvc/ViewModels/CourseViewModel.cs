using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using WebApp.Models;

namespace WebApp.ViewModels;

// Course pages receive only the fields they display or edit, never database entities.
public class CourseViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Course code is required.")]
    [StringLength(20, ErrorMessage = "Course code can be at most 20 characters.")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Course title is required.")]
    [StringLength(100, ErrorMessage = "Course title can be at most 100 characters.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description can be at most 500 characters.")]
    public string? Description { get; set; }

    [BindNever]
    public List<string> QuestTitles { get; set; } = [];

    public static CourseViewModel FromEntity(Course course) => new()
    {
        Id = course.Id,
        Code = course.Code,
        Title = course.Title,
        Description = course.Description,
        QuestTitles = course.Quests.Select(q => q.Title).ToList()
    };
}
