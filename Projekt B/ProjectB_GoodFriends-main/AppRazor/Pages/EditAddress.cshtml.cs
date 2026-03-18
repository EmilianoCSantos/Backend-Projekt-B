using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models;
using Models.DTO;
using Models.Interfaces;
using Services.Interfaces;

namespace AppRazor.Pages;

public class EditAddressModel : PageModel
{
    private readonly IFriendsService _friendsService;
    private readonly IAddressesService _addressesService;
    private readonly ILogger<EditAddressModel> _logger;

    [BindProperty]
    public Address? Address { get; set; }

    public Guid FriendId { get; set; }
    public string? FriendName { get; set; }
    public string? ErrorMessage { get; set; }

    public EditAddressModel(IFriendsService friendsService, IAddressesService addressesService, ILogger<EditAddressModel> logger)
    {
        _friendsService = friendsService;
        _addressesService = addressesService;
        _logger = logger;
    }

    public async Task OnGetAsync(Guid friendId)
    {
        try
        {
            if (friendId == Guid.Empty)
            {
                ErrorMessage = "No friend ID provided";
                return;
            }

            FriendId = friendId;
            var response = await _friendsService.ReadFriendAsync(friendId, false);
            
            if (response?.Item != null)
            {
                FriendName = $"{response.Item.FirstName} {response.Item.LastName}";
                Address = (Address)response.Item.Address;
                _logger.LogInformation($"Loaded address for friend {FriendName}");
            }
            else
            {
                ErrorMessage = "Friend not found";
                _logger.LogWarning($"Friend with ID {friendId} not found");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading address: {ex.Message}";
            _logger.LogError($"Error loading address: {ex.Message}");
        }
    }

    public async Task<IActionResult> OnPostAsync(Guid friendId)
    {
        try
        {
            if (friendId == Guid.Empty)
            {
                ModelState.AddModelError("", "Friend ID is missing");
                return Page();
            }

            FriendId = friendId;

            // Load friend name for display
            var friendResponse = await _friendsService.ReadFriendAsync(friendId, false);
            if (friendResponse?.Item != null)
            {
                FriendName = $"{friendResponse.Item.FirstName} {friendResponse.Item.LastName}";
            }

            // Server-side validation
            if (Address == null || Address.AddressId == Guid.Empty)
            {
                ModelState.AddModelError("", "Address data is missing");
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Address.StreetAddress) || Address.StreetAddress.Length < 3)
            {
                ModelState.AddModelError("Address.StreetAddress", "Street address must be at least 3 characters");
            }

            if (Address.ZipCode <= 0)
            {
                ModelState.AddModelError("Address.ZipCode", "Zip code must be greater than 0");
            }

            if (string.IsNullOrWhiteSpace(Address.City))
            {
                ModelState.AddModelError("Address.City", "City is required");
            }

            if (string.IsNullOrWhiteSpace(Address.Country))
            {
                ModelState.AddModelError("Address.Country", "Country is required");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning($"Validation failed for address {Address.AddressId}");
                return Page();
            }

            // Try to update address
            var dto = new AddressCuDto(Address);
            var response = await _addressesService.UpdateAddressAsync(dto);
            
            if (response?.Item != null)
            {
                _logger.LogInformation($"Address updated successfully for friend {FriendName}");
                return RedirectToPage("/FriendDetails", new { id = friendId });
            }
            else
            {
                ModelState.AddModelError("", "Failed to update address. Please try again.");
                _logger.LogError($"Update failed for address {Address.AddressId}");
                return Page();
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Error saving address: {ex.Message}");
            _logger.LogError($"Error updating address: {ex.Message}");
            return Page();
        }
    }
}
