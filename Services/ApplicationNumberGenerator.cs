using Microsoft.EntityFrameworkCore;
using ClemontLoan.Api.Data;

namespace ClemontLoan.Api.Services;

public interface IApplicationNumberGenerator
{
    Task<string> GenerateApplicationNumberAsync();
}

public class ApplicationNumberGenerator : IApplicationNumberGenerator
{
    private readonly ApplicationDbContext _context;

    public ApplicationNumberGenerator(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateApplicationNumberAsync()
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var prefix = $"LN-{today}-";

        // Cari nomor pengajuan terbesar untuk hari ini
        var latestApp = await _context.LoanApplications
            .Where(a => a.ApplicationNumber.StartsWith(prefix))
            .OrderByDescending(a => a.ApplicationNumber)
            .Select(a => a.ApplicationNumber)
            .FirstOrDefaultAsync();

        int nextSeq = 1;
        if (latestApp != null && latestApp.Length >= prefix.Length)
        {
            var seqStr = latestApp.Substring(prefix.Length);
            if (int.TryParse(seqStr, out int currentSeq))
            {
                nextSeq = currentSeq + 1;
            }
        }

        return $"{prefix}{nextSeq:D3}";
    }
}
