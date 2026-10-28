namespace ClemontLoan.Api.Models;

public class LoanApplication
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string ApplicationNumber { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string NationalId { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public int TenorMonths { get; set; }

    public string Purpose { get; set; } = string.Empty;

    public string Status { get; set; } = "submitted";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string CreatedByUserId { get; set; } = string.Empty;

    public ApplicationUser? CreatedByUser { get; set; }
}
