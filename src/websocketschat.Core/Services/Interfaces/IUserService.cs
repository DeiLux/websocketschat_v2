using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CoreModels = websocketschat.Core.Models;

namespace websocketschat.Core.Services.Interfaces
{
    /// <summary>
    /// Defines User service interface.
    /// </summary>
    public interface IUserService
    {
        #region CRUD Async methods
        Task<bool> AddUserAsync(CoreModels.User coreUser, string password);
        Task<IEnumerable<CoreModels.User>> GetAllUsersAsync();
        Task<IEnumerable<CoreModels.User>> GetAllUsersWithRoleAsync();
        Task<CoreModels.User> GetUserByUsernameAsync(string username);
        Task<CoreModels.User> GetUserWithRoleByUsernameAsync(string username);
        Task<CoreModels.User> GetUserByIdAsync(Guid guid);
        Task<CoreModels.User> GetUserWithRoleByIdAsync(Guid guid);
        Task<CoreModels.User> UpdateUserAsync(CoreModels.User user);
        #endregion

        #region Auth methods
        Task<bool> Authenticate(string username, string password);
        #endregion
    }
}
