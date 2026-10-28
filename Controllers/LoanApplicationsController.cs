using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClemontLoan.Api.Data;
using ClemontLoan.Api.DTOs;
using ClemontLoan.Api.Models;
using ClemontLoan.Api.Services;

namespace ClemontLoan.Api.Controllers;

[ApiController]
[Route("api/loan-applications")]
[Authorize]
public class LoanApplicationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IApplicationNumberGenerator _numberGenerator;

    public LoanApplicationsController(
        ApplicationDbContext context,
        IApplicationNumberGenerator numberGenerator)
    {
        _context = context;
        _numberGenerator = numberGenerator;
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    /// <summary>
    /// GET /api/loan-applications?search=&page=1&pageSize=10
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PaginatedResponseDto<LoanApplicationDto>>> GetLoanApplications(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var currentUserId = GetCurrentUserId();
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Unauthorized();
        }

        // Clamp pagination values
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 50) pageSize = 50;

        // Base query: only loans created by current user
        var query = _context.LoanApplications
            .AsNoTracking()
            .Where(a => a.CreatedByUserId == currentUserId);

        // Search filter (fullName or applicationNumber)
        if (!string.IsNullOrWhiteSpace(search))
        {
            var trimmedSearch = search.Trim().ToLower();
            query = query.Where(a =>
                a.FullName.ToLower().Contains(trimmedSearch) ||
                a.ApplicationNumber.ToLower().Contains(trimmedSearch));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new LoanApplicationDto
            {
                Id = a.Id,
                ApplicationNumber = a.ApplicationNumber,
                FullName = a.FullName,
                NationalId = a.NationalId,
                Phone = a.Phone,
                Email = a.Email,
                Amount = a.Amount,
                TenorMonths = a.TenorMonths,
                Purpose = a.Purpose,
                Status = a.Status,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();

        return Ok(new PaginatedResponseDto<LoanApplicationDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    /// <summary>
    /// GET /api/loan-applications/{id}
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<LoanApplicationDto>> GetLoanApplicationById(string id)
    {
        var currentUserId = GetCurrentUserId();
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Unauthorized();
        }

        var application = await _context.LoanApplications
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id && a.CreatedByUserId == currentUserId);

        if (application == null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Not Found",
                detail: $"Pengajuan pinjaman dengan ID '{id}' tidak ditemukan.");
        }

        return Ok(new LoanApplicationDto
        {
            Id = application.Id,
            ApplicationNumber = application.ApplicationNumber,
            FullName = application.FullName,
            NationalId = application.NationalId,
            Phone = application.Phone,
            Email = application.Email,
            Amount = application.Amount,
            TenorMonths = application.TenorMonths,
            Purpose = application.Purpose,
            Status = application.Status,
            CreatedAt = application.CreatedAt
        });
    }

    /// <summary>
    /// POST /api/loan-applications
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<LoanApplicationDto>> CreateLoanApplication([FromBody] CreateLoanApplicationRequestDto request)
    {
        var currentUserId = GetCurrentUserId();
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Unauthorized();
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var applicationNumber = await _numberGenerator.GenerateApplicationNumberAsync();

        var newApplication = new LoanApplication
        {
            Id = $"la-{Guid.NewGuid():N}",
            ApplicationNumber = applicationNumber,
            FullName = request.FullName.Trim(),
            NationalId = request.NationalId.Trim(),
            Phone = request.Phone.Trim(),
            Email = request.Email.Trim(),
            Amount = request.Amount,
            TenorMonths = request.TenorMonths,
            Purpose = request.Purpose.Trim(),
            Status = "submitted",
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = currentUserId
        };

        _context.LoanApplications.Add(newApplication);
        await _context.SaveChangesAsync();

        var resultDto = new LoanApplicationDto
        {
            Id = newApplication.Id,
            ApplicationNumber = newApplication.ApplicationNumber,
            FullName = newApplication.FullName,
            NationalId = newApplication.NationalId,
            Phone = newApplication.Phone,
            Email = newApplication.Email,
            Amount = newApplication.Amount,
            TenorMonths = newApplication.TenorMonths,
            Purpose = newApplication.Purpose,
            Status = newApplication.Status,
            CreatedAt = newApplication.CreatedAt
        };

        return CreatedAtAction(
            nameof(GetLoanApplicationById),
            new { id = resultDto.Id },
            resultDto);
    }
}
