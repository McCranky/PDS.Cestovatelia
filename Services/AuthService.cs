using PDS.Cestovatelia.Data;
using PDS.Cestovatelia.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PDS.Cestovatelia.Services
{
    public class AuthService
    {
        private OracleDbContext _context;
        private SessionStorageService _session;
        public AuthService(OracleDbContext context, SessionStorageService storage)
        {
            _context = context;
            _session = storage;
        }

        public async Task<bool> RegisterUserAsync(UserRegisterRequest user)
        {
            var result = await _context.InsertUserAsync(user);
            if (result) {
                await _session.SetUserAsync(new UserInfo { 
                    Name = user.Name,
                    Surname = user.Surname,
                    Nickname = user.Nickname,
                    Password = user.Password,
                    Role = Role.User
                });
            }
            return result;
        }

        public async Task<bool> LoginUserAsync(UserLoginRequest user)
        {
            var userInfo = await _context.GetUserInfoAsync(user);
            if (string.IsNullOrWhiteSpace(userInfo.Name)) {
                return false;
            }

            await _session.SetUserAsync(userInfo);
            return true;
        }

        public async Task LogoutCurrentUser()
        {
            await _session.LogOutUserAsync();
        }

        public async Task<UserInfo> GetCurrentUser()
        {
            return await _session.GetUserAsync();
        }
    }
}
