using Microsoft.AspNetCore.Mvc.RazorPages;
using Models.Interfaces;
using Services.Interfaces;

namespace AppRazor.Pages;

public class FriendsPetsModel : PageModel
{
    private readonly IFriendsService _friendsService;
    private readonly ILogger<FriendsPetsModel> _logger;

    public List<string> Countries { get; set; } = [];
    public List<string> Cities { get; set; } = [];
    public List<IFriend> Friends { get; set; } = [];
    public List<IPet> Pets { get; set; } = [];
    public string? SelectedCountry { get; set; }
    public string? SelectedCity { get; set; }

    public FriendsPetsModel(IFriendsService friendsService, ILogger<FriendsPetsModel> logger)
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
                var allFriends = response.PageItems;
                
                // Extract unique countries
                Countries = allFriends
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
            _logger.LogError($"Error loading countries: {ex.Message}");
        }
    }

    public async Task OnPostAsync(string? country, string? city)
    {
        try
        {
            SelectedCountry = country;
            SelectedCity = city;

            // Load all friends
            var response = await _friendsService.ReadFriendsAsync(true, false, "", 1, 100);
            
            if (response != null && response.PageItems != null)
            {
                var allFriends = response.PageItems;

                // Extract all countries for dropdown
                Countries = allFriends
                    .Where(f => f.Address != null)
                    .Select(f => f.Address.Country)
                    .Where(c => !string.IsNullOrEmpty(c))
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();

                // If country is selected, extract cities from that country
                if (!string.IsNullOrEmpty(country))
                {
                    Cities = allFriends
                        .Where(f => f.Address?.Country == country)
                        .Select(f => f.Address.City)
                        .Where(c => !string.IsNullOrEmpty(c))
                        .Distinct()
                        .OrderBy(c => c)
                        .ToList();
                    
                    _logger.LogInformation($"Extracted {Cities.Count} unique cities for country: {country}");
                }

                // If both country and city are selected, get friends and pets from that city
                if (!string.IsNullOrEmpty(country) && !string.IsNullOrEmpty(city))
                {
                    Friends = allFriends
                        .Where(f => f.Address?.Country == country && f.Address?.City == city)
                        .ToList();
                    
                    _logger.LogInformation($"Found {Friends.Count} friends in {city}, {country}");

                    // Extract all pets from these friends
                    Pets = Friends
                        .Where(f => f.Pets != null)
                        .SelectMany(f => f.Pets)
                        .ToList();
                    
                    _logger.LogInformation($"Found {Pets.Count} pets for friends in {city}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error filtering friends and pets: {ex.Message}");
        }
    }
}
