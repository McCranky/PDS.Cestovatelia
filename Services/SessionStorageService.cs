using Blazored.SessionStorage;
using PDS.Cestovatelia.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PDS.Cestovatelia.Services
{
    public class SessionStorageService
    {
        private ISessionStorageService _session;
        public SessionStorageService(ISessionStorageService sessionStorage)
        {
            _session = sessionStorage;
        }

        public async Task<UserInfo> GetUserAsync()
        {
            var id = await _session.GetItemAsync<int>("id");
            var name = await _session.GetItemAsync<string>("name");
            var surname = await _session.GetItemAsync<string>("surname");
            var nickname = await _session.GetItemAsync<string>("nickname");
            var password = await _session.GetItemAsync<string>("password");
            var role = await _session.GetItemAsync<int>("role");
            return new UserInfo { Id = id, Name = name, Surname = surname, Nickname = nickname, Password = password, Role = (Role)role };
        }

        public async Task SetUserAsync(UserInfo user)
        {
            await _session.SetItemAsync<int>("id", user.Id);
            await _session.SetItemAsync<string>("name", user.Name);
            await _session.SetItemAsync<string>("surname", user.Surname);
            await _session.SetItemAsync<string>("nickname", user.Nickname);
            await _session.SetItemAsync<string>("password", user.Password);
            await _session.SetItemAsync<int>("role", (int)user.Role);
        }

        public async Task LogOutUserAsync()
        {
            await _session.RemoveItemAsync("id");
            await _session.RemoveItemAsync("name");
            await _session.RemoveItemAsync("surname");
            await _session.RemoveItemAsync("nickname");
            await _session.RemoveItemAsync("password");
            await _session.RemoveItemAsync("role");
        }
    }
}
