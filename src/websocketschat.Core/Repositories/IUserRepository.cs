using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using websocketschat.Core.Models;

namespace websocketschat.Core.Repositories
{
    /// <summary>
    /// Defines User repository interface.
    /// </summary>
    public interface IUserRepository
    {
        Task AddUserAsync(User coreUser);
        Task<User> GetUserAsync(string username);
        Task<User> GetUserByIdAsync(Guid guid);
        Task<User> GetUserWithRoleAsync(string username);
        Task<User> GetUserWithRoleByIdAsync(Guid guid);
        Task<bool> UserExistsAsync(string username);
        Task<bool> UserExistsByIdAsync(Guid guid);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<IEnumerable<User>> GetAllUsersWithRolesAsync();
        Task<User> UpdateUserAsync(User user);
    }
}
