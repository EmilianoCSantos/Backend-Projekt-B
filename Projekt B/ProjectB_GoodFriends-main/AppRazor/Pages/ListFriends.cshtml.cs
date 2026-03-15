using Microsoft.AspNetCore.Mvc.RazorPages;
using Models.Interfaces;
using Services.Interfaces;

namespace AppRazor.Pages;

public class ListFriendsModel : PageModel
{
    private readonly IFriendsService _friendsService;
    private readonly ILogger<ListFriendsModel> _logger;

    public List<IFriend> Friends { get; set; } = [];
    public List<string> Countries { get; set; } = [];
    public List<string> Cities { get; set; } = [];
    public string? SelectedCountry { get; set; }
    public string? SelectedCity { get; set; }

    public ListFriendsModel(IFriendsService friendsService, ILogger<ListFriendsModel> logger)
    {
        _friendsService = friendsService;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        try
        {
            var response = await _friendsService.ReadFriendsAsync(true, false, "", 1, 100);
            
            _logger.LogInformation($"Loaded {response?.PageItems?.Count ?? 0} friends from service");
            
            if (response != null && response.PageItems != null)
            {
                // Show all friends initially
                Friends = response.PageItems;
                
                // Extract unique countries for dropdown
                Countries = Friends
                    .Where(f => f.Address != null)
                    .Select(f => f.Address.Country)
                    .Where(c => !string.IsNullOrEmpty(c))
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();
                
                _logger.LogInformation($"Extracted {Countries.Count} unique countries");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error loading friends: {ex.Message}");
        }
    }

    public async Task OnPostAsync(string? country, string? city)
    {
        try
        {
            SelectedCountry = country;
            SelectedCity = city;

            var response = await _friendsService.ReadFriendsAsync(true, false, "", 1, 100);
            
            if (response != null && response.PageItems != null)
            {
                var allFriends = response.PageItems;

                // Always extract all countries
                Countries = allFriends
                    .Where(f => f.Address != null)
                    .Select(f => f.Address.Country)
                    .Where(c => !string.IsNullOrEmpty(c))
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();

                // If country selected, extract cities from that country
                if (!string.IsNullOrEmpty(country))
                {
                    Cities = allFriends
                        .Where(f => f.Address?.Country == country)
                        .Select(f => f.Address.City)
                        .Where(c => !string.IsNullOrEmpty(c))
                        .Distinct()
                        .OrderBy(c => c)
                        .ToList();
                    
                    _logger.LogInformation($"Extracted {Cities.Count} cities for country: {country}");
                }

                // Apply filters
                Friends = allFriends;

                if (!string.IsNullOrEmpty(country))
                {
                    Friends = Friends.Where(f => f.Address?.Country == country).ToList();
                    
                    if (!string.IsNullOrEmpty(city))
                    {
                        Friends = Friends.Where(f => f.Address?.City == city).ToList();
                        _logger.LogInformation($"Found {Friends.Count} friends in {city}, {country}");
                    }
                    else
                    {
                        _logger.LogInformation($"Found {Friends.Count} friends in {country}");
                    }
                }
                else
                {
                    _logger.LogInformation($"Showing all {Friends.Count} friends");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error filtering friends: {ex.Message}");
        }
    }
}
