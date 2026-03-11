using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.Interfaces;
using Models.DTO;

namespace AppRazor.Pages;

public class SeedModel : PageModel
{
    private readonly IAdminService _adminService;
    private readonly ILogger<SeedModel> _logger;

    public GstUsrInfoAllDto? SeedResult { get; set; }
    public string? Message { get; set; }
    public bool IsSuccess { get; set; }

    public SeedModel(IAdminService adminService, ILogger<SeedModel> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    public void OnGet()
    {
        // Initial load - no action needed
    }

    public async Task<IActionResult> OnPostSeedAsync()
    {
        try
        {
            _logger.LogInformation("Seeding database...");
            var response = await _adminService.SeedAsync(100);
            
                if (response != null && response.Item != null)
                {
                    SeedResult = response.Item;
            }
            
            _logger.LogInformation(Message);
            return Page();
        }
        catch (Exception ex)
        {
            Message = $"Error: {ex.Message}";
            IsSuccess = false;
            _logger.LogError(ex, "Error seeding database");
            return Page();
        }
    }

    public async Task<IActionResult> OnPostRemoveSeedAsync()
    {
        try
        {
            _logger.LogInformation("Removing seed data...");
            var response = await _adminService.RemoveSeedAsync(true);
            
            if (response != null && response.Item != null)
            {
                SeedResult = response.Item;
                Message = "Seed data removed successfully!";
                IsSuccess = true;
            }
            else
            {
                Message = "Remove seed failed!";
                IsSuccess = false;
            }
            
            _logger.LogInformation(Message);
            return Page();
        }
        catch (Exception ex)
        {
            Message = $"Error: {ex.Message}";
            IsSuccess = false;
            _logger.LogError(ex, "Error removing seed data");
            return Page();
        }
    }
}
