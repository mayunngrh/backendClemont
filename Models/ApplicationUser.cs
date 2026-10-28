using Microsoft.AspNetCore.Identity;

namespace ClemontLoan.Api.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;

    public ICollection<LoanApplication> LoanApplications { get; set; } = new List<LoanApplication>();
}
