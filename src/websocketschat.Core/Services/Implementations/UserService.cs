using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using websocketschat.Core.Models;
using websocketschat.Core.Repositories;
using websocketschat.Core.Services.Interfaces;
using CoreModels = websocketschat.Core.Models;

namespace websocketschat.Core.Services.Implementations
{
    /// <inheritdoc cref="IUserService"/>
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserService> _logger;
        public UserService(IUserRepository userRepository, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        /// <summary>
        /// Add user to data storage.
        /// </summary>
        /// <param name="coreUser">User model to save in data storage.</param>
        /// <returns>
        /// true - if user is saved.
        /// false - if something went wrong.
        /// </returns>
        public async Task<bool> AddUserAsync(CoreModels.User coreUser, string password)
        {
            _logger.LogInformation($"Invoked UserService.AddUserAsync with data [{coreUser}]");

            bool userIsExists = await _userRepository
                .UserExistsAsync(username: coreUser.Username);

            if (!userIsExists)
            {
                byte[] passwordHash, passwordHashingKey;

                CreatePasswordHash(password, out passwordHash, out passwordHashingKey);

                // Set "User" type for new created user.
                if (coreUser.Username == "Admin" || coreUser.Username == "root")
                {
                    coreUser.RoleId = 1;
                }
                else if(coreUser.Username == "Bot")
                {
                    coreUser.RoleId = 3;
                }
                else
                {
                    coreUser.RoleId = 2;
                }

                coreUser.IsDeleted = false;
                coreUser.PasswordHash = passwordHash;
                coreUser.PasswordHashingKey = passwordHashingKey;

                try
                {
                    await _userRepository.AddUserAsync(coreUser);

                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogInformation(ex.Message);
                }
            }

            return false;
        }

        /// <summary>
        /// Authenticate user to get token.
        /// </summary>
        /// <param name="username">User username to verify user input with stored data.</param>
        /// <param name="password">User password to verify user input with stored data.</param>
        /// <returns>
        /// CoreModels.User - if user is in storage and credential are correct.
        /// null - if was not found in data storage.
        /// </returns>
        public async Task<bool> Authenticate(string username, string password)
        {
            _logger.LogInformation($"Invoked UserService.Authenticate with data [Username: {username} Password: {password}]");

            bool userIsExists = await _userRepository
                .UserExistsAsync(username: username);

            if (userIsExists)
            {
                CoreModels.User storedUser = await _userRepository.GetUserAsync(username: username);

                try
                {
                    if (VerifyPasswordHash(password, storedUser.PasswordHash, storedUser.PasswordHashingKey))
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogInformation(ex.Message);
                }
            }

            return false;
        }

        public async Task<IEnumerable<CoreModels.User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllUsersAsync();
        }

        public async Task<IEnumerable<CoreModels.User>> GetAllUsersWithRoleAsync()
        {
            return await _userRepository.GetAllUsersWithRolesAsync();
        }

        /// <summary>
        /// Get user by its name.
        /// </summary>
        /// <param name="username"></param>
        /// <returns>
        /// CoreModels.User - if user is found in data storage.
        /// null - if user is not stored.
        /// </returns>
        public async Task<CoreModels.User> GetUserByUsernameAsync(string username)
        {
            _logger.LogInformation($"Invoked UserService.GetUserAsync with data [Username: {username}]");

            bool userIsExists = await _userRepository
               .UserExistsAsync(username: username);

            if (userIsExists)
            {
                try
                {
                    return await _userRepository.GetUserAsync(username);
                }
                catch (Exception ex)
                {
                    _logger.LogInformation(ex.Message);
                }
            }

            return null;
        }

        /// <summary>
        /// Check inputed passsword with stored hash data associated with username.
        /// </summary>
        /// <param name="password">Inputed password from user.</param>
        /// <param name="passwordHash">Stored hash associated with username.</param>
        /// <param name="passwordHashingKey">Stored hashingkey associated with username.</param>
        /// <returns>
        /// true - if inputed password matched to stored.
        /// false - if inputed password didn't matched to stored.
        /// Exception - if occured some exception while executing method.
        /// </returns>
        private static bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordHashingKey)
        {
            if (passwordHash.Length != 64)
            {
                throw new ArgumentException("Invalid length of password hash (64 bytes expected).", "passwordHash");
            }

            if (passwordHashingKey.Length != 128)
            {
                throw new ArgumentException("Invalid length of password salt (128 bytes expected).", "passwordHash");
            }

            using (var hmac = new HMACSHA512(passwordHashingKey))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != passwordHash[i]) return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Create credential data to store in data storage.
        /// </summary>
        /// <param name="password">Inputed password from user.</param>
        /// <param name="passwordHash">Outputed generated passwordHash to be stored.</param>
        /// <param name="passwordHashingKey">Outputed generated passwordHashingKey to be stored.</param>
        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordHashingKey)
        {
            using (HMACSHA512 hmac = new HMACSHA512())
            {
                passwordHashingKey = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        /// <summary>
        /// Updates user model.
        /// </summary>
        /// <param name="user">User to update.</param>
        /// <returns>
        /// user - if update was made with success.
        /// null - if not.
        /// </returns>
        public async Task<User> UpdateUserAsync(User user)
        {
            _logger.LogInformation($"Invoked UserService.UpdateUserAsync with data [Username: {user.Username}]");

            if(await _userRepository.UserExistsByIdAsync(user.Id))
            {
                return await _userRepository.UpdateUserAsync(user);
            }

            return null;
        }

        public async Task<User> GetUserByIdAsync(Guid guid)
        {
            _logger.LogInformation($"Invoked UserService.GetUserByIdAsync with data [Id: {guid}]");

            bool userIsExists = await _userRepository
               .UserExistsByIdAsync(guid: guid);

            if (userIsExists)
            {
                try
                {
                    return await _userRepository.GetUserByIdAsync(guid);
                }
                catch (Exception ex)
                {
                    _logger.LogInformation(ex.Message);
                }
            }

            return null;
        }

        public async Task<User> GetUserWithRoleByUsernameAsync(string username)
        {
            _logger.LogInformation($"Invoked UserService.GetUserWithRoleAsync with data [Username: {username}]");

            bool userIsExists = await _userRepository
               .UserExistsAsync(username: username);

            if (userIsExists)
            {
                try
                {
                    return await _userRepository.GetUserWithRoleAsync(username);
                }
                catch (Exception ex)
                {
                    _logger.LogInformation(ex.Message);
                }
            }

            return null;
        }

        public async Task<User> GetUserWithRoleByIdAsync(Guid guid)
        {
            _logger.LogInformation($"Invoked UserService.GetUserByIdAsync with data [Id: {guid}]");

            bool userIsExists = await _userRepository
               .UserExistsByIdAsync(guid: guid);

            if (userIsExists)
            {
                try
                {
                    return await _userRepository.GetUserWithRoleByIdAsync(guid);
                }
                catch (Exception ex)
                {
                    _logger.LogInformation(ex.Message);
                }
            }

            return null;
        }
    }
}
