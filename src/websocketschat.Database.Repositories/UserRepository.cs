using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using websocketschat.Core.Models;
using websocketschat.Core.Repositories;
using websocketschat.Database.Context;
using DbModels = websocketschat.Database.Models;
using CoreModels = websocketschat.Core.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace websocketschat.Database.Repositories
{
    /// <inheritdoc cref="IUserRepository"/>
    public class UserRepository : BaseRepository, IUserRepository
    {
        private readonly IMapper _mapper;
        public UserRepository(NpgSqlContext context, IMapper mapper)
            : base(context)
        {
            _mapper = mapper;
        }
        public async Task AddUserAsync(CoreModels.User coreUser)
        {
            DbModels.User user = _mapper.Map<DbModels.User>(coreUser);

            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<CoreModels.User>> GetAllUsersAsync()
        {
            IEnumerable<DbModels.User> storedUsers = await _dbContext.Users
                .ToListAsync();

            return _mapper.Map<IEnumerable<CoreModels.User>>(storedUsers);
        }

        public async Task<IEnumerable<User>> GetAllUsersWithRolesAsync()
        {
            IEnumerable<DbModels.User> storedUsers = await _dbContext.Users
                .Include(user => user.Role)
                .ToListAsync();

            return _mapper.Map<IEnumerable<CoreModels.User>>(storedUsers);
        }

        public async Task<User> GetUserAsync(string username)
        {
            DbModels.User storedUser = await _dbContext.Users
                .AsNoTracking()
                .FirstAsync(user => user.Username == username);

            return _mapper.Map<CoreModels.User>(storedUser);
        }

        public async Task<User> GetUserByIdAsync(Guid guid)
        {
            DbModels.User storedUser = await _dbContext.Users
                .AsNoTracking()
                .FirstAsync(user => user.Id == guid);

            return _mapper.Map<CoreModels.User>(storedUser);
        }

        public async Task<User> GetUserWithRoleAsync(string username)
        {
            DbModels.User storedUser = await _dbContext.Users
                .Include(user => user.Role)
                .AsNoTracking()
                .FirstAsync(user => user.Username == username);

            return _mapper.Map<CoreModels.User>(storedUser);
        }

        public async Task<User> GetUserWithRoleByIdAsync(Guid guid)
        {
            DbModels.User storedUser = await _dbContext.Users
                .Include(user => user.Role)
                .AsNoTracking()
                .FirstAsync(user => user.Id == guid);

            return _mapper.Map<CoreModels.User>(storedUser);
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            DbModels.User updatedUser = _mapper.Map<DbModels.User>(user);

            _dbContext.Users.Update(updatedUser);
            await _dbContext.SaveChangesAsync();

            return _mapper.Map<CoreModels.User>(updatedUser);
        }

        public async Task<bool> UserExistsAsync(string username)
        {
            return await _dbContext.Users.AnyAsync(user => user.Username == username);
        }

        public async Task<bool> UserExistsByIdAsync(Guid guid)
        {
            return await _dbContext.Users.AnyAsync(user => user.Id == guid);
        }
    }
}
