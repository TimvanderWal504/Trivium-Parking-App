using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using TriviumParkingApp.Backend.Data;
using TriviumParkingApp.Backend.DTOs;
using TriviumParkingApp.Backend.Models;
using TriviumParkingApp.Backend.Repositories;
using FirebaseAdmin;
using FirebaseAdmin.Auth;

namespace TriviumParkingApp.Backend.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IDbContextFactory<ParkingDbContext> _contextFactory;
    private readonly ILogger<UserService> _logger;
    private readonly FirebaseAuth _firebaseAuth;

    public UserService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        FirebaseApp firebaseApp,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _logger = logger;
        _firebaseAuth = FirebaseAuth.GetAuth(firebaseApp);
    }

    public async Task<UserResponseDto?> SyncFirebaseUserAsync(UserSyncRequestDto syncRequest, bool createNewUser = false)
    {
        if (syncRequest == null || string.IsNullOrWhiteSpace(syncRequest.FirebaseUid))
        {
            _logger.LogWarning("SyncFirebaseUserAsync called with invalid request DTO.");
            return null;
        }

        try
        {
            var user = await _userRepository.GetUserByFirebaseUidAsync(syncRequest.FirebaseUid, includeRoles: true);

            if (user == null)
            {
                if(!createNewUser)
                {
                    _logger.LogError("Failed to fetch user with Firebase UID {FirebaseUid}", syncRequest.FirebaseUid);
                    return null;
                }

                _logger.LogInformation("User with Firebase UID {FirebaseUid} not found. Creating new user.", syncRequest.FirebaseUid);
                user = new User
                {
                    FirebaseUid = syncRequest.FirebaseUid,
                    Email = syncRequest.Email,
                    DisplayName = syncRequest.DisplayName
                };
                await _userRepository.AddUserAsync(user);

                var defaultRole = await _roleRepository.GetRoleByNameAsync(Constants.Constants.RoleNames.Employee);
                if (defaultRole != null)
                {
                    if (!user.UserRoles.Any(ur => ur.RoleId == defaultRole.Id))
                    {
                         user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = defaultRole.Id });
                         _logger.LogInformation("Assigned default role '{DefaultRole}' to new user ID {UserId}", Constants.Constants.RoleNames.Employee, user.Id);
                         await _userRepository.UpdateUserAsync(user);
                    }
                }
                else
                {
                    _logger.LogWarning("Default role '{DefaultRole}' not found. New user ID {UserId} created without a role.", Constants.Constants.RoleNames.Employee, user.Id);
                }

                user = await _userRepository.GetUserByFirebaseUidAsync(syncRequest.FirebaseUid, includeRoles: true);
                if(user == null) {
                    _logger.LogError("Failed to re-fetch newly created user with Firebase UID {FirebaseUid}", syncRequest.FirebaseUid);
                    return null;
                }
            }
            else
            {
                _logger.LogInformation("Found existing user ID {UserId} for Firebase UID {FirebaseUid}.", user.Id, syncRequest.FirebaseUid);
                var updated = false;
                if (user.Email != syncRequest.Email && !string.IsNullOrEmpty(syncRequest.Email)) 
                { 
                    user.Email = syncRequest.Email; 
                    updated = true; 
                }
                if (user.DisplayName != syncRequest.DisplayName && !string.IsNullOrEmpty(syncRequest.DisplayName)) 
                { 
                    user.DisplayName = syncRequest.DisplayName; 
                    updated = true; 
                }

                if (updated)
                {
                    await _userRepository.UpdateUserAsync(user);
                    _logger.LogInformation("Updated details for user ID {UserId}.", user.Id);
                }
            }

            var responseDto = new UserResponseDto
            {
                Id = user.Id,
                FirebaseUid = user.FirebaseUid,
                Email = user.Email,
                DisplayName = user.DisplayName,
                Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList() ?? []
            };

            return responseDto;
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database error during SyncFirebaseUserAsync for Firebase UID {FirebaseUid}.", syncRequest.FirebaseUid);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during SyncFirebaseUserAsync for Firebase UID {FirebaseUid}.", syncRequest.FirebaseUid);
            return null;
        }
    }

    public async Task<bool> AssignRoleAsync(int userId, string roleName)
    {
         if (string.IsNullOrWhiteSpace(roleName))
         {
             _logger.LogWarning("AssignRoleAsync called with empty role name for user ID {UserId}.", userId);
             return false;
         }

        try
        {
            await using var ctx = _contextFactory.CreateDbContext();
            var user = await ctx.Users.Include(u => u.UserRoles).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                _logger.LogWarning("AssignRoleAsync: User with ID {UserId} not found.", userId);
                return false;
            }

            var role = await _roleRepository.GetRoleByNameAsync(roleName);
            if (role == null)
            {
                _logger.LogWarning("AssignRoleAsync: Role '{RoleName}' not found.", roleName);
                return false;
            }

            if (!user.UserRoles.Any(ur => ur.RoleId == role.Id))
            {
                user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
                await ctx.SaveChangesAsync();
                _logger.LogInformation("Assigned role '{RoleName}' to user ID {UserId}", role.Name, user.Id);
            }
            else
            {
                 _logger.LogInformation("User ID {UserId} already has role '{RoleName}'", user.Id, role.Name);
            }
            return true;
        }
        catch (DbUpdateException dbEx)
        {
             _logger.LogError(dbEx, "Database error during AssignRoleAsync for user ID {UserId} and role {RoleName}.", userId, roleName);
             return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during AssignRoleAsync for user ID {UserId} and role {RoleName}.", userId, roleName);
            return false;
        }
    }

     public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
     {
         try
        {
            await using var ctx = _contextFactory.CreateDbContext();
            var users = await ctx.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .OrderBy(u => u.DisplayName ?? u.Email)
                .ToListAsync();

             return users.Select(user => new UserResponseDto
             {
                 Id = user.Id,
                 FirebaseUid = user.FirebaseUid,
                 Email = user.Email,
                 DisplayName = user.DisplayName,
                 Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList() ?? new List<string>()
             });
         }
         catch (Exception ex)
         {
            _logger.LogError(ex, "Error retrieving all users.");
            throw;
         }
     }

    // --- Implementation for CreateUserAsync ---
    public async Task<UserResponseDto?> CreateUserAsync(CreateUserRequestDto createUserDto)
    {
        if (createUserDto?.Email == null || createUserDto.Password == null)
        {
            _logger.LogWarning("CreateUserAsync called with missing email or password.");
            return null;
        }

        try
        {
            // Step 1: Create user in Firebase Authentication
            var userArgs = new UserRecordArgs()
            {
                Email = createUserDto.Email,
                Password = createUserDto.Password,
                DisplayName = createUserDto.DisplayName,
                EmailVerified = false, // Or true depending on your flow
                Disabled = false
            };

            UserRecord firebaseUserRecord = await _firebaseAuth.CreateUserAsync(userArgs);
            _logger.LogInformation("Successfully created Firebase user: {FirebaseUid}", firebaseUserRecord.Uid);

            var syncDto = new UserSyncRequestDto
            {
                FirebaseUid = firebaseUserRecord.Uid,
                Email = firebaseUserRecord.Email,
                DisplayName = firebaseUserRecord.DisplayName
            };

            var userResponse = await SyncFirebaseUserAsync(syncDto);

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

            // Optional Step 3: Assign a specific initial role (if DTO included it)
            // ... (logic for assigning non-default role if needed) ...

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
    // --- End of CreateUserAsync ---

} // End of UserService class
// End of namespace