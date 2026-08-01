using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nadixa.Core.Entities;
using Nadixa.Core.Interfaces;

namespace Nadixa.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserManager<AppUser> _userManager;

    public UserRepository(
        UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<int> GetTotalUsersAsync()
    {
        return await _userManager.Users.CountAsync();
    }

    public async Task<int> GetNewUsersCountAsync(
        DateTime date)
    {
        var nextDate = date.AddDays(1);

        return await _userManager.Users
            .CountAsync(u =>
                u.CreatedAt >= date &&
                u.CreatedAt < nextDate);
    }
}
