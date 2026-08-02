using Microsoft.AspNetCore.Identity;

namespace Nres.Onboarding.Web.Models.Shared;

/// <summary>
/// Staff details for a logged-in user.
/// Deliberately kept OUT of <c>AspNetUsers</c>: the Identity tables stay purely about
/// authentication (login, password hash, lockout), while everything the business
/// cares about lives here where it can be extended without touching Identity.
/// This is one of the most common beginner mistakes the course sets out to correct.
/// </summary>
public class UserProfile
{
    public int Id { get; set; }

    /// <summary>Foreign key to <c>AspNetUsers.Id</c>. One profile per user.</summary>
    public string UserId { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    /// <summary>IC / identity card number.</summary>
    public string IdentityNo { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public int? DepartmentId { get; set; }

    public int? PositionId { get; set; }

    public int? GradeId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public IdentityUser? User { get; set; }

    public LookupDepartment? Department { get; set; }

    public LookupPosition? Position { get; set; }

    public LookupGrade? Grade { get; set; }
}
