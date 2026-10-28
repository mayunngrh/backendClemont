using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ClemontLoan.Api.DTOs;

public class CreateLoanApplicationRequestDto : IValidatableObject
{
    [Required(ErrorMessage = "Nama lengkap wajib diisi.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Nama lengkap harus antara 3 hingga 100 karakter.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "NIK wajib diisi.")]
    public string NationalId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nomor telepon wajib diisi.")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email wajib diisi.")]
    [EmailAddress(ErrorMessage = "Format email tidak valid.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Jumlah pinjaman wajib diisi.")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Tenor wajib diisi.")]
    public int TenorMonths { get; set; }

    [Required(ErrorMessage = "Tujuan pinjaman wajib diisi.")]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "Tujuan pinjaman harus antara 10 hingga 500 karakter.")]
    public string Purpose { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Validasi NIK: Tepat 16 digit angka (string)
        if (string.IsNullOrWhiteSpace(NationalId) || !Regex.IsMatch(NationalId.Trim(), @"^\d{16}$"))
        {
            yield return new ValidationResult("NIK harus tepat 16 digit angka.", new[] { nameof(NationalId) });
        }

        // Validasi Phone: 10 - 15 digit setelah normalisasi awalan +
        var rawPhone = (Phone ?? "").Trim();
        var normalizedPhone = rawPhone.StartsWith("+") ? rawPhone.Substring(1) : rawPhone;
        if (!Regex.IsMatch(normalizedPhone, @"^\d{10,15}$"))
        {
            yield return new ValidationResult("Nomor telepon harus terdiri dari 10 hingga 15 digit angka.", new[] { nameof(Phone) });
        }

        // Validasi Amount: Rp1.000.000 s/d Rp100.000.000
        if (Amount < 1000000m || Amount > 100000000m)
        {
            yield return new ValidationResult("Jumlah pinjaman harus antara Rp1.000.000 dan Rp100.000.000.", new[] { nameof(Amount) });
        }

        // Validasi Tenor: hanya 6, 12, 24, atau 36 bulan
        int[] validTenors = { 6, 12, 24, 36 };
        if (!validTenors.Contains(TenorMonths))
        {
            yield return new ValidationResult("Tenor hanya boleh 6, 12, 24, atau 36 bulan.", new[] { nameof(TenorMonths) });
        }
    }
}

public class LoanApplicationDto
{
    public string Id { get; set; } = string.Empty;
    public string ApplicationNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int TenorMonths { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public string Status { get; set; } = "submitted";
    public DateTime CreatedAt { get; set; }
}

public class PaginatedResponseDto<T>
{
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}
