using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using TriviumParkingApp.Backend.Data;
using TriviumParkingApp.Backend.DTOs;
using TriviumParkingApp.Backend.Models;
using TriviumParkingApp.Backend.Repositories;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Identity;

namespace TriviumParkingApp.Backend.Services;

public class UserService : IUserService
{
    private readonly IDbContextFactory<ParkingDbContext> _contextFactory;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly ILogger<UserService> _logger;
    private readonly FirebaseAuth _firebaseAuth;

    public UserService(
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        FirebaseApp firebaseApp,
        ILogger<UserService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
        _firebaseAuth = FirebaseAuth.GetAuth(firebaseApp);
    }

    public async Task<UserResponseDto?> SyncFirebaseUserAsync(
        UserSyncRequestDto syncRequest,
        bool createNewUser = false)
    {
        if (syncRequest == null || string.IsNullOrWhiteSpace(syncRequest.FirebaseUid))
        {
            _logger.LogWarning("SyncFirebaseUserAsync called with invalid request DTO.");
            return null;
        }

        try
        {
            var user = await _userManager.FindByNameAsync(syncRequest.FirebaseUid);

            if (user == null)
            {
                if (!createNewUser)
                {
                    _logger.LogError("Failed to fetch user with Firebase UID {FirebaseUid}", syncRequest.FirebaseUid);
                    return null;
                }

                _logger.LogInformation(
                    "User with Firebase UID {FirebaseUid} not found. Creating new user.",
                    syncRequest.FirebaseUid);

                user = new User
                {
                    FirebaseUid = syncRequest.FirebaseUid,
                    Email = syncRequest.Email,
                    DisplayName = syncRequest.DisplayName
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    _logger.LogError("Error creating new user: {Errors}",
                        string.Join(", ", createResult.Errors.Select(e => e.Description)));
                    return null;
                }

                const string defaultRoleName = Constants.Constants.RoleNames.Employee;
                var role = await _roleManager.FindByNameAsync(defaultRoleName);
                if(role != null)
                {
                    var addRoleResult = await _userManager.AddToRoleAsync(user, defaultRoleName);
                    if (addRoleResult.Succeeded)
                    {
                        _logger.LogInformation(
                            "Assigned default role '{DefaultRole}' to new user {UserId}",
                            defaultRoleName, user.Id);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Failed to assign default role '{DefaultRole}' to user {UserId}: {Errors}",
                            defaultRoleName, user.Id,
                            string.Join(", ", addRoleResult.Errors.Select(e => e.Description)));
                    }
                }
                else
                {
                    _logger.LogWarning(
                        "Default role '{DefaultRole}' not found. New user {UserId} created without a role.",
                        defaultRoleName, user.Id);
                }
            }
            else
            {
                _logger.LogInformation(
                    "Found existing user {UserId} for Firebase UID {FirebaseUid}.",
                    user.Id, syncRequest.FirebaseUid);

                bool updated = false;

                if (!string.IsNullOrEmpty(syncRequest.Email) &&
                    !string.Equals(user.Email, syncRequest.Email, StringComparison.OrdinalIgnoreCase))
                {
                    user.Email = syncRequest.Email;
                    updated = true;
                }

                if (!string.IsNullOrEmpty(syncRequest.DisplayName) &&
                    user.DisplayName != syncRequest.DisplayName)
                {
                    user.DisplayName = syncRequest.DisplayName;
                    updated = true;
                }

                if (updated)
                {
                    var updateResult = await _userManager.UpdateAsync(user);
                    if (updateResult.Succeeded)
                    {
                        _logger.LogInformation("Updated details for user {UserId}.", user.Id);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Failed to update user {UserId}: {Errors}",
                            user.Id,
                            string.Join(", ", updateResult.Errors.Select(e => e.Description)));
                    }
                }
            }

            var roles = await _userManager.GetRolesAsync(user);

            var responseDto = new UserResponseDto
            {
                Id = user.Id,
                FirebaseUid = user.FirebaseUid,
                Email = user.Email,
                DisplayName = user.DisplayName,
                Roles = [.. roles]
            };

            return responseDto;
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database error during SyncFirebaseUserAsync for Firebase UID {FirebaseUid}.",
                syncRequest.FirebaseUid);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during SyncFirebaseUserAsync for Firebase UID {FirebaseUid}.",
                syncRequest.FirebaseUid);
            return null;
        }
    }

    public async Task<bool> AssignRoleAsync(int userId, string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            _logger.LogWarning(
                "AssignRoleAsync called with empty role name for user ID {UserId}.",
                userId);
            return false;
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            _logger.LogWarning(
                "AssignRoleAsync: User with ID {UserId} not found.",
                userId);
            return false;
        }

        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            _logger.LogWarning(
                "AssignRoleAsync: Role '{RoleName}' not found.",
                roleName);
            return false;
        }

        if (await _userManager.IsInRoleAsync(user, roleName))
        {
            _logger.LogInformation(
                "User ID {UserId} already has role '{RoleName}'",
                userId, roleName);
            return true;
        }

        var result = await _userManager.AddToRoleAsync(user, roleName);
        if (result.Succeeded)
        {
            _logger.LogInformation(
                "Assigned role '{RoleName}' to user ID {UserId}",
                roleName, userId);
            return true;
        }
        else
        {
            _logger.LogError(
                "Failed to assign role '{RoleName}' to user ID {UserId}: {Errors}",
                roleName, userId, string.Join(", ", result.Errors.Select(e => e.Description)));
            return false;
        }
    }

    public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
    {
        try
        {
            var users = await _userManager.Users
                .OrderBy(u => u.DisplayName ?? u.Email)
                .ToListAsync();

            var result = new List<UserResponseDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                result.Add(new UserResponseDto
                {
                    Id = user.Id,
                    FirebaseUid = user.FirebaseUid,
                    Email = user.Email,
                    DisplayName = user.DisplayName,
                    Roles = [.. roles]
                });
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all users via UserManager.");
            throw;
        }
    }

    public async Task<UserResponseDto?> CreateUserAsync(CreateUserRequestDto createUserDto)
    {
        if (createUserDto?.Email == null || createUserDto.Password == null)
        {
            _logger.LogWarning("CreateUserAsync called with missing email or password.");
            return null;
        }

        try
        {
            var userArgs = new UserRecordArgs()
            {
                Email = createUserDto.Email,
                Password = createUserDto.Password,
                DisplayName = createUserDto.DisplayName,
                EmailVerified = false,
                Disabled = false
            };

            UserRecord firebaseUserRecord = await _firebaseAuth.CreateUserAsync(userArgs);
            _logger.LogInformation("Successfully created Firebase user: {FirebaseUid}", firebaseUserRecord.Uid);

            var syncDto = new UserSyncRequestDto
            {
                FirebaseUid = firebaseUserRecord.Uid,
                Email = firebaseUserRecord.Email,
                DisplayName = firebaseUserRecord.DisplayName,
                CountryIsoCode = createUserDto.CountryIsoCode,
            };

            var userResponse = await SyncFirebaseUserAsync(syncDto, true);

            if (userResponse == null)
            {
                _logger.LogError("Failed to sync newly created Firebase user {FirebaseUid} to local DB.", firebaseUserRecord.Uid);
                try
                {
                    await _firebaseAuth.DeleteUserAsync(firebaseUserRecord.Uid);
                }
                catch (Exception deleteEx)
                {
                    _logger.LogError(deleteEx, "Failed to delete Firebase user {FirebaseUid} after DB sync failure.", firebaseUserRecord.Uid);
                }
                return null;
            }

            return userResponse;
        }
        catch (FirebaseAuthException faEx)
        {
            _logger.LogError(faEx, "Firebase error creating user with email {Email}. Code: {ErrorCode}", createUserDto.Email, faEx.AuthErrorCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during CreateUserAsync for email {Email}.", createUserDto.Email);
            return null;
        }
    }
}