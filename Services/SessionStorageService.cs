using Blazored.SessionStorage;
using PDS.Cestovatelia.Models;
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

        public async Task RemoveUserAsync()
        {
            await _session.RemoveItemAsync("id");
            await _session.RemoveItemAsync("name");
            await _session.RemoveItemAsync("surname");
            await _session.RemoveItemAsync("nickname");
            await _session.RemoveItemAsync("password");
            await _session.RemoveItemAsync("role");
        }

        public async Task SetPostAsync(Post post)
        {
            await _session.SetItemAsync<int>("postId", post.PostId);
            await _session.SetItemAsync<int>("postUserId", post.UserId);
            await _session.SetItemAsync<int>("likes", post.Likes);
            await _session.SetItemAsync<bool>("likedByMe", post.LikedByMe);
            await _session.SetItemAsync<bool>("editableByMe", post.EditableByMe);
            await _session.SetItemAsync<string>("postNickname", post.Nickname);
            await _session.SetItemAsync<string>("pictureSource", post.PictureSource);
            await _session.SetItemAsync<string>("description", post.Description);
            await _session.SetItemAsync<DateTime>("creationDate", post.CreationDate);


            //await _session.SetItemAsync<Post>("post", post);
        }

        public async Task RemovePost()
        {
            await _session.RemoveItemAsync("postId");
            await _session.RemoveItemAsync("postUserId");
            await _session.RemoveItemAsync("likes");
            await _session.RemoveItemAsync("likedByMe");
            await _session.RemoveItemAsync("editableByMe");
            await _session.RemoveItemAsync("postNickname");
            await _session.RemoveItemAsync("pictureSource");
            await _session.RemoveItemAsync("description");
            await _session.RemoveItemAsync("creationDate");

            //await _session.RemoveItemAsync("post");
        }

        public async Task<Post> GetPostAsync()
        {
            var postId = await _session.GetItemAsync<int>("postId");
            var userId = await _session.GetItemAsync<int>("postUserId");
            var likes = await _session.GetItemAsync<int>("likes");
            var liked = await _session.GetItemAsync<bool>("likedByMe");
            var editable = await _session.GetItemAsync<bool>("editableByMe");
            var nick = await _session.GetItemAsync<string>("postNickname");
            var picture = await _session.GetItemAsync<string>("pictureSource");
            var desc = await _session.GetItemAsync<string>("description");
            var date = await _session.GetItemAsync<DateTime>("creationDate");

            //var post = await _session.GetItemAsync<Post>("post");
            //return post;
            return new Post {
                PostId = postId,
                UserId = userId,
                Likes = likes,
                LikedByMe = liked,
                EditableByMe = editable,
                Nickname = nick,
                PictureSource = picture,
                Description = desc,
                CreationDate = date
            };
        }

        public async Task SetSearchUser(SearchUserInfo user)
        {
            await _session.SetItemAsync<SearchUserInfo>("searchUser", user);
        }

        public async Task RemoveSearchUser()
        {
            await _session.RemoveItemAsync("searchUser");
        }
        
        public async Task<SearchUserInfo> GetSearchUser()
        {
            return await _session.GetItemAsync<SearchUserInfo>("searchUser");
        }
    }
}
