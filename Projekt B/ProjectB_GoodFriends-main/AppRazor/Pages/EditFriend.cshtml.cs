using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models;
using Models.DTO;
using Services.Interfaces;

namespace AppRazor.Pages;

public class EditFriendModel : PageModel
{
    private readonly IFriendsService _friendsService;
    private readonly ILogger<EditFriendModel> _logger;

    [BindProperty]
    public csFriend? Friend { get; set; }

    public string? ErrorMessage { get; set; }

    public EditFriendModel(IFriendsService friendsService, ILogger<EditFriendModel> logger)
    {
        _friendsService = friendsService;
        _logger = logger;
    }

    public async Task OnGetAsync(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
            {
                ErrorMessage = "No friend ID provided";
                return;
            }

            var response = await _friendsService.ReadFriendAsync(id, false);
            if (response?.Item != null)
            {
                Friend = (csFriend)response.Item;
                _logger.LogInformation($"Loaded friend {Friend.FirstName} {Friend.LastName} for editing");
            }
            else
            {
                ErrorMessage = "Friend not found";
                _logger.LogWarning($"Friend with ID {id} not found");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading friend: {ex.Message}";
            _logger.LogError($"Error loading friend: {ex.Message}");
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            // Server-side validation
            if (Friend == null || Friend.FriendId == Guid.Empty)
            {
                ModelState.AddModelError("", "Friend data is missing");
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Friend.FirstName) || Friend.FirstName.Length < 2)
            {
                ModelState.AddModelError("Friend.FirstName", "First name must be at least 2 characters");
            }

            if (string.IsNullOrWhiteSpace(Friend.LastName) || Friend.LastName.Length < 2)
            {
                ModelState.AddModelError("Friend.LastName", "Last name must be at least 2 characters");
            }

            if (string.IsNullOrWhiteSpace(Friend.Email) || !IsValidEmail(Friend.Email))
            {
                ModelState.AddModelError("Friend.Email", "Invalid email format");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning($"Validation failed for friend {Friend.FriendId}");
                return Page();
            }

            // Try to update friend
            var dto = new FriendCuDto(Friend);
            var response = await _friendsService.UpdateFriendAsync(dto);
            if (response?.Item != null)
            {
                _logger.LogInformation($"Friend {Friend.FirstName} {Friend.LastName} updated successfully");
                return RedirectToPage("/ListFriends");
            }
            else
            {
                ModelState.AddModelError("", "Failed to update friend. Please try again.");
                _logger.LogError($"Update failed for friend {Friend.FriendId}");
                return Page();
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Error saving friend: {ex.Message}");
            _logger.LogError($"Error updating friend: {ex.Message}");
            return Page();
        }
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
