using Microsoft.AspNetCore.Mvc.RazorPages;
using Models.Interfaces;
using Services.Interfaces;

namespace AppRazor.Pages;

public class FriendsModel : PageModel
{
    private readonly IFriendsService _friendsService;
    private readonly ILogger<FriendsModel> _logger;

    public List<IFriend> Friends { get; set; } = [];
    public List<string> Countries { get; set; } = [];
    public string? SelectedCountry { get; set; }

    public FriendsModel(IFriendsService friendsService, ILogger<FriendsModel> logger)
    {
        _friendsService = friendsService;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        try
        {
            // Load all friends and extract unique countries
            var response = await _friendsService.ReadFriendsAsync(true, false, "", 1, 100);
            
            _logger.LogInformation($"Loaded {response?.PageItems?.Count ?? 0} friends from service");
            
            if (response != null && response.PageItems != null)
            {
                Friends = response.PageItems;
                _logger.LogInformation($"Total friends in list: {Friends.Count}");
                _logger.LogInformation($"Friends with Address: {Friends.Count(f => f.Address != null)}");
                
                Countries = Friends
                    .Where(f => f.Address != null)
                    .Select(f => f.Address.Country)
                    .Where(c => !string.IsNullOrEmpty(c))
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();
                
                _logger.LogInformation($"Extracted {Countries.Count} unique countries: {string.Join(", ", Countries)}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error loading friends: {ex.Message}");
        }
    }

    public async Task OnPostAsync(string? country)
    {
        try
        {
            // Set the selected country for display
            SelectedCountry = country;

            // Load all friends
            var response = await _friendsService.ReadFriendsAsync(true, false, "", 1, 100);
            
            if (response != null && response.PageItems != null)
            {
                Friends = response.PageItems;

                // Extract countries for dropdown
                Countries = Friends
                    .Where(f => f.Address != null)
                    .Select(f => f.Address.Country)
                    .Where(c => !string.IsNullOrEmpty(c))
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();

                // Filter by selected country if provided
                if (!string.IsNullOrEmpty(country))
                {
                    Friends = Friends
                        .Where(f => f.Address != null && f.Address.Country == country)
                        .ToList();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error filtering friends: {ex.Message}");
        }
    }
}
