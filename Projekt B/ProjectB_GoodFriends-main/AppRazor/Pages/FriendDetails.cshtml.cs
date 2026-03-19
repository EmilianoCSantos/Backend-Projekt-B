using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models.Interfaces;
using Services.Interfaces;

namespace AppRazor.Pages;

public class FriendDetailsModel : PageModel
{
    private readonly IFriendsService _friendsService;
    private readonly IPetsService _petsService;
    private readonly IQuotesService _quotesService;
    private readonly ILogger<FriendDetailsModel> _logger;

    public IFriend? Friend { get; set; }
    public string ErrorMessage { get; set; } = "";

    public FriendDetailsModel(IFriendsService friendsService, IPetsService petsService, IQuotesService quotesService, ILogger<FriendDetailsModel> logger)
    {
        _friendsService = friendsService;
        _petsService = petsService;
        _quotesService = quotesService;
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

    public async Task<IActionResult> OnPostDeletePetAsync(Guid friendId, Guid petId)
    {
        try
        {
            if (friendId == Guid.Empty || petId == Guid.Empty)
            {
                _logger.LogWarning("Invalid IDs for pet deletion");
                return BadRequest();
            }

            var response = await _petsService.DeletePetAsync(petId);
            if (response?.Item != null)
            {
                _logger.LogInformation($"Pet {petId} deleted successfully");
                return RedirectToPage(new { id = friendId });
            }
            else
            {
                ErrorMessage = "Failed to delete pet";
                _logger.LogWarning($"Failed to delete pet {petId}");
                return RedirectToPage(new { id = friendId });
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error deleting pet: {ex.Message}";
            _logger.LogError($"Error deleting pet: {ex.Message}");
            return RedirectToPage(new { id = friendId });
        }
    }

    public async Task<IActionResult> OnPostDeleteQuoteAsync(Guid friendId, Guid quoteId)
    {
        try
        {
            if (friendId == Guid.Empty || quoteId == Guid.Empty)
            {
                _logger.LogWarning("Invalid IDs for quote deletion");
                return BadRequest();
            }

            var response = await _quotesService.DeleteQuoteAsync(quoteId);
            if (response?.Item != null)
            {
                _logger.LogInformation($"Quote {quoteId} deleted successfully");
                return RedirectToPage(new { id = friendId });
            }
            else
            {
                ErrorMessage = "Failed to delete quote";
                _logger.LogWarning($"Failed to delete quote {quoteId}");
                return RedirectToPage(new { id = friendId });
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error deleting quote: {ex.Message}";
            _logger.LogError($"Error deleting quote: {ex.Message}");
            return RedirectToPage(new { id = friendId });
        }
    }
}
