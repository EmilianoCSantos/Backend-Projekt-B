using Microsoft.AspNetCore.Mvc.RazorPages;
using Models.Interfaces;
using Services.Interfaces;

namespace AppRazor.Pages;

public class FriendDetailsModel : PageModel
{
    private readonly IFriendsService _friendsService;
    private readonly ILogger<FriendDetailsModel> _logger;

    public IFriend? Friend { get; set; }
    public string ErrorMessage { get; set; } = "";

    public FriendDetailsModel(IFriendsService friendsService, ILogger<FriendDetailsModel> logger)
    {
        _friendsService = friendsService;
        _logger = logger;
    }

    public async Task OnGetAsync(Guid id)
    {
        try
        {
            // If no ID provided, show error
            if (id == Guid.Empty)
            {
                ErrorMessage = "No friend ID provided";
                return;
            }

            _logger.LogInformation($"Loading friend with ID: {id}");

            // Fetch the specific friend with all related data (Address, Pets, Quotes)
            var response = await _friendsService.ReadFriendAsync(id, false);
            
            if (response?.Item != null)
            {
                Friend = response.Item;
                _logger.LogInformation($"Found friend: {Friend.FirstName} {Friend.LastName}");
            }
            else
            {
                ErrorMessage = "Friend not found";
                _logger.LogWarning($"Friend with ID {id} not found");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading friend details: {ex.Message}";
            _logger.LogError($"Error loading friend: {ex.Message}");
        }
    }
}
