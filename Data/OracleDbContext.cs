using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using PDS.Cestovatelia.Models;
using PDS.Cestovatelia.Models.User;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace PDS.Cestovatelia.Data
{
    public class OracleDbContext
    {
        public string ConnectionString { get; set; }
        public OracleDbContext(string connectionString)
        {
            ConnectionString = connectionString;
        }

        private OracleConnection GetConnection()
        {
            return new OracleConnection(ConnectionString);
        }

        public async Task<bool> GenerateReportAsync()
        {
            using (var connection = GetConnection()) {
                connection.Open();

                var cmdTxt = "create_report";
                var cmd = new OracleCommand(cmdTxt, connection);
                cmd.CommandType = CommandType.StoredProcedure;

                await cmd.ExecuteNonQueryAsync();
                connection.Close();
                return true;
            }
        }

        public async Task<string> GetReportAsync()
        {
            using (var connection = GetConnection()) {
                connection.Open();

                var cmdTxt = @"select report from os_social_stats@db_link_os
                                 order by creation_date desc
                                 fetch first 1 row only";
                var cmd = new OracleCommand(cmdTxt, connection);
                var reader = await cmd.ExecuteReaderAsync();

                //XmlDocument xmlDoc = new XmlDocument();
                var xmlString = "";
                if (reader.Read()) {
                    //xmlDoc.LoadXml(reader.GetString(0));
                    xmlString = reader.GetString(0);
                }

                connection.Close();
                return xmlString;
            }
        }

        public async Task<Post> GetMostLikedPostAsync(int currentUserId, int roleId)
        {
            using (var connection = GetConnection()) {
                connection.Open();

                var cmdTxt = @"select * from (select p.post_id, row_number() over(order by (select count(1) 
                                from table(like_list)) desc) as rn 
                                from s_post p) where rn = 1";
                var cmd = new OracleCommand(cmdTxt, connection);
                var result = (int)(decimal)await cmd.ExecuteScalarAsync();

                connection.Close();
                return await GetPostAsync(currentUserId, roleId, result);
            }
        }

        public async Task<Post> GetMostCommentedPostAsync(int currentUserId, int roleId)
        {
            using (var connection = GetConnection()) {
                connection.Open();

                var cmdTxt = @"select * from (select p.post_id, 
                                row_number() over(order by count(*) desc)  as rn 
                                from s_post p 
                                join s_comment c on c.post_id = p.post_id  
                                group by p.post_id) where rn = 1";
                var cmd = new OracleCommand(cmdTxt, connection);
                var result = (int)(decimal)await cmd.ExecuteScalarAsync();

                connection.Close();
                return await GetPostAsync(currentUserId, roleId, result);
            }
        }

        public async Task<List<Post>> GetThreeMostLikedPostsAsync(int currentUserId, int roleId)
        {
            var postsIds = new List<int>();
            using (var connection = GetConnection()) {
                connection.Open();

                var cmdTxt = @$"select post_id
                                from (select post_id, 
                                        row_number() over(order by (select count(1) from table(like_list)) desc) as rn
                                        from s_user
                                            join s_post using(user_id)
                                                where user_id = :currentUserId)
                                                    where rn < 4";
                var cmd = new OracleCommand(cmdTxt, connection);
                cmd.Parameters.Add("currentUserId", currentUserId);

                var reader = await cmd.ExecuteReaderAsync();
                while (reader.Read()) {
                    postsIds.Add(reader.GetInt32(0));
                }
                connection.Close();
            }
            var posts = new List<Post>();
            foreach (var postId in postsIds) {
                var post = await GetPostAsync(currentUserId, roleId, postId);
                posts.Add(post);
            }
            return posts;
        }

        public async Task<List<Post>> GetThreeMostCommentedPostsAsync(int currentUserId, int roleId)
        {
            var postsIds = new List<int>();
            using (var connection = GetConnection()) {
                connection.Open();

                var cmdTxt = @$"select post_id from (select p.post_id, row_number() over(order by count(*) desc) as rn from s_post p
                                    join s_comment c on c.post_id = p.post_id
                                    where p.user_id = :currentUserId
                                    group by p.post_id)
                                    where rn < 4";
                var cmd = new OracleCommand(cmdTxt, connection);
                cmd.Parameters.Add("currentUserId", currentUserId);

                var reader = await cmd.ExecuteReaderAsync();
                while (reader.Read()) {
                    postsIds.Add(reader.GetInt32(0));
                }
                connection.Close();
            }
            var posts = new List<Post>();
            foreach (var postId in postsIds) {
                var post = await GetPostAsync(currentUserId, roleId, postId);
                posts.Add(post);
            }
            return posts;
        }

        public async Task<List<Post>> GetThreeMostLikedPostsOfMyFollowersAsync(int currentUserId, int roleId)
        {
            var postsIds = new List<int>();
            using (var connection = GetConnection()) {
                connection.Open();

                var cmdTxt = @$"select post_id from (select p.post_id, row_number() over(order by (select count(1) from table(like_list)) desc) as rn 
                                from s_user_follower uf 
                                join s_user u on u.user_id = uf.follower_id 
                                join s_post p on p.user_id = u.user_id 
                                where uf.user_id = :currentUserId)  
                                where rn < 4";
                var cmd = new OracleCommand(cmdTxt, connection);
                cmd.Parameters.Add("currentUserId", currentUserId);

                var reader = await cmd.ExecuteReaderAsync();
                while (reader.Read()) {
                    postsIds.Add(reader.GetInt32(0));
                }
                connection.Close();
            }
            var posts = new List<Post>();
            foreach (var postId in postsIds) {
                var post = await GetPostAsync(currentUserId, roleId, postId);
                posts.Add(post);
            }
            return posts;
        }

        public async Task<List<Post>> GetThreeMostCommentedPostsOfMyFollowersAsync(int currentUserId, int roleId)
        {
            var postsIds = new List<int>();
            using (var connection = GetConnection()) {
                connection.Open();

                var cmdTxt = @$"select post_id from (select p.post_id, row_number() over(order by count(*) desc) as rn 
                                from s_user_follower uf join s_user u on u.user_id = uf.follower_id 
                                join s_post p on p.user_id = u.user_id join s_comment c on c.post_id = p.post_id 
                                where uf.user_id = :currentUserId group by p.post_id)  
                                where rn < 4";
                var cmd = new OracleCommand(cmdTxt, connection);
                cmd.Parameters.Add("currentUserId", currentUserId);

                var reader = await cmd.ExecuteReaderAsync();
                while (reader.Read()) {
                    postsIds.Add(reader.GetInt32(0));
                }
                connection.Close();
            }
            var posts = new List<Post>();
            foreach (var postId in postsIds) {
                var post = await GetPostAsync(currentUserId, roleId, postId);
                posts.Add(post);
            }
            return posts;
        }

        public async Task<int> GetCountOfMonthlyPostsAsync(int currentUserId)
        {
            using (var connection = GetConnection()) {
                connection.Open();

                var quarry = @"select count(*) from s_post p
                                where extract(year from created_at) = extract(year from sysdate)
                                and extract(month from created_at) = extract(month from sysdate) 
                                and user_id = :currentUserId";
                var cmd = new OracleCommand(quarry, connection);
                cmd.Parameters.Add("currentUserId", currentUserId);

                var result = (int)(decimal)await cmd.ExecuteScalarAsync();
                connection.Close();
                return result;
            }
        }

        public async Task<int> GetCountOfWeeklyPostsAsync(int currentUserId)
        {
            using (var connection = GetConnection()) {
                connection.Open();

                var quarry = @"select count(*) from s_post p
                                where extract(year from created_at) = extract(year from sysdate)
                                and extract(month from created_at) = extract(month from sysdate)
                                and to_char(created_at, 'W') = to_char(sysdate, 'W')
                                and user_id = :currentUserId";
                var cmd = new OracleCommand(quarry, connection);
                cmd.Parameters.Add("currentUserId", currentUserId);

                var result = (int)(decimal)await cmd.ExecuteScalarAsync();
                connection.Close();
                return result;
            }
        }

        public async Task<int> GetCountOfMonthlyLikesGivenAsync(int currentUserId)
        {
            using (var connection = GetConnection()) {
                connection.Open();

                var quarry = @"select count(*) from s_post 
                                where extract(year from created_at) = extract(year from sysdate)
                                and extract(month from created_at) = extract(month from sysdate)
                                and exists (
                                    select * from table(like_list)
                                    where column_value = :currentUserId)";
                var cmd = new OracleCommand(quarry, connection);
                cmd.Parameters.Add("currentUserId", currentUserId);

                var result = (int)(decimal)await cmd.ExecuteScalarAsync();
                connection.Close();
                return result;
            }
        }

        public async Task<int> GetCountOfMonthlyCommentsGivenAsync(int currentUserId)
        {
            using (var connection = GetConnection()) {
                connection.Open();

                var quarry = @"select count(*) from s_comment 
                                where extract(year from created_at) = extract(year from sysdate)
                                and extract(month from created_at) = extract(month from sysdate)
                                and user_id = :currentUserId";
                var cmd = new OracleCommand(quarry, connection);
                cmd.Parameters.Add("currentUserId", currentUserId);

                var result = (int)(decimal)await cmd.ExecuteScalarAsync();
                connection.Close();
                return result;
            }
        }

        public async Task<int> GetCountOfWeeklyCommentsGivenAsync(int currentUserId)
        {
            using (var connection = GetConnection()) {
                connection.Open();

                var quarry = @"select count(*) from s_comment 
                                where extract(year from created_at) = extract(year from sysdate)
                                and extract(month from created_at) = extract(month from sysdate)
                                and to_char(created_at, 'W') = to_char(sysdate, 'W')
                                and user_id = :currentUserId";
                var cmd = new OracleCommand(quarry, connection);
                cmd.Parameters.Add("currentUserId", currentUserId);

                var result = (int)(decimal)await cmd.ExecuteScalarAsync();
                connection.Close();
                return result;
            }
        }

        public async Task<Post> GetPostAsync(int currentUserId, int roleId, int postId)
        {
            using (var connection = GetConnection()) {
                connection.Open();

                var cmdTxt = @"select user_id, nickname, post_id, description, picture, created_at, (select count(1) from table(like_list)) as like_count,
                                (select count(1) from table(like_list) where column_value = :currentUserId) as liked_by_user,
                                (case when :roleId > 1 or user_id = :currentUserId then 1 else 0 end) as able_to_edit
                                from
                                (select user_id, nickname, role_id, post_id, description, picture, created_at, like_list,
                                row_number() over(order by created_at desc) as rn from
                                ( select user_id, nickname, role_id from s_user su
                                join s_user_follower suf using(user_id)
                                where follower_id = :currentUserId
                                union
                                select user_id, nickname, role_id from s_user
                                where user_id = :currentUserId) interesting_posts
                                join s_post post using(user_id))
                                where post_id = :postId";
                var cmd = new OracleCommand(cmdTxt, connection);
                cmd.Parameters.Add("currentUserId", currentUserId);
                cmd.Parameters.Add("roleId", roleId);
                cmd.Parameters.Add("currentUserId", currentUserId);
                cmd.Parameters.Add("currentUserId", currentUserId);
                cmd.Parameters.Add("currentUserId", currentUserId);
                cmd.Parameters.Add("postId", postId);
                var reader = await cmd.ExecuteReaderAsync();


                Post post = null;
                if (reader.Read()) {
                    post = new Post {
                        UserId = reader.GetInt32(0),
                        Nickname = reader.GetString(1),
                        PostId = reader.GetInt32(2),
                        Description = reader.IsDBNull(3) ? "" : reader.GetString(3),
                        PictureSource = $"data:{"image/png"};base64,{Convert.ToBase64String((byte[])reader.GetValue(4))}",
                        CreationDate = reader.GetDateTime(5),
                        Likes = reader.GetInt32(6),
                        LikedByMe = reader.GetInt32(7) > 0,
                        EditableByMe = reader.GetInt32(8) > 0
                    };
                }
                connection.Close();
                return post;
            }
        }

        public async Task<bool> InsertFollowAsync(int currentUserId, int userId)
        {
            using (var connection = GetConnection()) {
                connection.Open();

                var cmdTxt = "follow_user";
                var cmd = new OracleCommand(cmdTxt, connection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("p_followed_user_id", OracleDbType.Int32, 32, userId, ParameterDirection.Input);
                cmd.Parameters.Add("p_follower_user_id", OracleDbType.Int32, 32, currentUserId, ParameterDirection.Input);
                cmd.Parameters.Add("p_date", OracleDbType.Date, 69, DateTime.Now, ParameterDirection.Input);
                cmd.Parameters.Add("p_output", OracleDbType.Int32).Direction = ParameterDirection.Output;

                await cmd.ExecuteNonQueryAsync();
                connection.Close();

                return true;
            }
        }

        public async Task<bool> DeleteFollowAsync(int currentUserId, int userId)
        {
            using (var connection = GetConnection()) {
                connection.Open();

                var cmdTxt = "unfollow_user";
                var cmd = new OracleCommand(cmdTxt, connection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("p_followed_user_id", OracleDbType.Int32, 32, userId, ParameterDirection.Input);
                cmd.Parameters.Add("p_follower_user_id", OracleDbType.Int32, 32, currentUserId, ParameterDirection.Input);
                cmd.Parameters.Add("p_output", OracleDbType.Int32).Direction = ParameterDirection.Output;

                await cmd.ExecuteNonQueryAsync();
                connection.Close();

                return true;
            }
        }

        public async Task<List<SearchUserInfo>> GetUsersLikeAsync(int currentUserId, string quarryNickname)
        {
            var users = new List<SearchUserInfo>();
            using (var connection = GetConnection()) {
                connection.Open();

                var cmdTxt = @$"select user_id, name, surname, nickname, (select count(1) from s_user_follower suf
                                where suf.user_id = su.user_id and suf.follower_id = :currentUserId) as followed_by_me
                                from s_user su where nickname like '%{quarryNickname}%'";
                var cmd = new OracleCommand(cmdTxt, connection);
                cmd.Parameters.Add("currentUserId", currentUserId);

                var reader = await cmd.ExecuteReaderAsync();
                while (reader.Read()) {
                    users.Add(new SearchUserInfo { 
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Surname = reader.GetString(2),
                        Nickname = reader.GetString(3),
                        Following = reader.GetInt32(4) > 0
                    });
                }

                connection.Close();
                return users;
            }
        }
        
        public async Task<List<SearchUserInfo>> GetFollowersForAsync(int currentUserId, int userId)
        {
            var users = new List<SearchUserInfo>();
            using (var connection = GetConnection()) {
                connection.Open();

                var cmdTxt = @"select su.user_id, name, surname, nickname, (select count(1) from s_user_follower suf1
                                where suf1.user_id = su.user_id and suf1.follower_id = :currentUserId) as followed_by_me
                                from s_user su
                                join s_user_follower suf on (suf.user_id = su.user_id)
                                where follower_id = :userId";
                var cmd = new OracleCommand(cmdTxt, connection);
                cmd.Parameters.Add("currentUserId", currentUserId);
                cmd.Parameters.Add("userId", userId);

                var reader = await cmd.ExecuteReaderAsync();
                while (reader.Read()) {
                    users.Add(new SearchUserInfo { 
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Surname = reader.GetString(2),
                        Nickname = reader.GetString(3),
                        Following = reader.GetInt32(4) > 0
                    });
                }

                connection.Close();
                return users;
            }
        }
        
        public async Task<List<SearchUserInfo>> GetFollowersOfAsync(int currentUserId, int userId)
        {
            var users = new List<SearchUserInfo>();
            using (var connection = GetConnection()) {
                connection.Open();

                var cmdTxt = @"select su.user_id, name, surname, nickname, (select count(1) from s_user_follower suf1
                                where suf1.user_id = su.user_id and suf1.follower_id = :currentUserId) as followed_by_me
                                from s_user su
                                join s_user_follower suf on (su.user_id = suf.follower_id)
                                where suf.user_id = :userId";
                var cmd = new OracleCommand(cmdTxt, connection);
                cmd.Parameters.Add("currentUserId", currentUserId);
                cmd.Parameters.Add("userId", userId);

                var reader = await cmd.ExecuteReaderAsync();
                while (reader.Read()) {
                    users.Add(new SearchUserInfo { 
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Surname = reader.GetString(2),
                        Nickname = reader.GetString(3),
                        Following = reader.GetInt32(4) > 0
                    });
                }

                connection.Close();
                return users;
            }
        }

        public async Task<bool> DeleteCommentAsync(int commentId)
        {
            using (var connection = GetConnection()) {
                connection.Open();

                var cmdTxt = "delete_comment";
                var cmd = new OracleCommand(cmdTxt, connection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("p_comment_id", OracleDbType.Int32, 32, commentId, ParameterDirection.Input);
                cmd.Parameters.Add("p_output", OracleDbType.Int32).Direction = ParameterDirection.Output;

                await cmd.ExecuteNonQueryAsync();
                connection.Close();

                return true;
            }
        }

        public async Task<bool> DeletePostAsync(int postId)
        {
            using (var connection = GetConnection()) {
                connection.Open();

                var cmdTxt = "delete_post";
                var cmd = new OracleCommand(cmdTxt, connection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("p_post_id", OracleDbType.Int32, 32, postId, ParameterDirection.Input);
                cmd.Parameters.Add("p_output", OracleDbType.Int32).Direction = ParameterDirection.Output;

                await cmd.ExecuteNonQueryAsync();
                connection.Close();

                return true;
            }
        }

        public async Task<List<Comment>> GetAllCommentsAsync(int postId, int userId, int roleId)
        {
            var comments = new List<Comment>();

            using (var connection = GetConnection()) {
                connection.Open();

                var cmdTxt = @"select comment_id, user_id, nickname, text, created_at, able_to_edit from
                                (select comment_id, user_id, nickname, text, created_at,
                                (case when :roleId > 1 or user_id = :userId then 1 else 0 end) as able_to_edit
                                from s_comment
                                join s_user using(user_id)
                                where post_id = :postId)";
                var cmd = new OracleCommand(cmdTxt, connection);
                cmd.Parameters.Add("roleId", roleId);
                cmd.Parameters.Add("userId", userId);
                cmd.Parameters.Add("postId", postId);

                var reader = await cmd.ExecuteReaderAsync();
                while (reader.Read()) {
                    comments.Add(new Comment { 
                        CommentId = reader.GetInt32(0),
                        UserId = reader.GetInt32(1),
                        Nickname = reader.GetString(2),
                        Text = reader.GetString(3),
                        CreationDate = reader.GetDateTime(4),
                        EditableByMe = reader.GetInt32(5) > 0
                    });
                }

                connection.Close();
                return comments;
            }
        }

        public async Task<Comment> InsertCommentAsync(CommentModel comment)
        {
            using (var connection = GetConnection()) {
                connection.Open();

                var date = DateTime.Now;
                var cmdTxt = "add_comment";
                var cmd = new OracleCommand(cmdTxt, connection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("p_user_id", OracleDbType.Int32, 32, comment.UserId, ParameterDirection.Input);
                cmd.Parameters.Add("p_post_id", OracleDbType.Int32, 32, comment.PostId, ParameterDirection.Input);
                cmd.Parameters.Add("p_date", OracleDbType.Date, 69, date, ParameterDirection.Input);
                cmd.Parameters.Add("p_text", OracleDbType.Varchar2, 200, comment.Text, ParameterDirection.Input);
                cmd.Parameters.Add("p_output", OracleDbType.Int32).Direction = ParameterDirection.Output;

                await cmd.ExecuteNonQueryAsync();
                connection.Close();

                return new Comment {
                    CommentId = int.Parse(cmd.Parameters["p_output"].Value.ToString()),
                    Text = comment.Text,
                    CreationDate = date,
                    EditableByMe = true,
                    UserId = comment.UserId
                };
            }
        }

        public async Task<bool> InsertLikeAsync(int userId, int postId)
        {
            using (var connection = GetConnection()) {
                connection.Open();

                var date = DateTime.Now;
                var cmdTxt = "like_post";
                var cmd = new OracleCommand(cmdTxt, connection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("p_user_id", OracleDbType.Int32, 32, userId, ParameterDirection.Input);
                cmd.Parameters.Add("p_post_id", OracleDbType.Int32, 32, postId, ParameterDirection.Input);
                cmd.Parameters.Add("p_result", OracleDbType.Int32).Direction = ParameterDirection.Output;

                await cmd.ExecuteNonQueryAsync();
                connection.Close();

                return true;
            }
        }

        public async Task<bool> DeleteLikeAsync(int userId, int postId)
        {
            using (var connection = GetConnection()) {
                connection.Open();

                var date = DateTime.Now;
                var cmdTxt = "dislike_post";
                var cmd = new OracleCommand(cmdTxt, connection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("p_user_id", OracleDbType.Int32, 32, userId, ParameterDirection.Input);
                cmd.Parameters.Add("p_post_id", OracleDbType.Int32, 32, postId, ParameterDirection.Input);
                cmd.Parameters.Add("p_result", OracleDbType.Int32).Direction = ParameterDirection.Output;

                await cmd.ExecuteNonQueryAsync();
                connection.Close();

                return true;
            }
        }

        public async Task<bool> InsertPostAsync(PostModel post)
        {
            using (var connection = GetConnection()) {
                connection.Open();

                var date = DateTime.Now;
                var cmdTxt = "add_post";
                var cmd = new OracleCommand(cmdTxt, connection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("p_desc", OracleDbType.Varchar2, 200, post.Description, ParameterDirection.Input);
                cmd.Parameters.Add("p_pic", OracleDbType.Blob, post.Picture.Length, post.Picture, ParameterDirection.Input);
                cmd.Parameters.Add("p_date", OracleDbType.Date, 69, date, ParameterDirection.Input);
                cmd.Parameters.Add("p_user_id", OracleDbType.Int32, 32, post.UserId, ParameterDirection.Input);
                cmd.Parameters.Add("p_output", OracleDbType.Int32).Direction = ParameterDirection.Output;

                await cmd.ExecuteNonQueryAsync();
                var id = int.Parse(cmd.Parameters["p_output"].Value.ToString());
                connection.Close();

                return true;
            }
        }

        public async Task<List<Post>> GetAllUsersPostsAsync(int currentUserId, int roleId, int userId, int actualPage = 1)
        {
            var posts = new List<Post>();
            using (var connection = GetConnection()) {
                connection.Open();

                var cmdTxt = @"select user_id, nickname, post_id, description, picture, created_at, (select count(1) from table(like_list)) as like_count,
                                (select count(1) from table(like_list) where column_value = :currentUserId) as liked_by_user, 
                                able_to_edit from (select user_id, nickname, post_id, description, picture, created_at, like_list,
                                (case when :roleId > 1 or user_id = :currentUserId then 1 else 0 end) as able_to_edit,
                                row_number() over(order by created_at) as rn
                                from s_user
                                join s_post using(user_id)
                                where user_id=:userId)
                                where rn between ((:actualPage-1) * 12 + 1) and ( :actualPage * 12)";
                var cmd = new OracleCommand(cmdTxt, connection);
                cmd.Parameters.Add("currentUserId", currentUserId);
                cmd.Parameters.Add("roleId", roleId);
                cmd.Parameters.Add("currentUserId", currentUserId);
                cmd.Parameters.Add("userId", userId);
                cmd.Parameters.Add("actualPage", actualPage);
                cmd.Parameters.Add("actualPage", actualPage);
                var reader = await cmd.ExecuteReaderAsync();

                while (reader.Read()) {
                    posts.Add(new Post { 
                        UserId = reader.GetInt32(0),
                        Nickname = reader.GetString(1),
                        PostId = reader.GetInt32(2),
                        Description = reader.IsDBNull(3) ? "" : reader.GetString(3),
                        PictureSource = $"data:{"image/png"};base64,{Convert.ToBase64String((byte[])reader.GetValue(4))}",
                        CreationDate = reader.GetDateTime(5),
                        Likes = reader.GetInt32(6),
                        LikedByMe = reader.GetInt32(7) > 0,
                        EditableByMe = reader.GetInt32(8) > 0
                    });
                }

                connection.Close();
            }
            return posts;
        }
        
        public async Task<List<Post>> GetAllFollowersPostsAsync(int currentUserId, int roleId, int actualPage = 1)
        {
            var posts = new List<Post>();
            using (var connection = GetConnection()) {
                connection.Open();

                var cmdTxt = @"select user_id, nickname, post_id, description, picture, created_at, (select count(1) from table(like_list)) as like_count,
                                (select count(1) from table(like_list) where column_value = :currentUserId) as liked_by_user,
                                (case when :roleId > 1 or user_id = :currentUserId then 1 else 0 end) as able_to_edit
                                from
                                (select user_id, nickname, role_id, post_id, description, picture, created_at, like_list,
                                row_number() over(order by created_at desc) as rn from
                                ( select user_id, nickname, role_id from s_user su
                                join s_user_follower suf using(user_id)
                                where follower_id = :currentUserId
                                union
                                select user_id, nickname, role_id from s_user
                                where user_id = :currentUserId) interesting_posts
                                join s_post post using(user_id))
                                where rn between ((:actualPage - 1) * 12 + 1) and (:actualPage*12)";
                var cmd = new OracleCommand(cmdTxt, connection);
                cmd.Parameters.Add("currentUserId", currentUserId);
                cmd.Parameters.Add("roleId", roleId);
                cmd.Parameters.Add("currentUserId", currentUserId);
                cmd.Parameters.Add("currentUserId", currentUserId);
                cmd.Parameters.Add("currentUserId", currentUserId);
                cmd.Parameters.Add("actualPage", actualPage);
                cmd.Parameters.Add("actualPage", actualPage);
                var reader = await cmd.ExecuteReaderAsync();

                while (reader.Read()) {
                    posts.Add(new Post { 
                        UserId = reader.GetInt32(0),
                        Nickname = reader.GetString(1),
                        PostId = reader.GetInt32(2),
                        Description = reader.IsDBNull(3) ? "" : reader.GetString(3),
                        PictureSource = $"data:{"image/png"};base64,{Convert.ToBase64String((byte[])reader.GetValue(4))}",
                        CreationDate = reader.GetDateTime(5),
                        Likes = reader.GetInt32(6),
                        LikedByMe = reader.GetInt32(7) > 0,
                        EditableByMe = reader.GetInt32(8) > 0
                    });
                }

                connection.Close();
            }
            return posts;
        }

        public async Task<UserStats> GetUserStats(int userId)
        {
            using (var connection = GetConnection()) {
                connection.Open();

                var cmdTxt = "get_user_stats";
                var cmd = new OracleCommand(cmdTxt, connection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("p_user_id", OracleDbType.Int32, 32, userId, ParameterDirection.Input);
                cmd.Parameters.Add("p_xml_output", OracleDbType.XmlType).Direction = ParameterDirection.Output;
                
                await cmd.ExecuteNonQueryAsync();

                var xmlDoc = ((OracleXmlType)cmd.Parameters["p_xml_output"].Value).GetXmlDocument();

                var userStats = new UserStats {
                    PostsCount = int.Parse(xmlDoc.FirstChild.ChildNodes[0].InnerText),
                    FollowingCount = int.Parse(xmlDoc.FirstChild.ChildNodes[1].InnerText),
                    FollowersCount = int.Parse(xmlDoc.FirstChild.ChildNodes[2].InnerText)
                };
                connection.Close();
                return userStats;
            }
        }

        public async Task<int> InsertUserAsync(UserRegisterRequest user)
        {
            using (var connection = GetConnection()) {
                connection.Open();

                var cmdTxt = "register_new_user";
                var cmd = new OracleCommand(cmdTxt, connection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("p_name", OracleDbType.Varchar2, 50, user.Name, ParameterDirection.Input);
                cmd.Parameters.Add("p_surname", OracleDbType.Varchar2, 50, user.Surname, ParameterDirection.Input);
                cmd.Parameters.Add("p_nickname", OracleDbType.Varchar2, 30, user.Nickname, ParameterDirection.Input);
                cmd.Parameters.Add("p_password", OracleDbType.Varchar2, 30, user.Password, ParameterDirection.Input);
                cmd.Parameters.Add("out_result", OracleDbType.Int32).Direction = ParameterDirection.Output;

                await cmd.ExecuteNonQueryAsync();
                connection.Close();
                
                return int.Parse(cmd.Parameters["out_result"].Value.ToString());
            }
        }

        public async Task<UserInfo> GetUserInfoAsync(UserLoginRequest user)
        {
            var info = new UserInfo { Nickname = user.Nickname, Password = user.Password };

            using (var connection = GetConnection()) {
                connection.Open();

                var cmdTxt = "select name, surname, role_id, user_id from s_user where nickname=:nickname and password=:password";
                var cmd = new OracleCommand(cmdTxt, connection);
                cmd.Parameters.Add("nickname", user.Nickname);
                cmd.Parameters.Add("password", user.Password);

                var reader = await cmd.ExecuteReaderAsync();
                if (reader.Read()) {
                    info.Name = reader.GetString(0);
                    info.Surname = reader.GetString(1);
                    info.Role =  (Role)reader.GetInt32(2);
                    info.Id =  reader.GetInt32(3);
                }

                connection.Close();
            }
            return info;
        }

        public async Task<byte[]> SelectImageAsync(string title)
        {
            using (var connection = GetConnection()) {
                connection.Open();

                var quarry = "select image from img_table where title=:title";
                var cmd = new OracleCommand(quarry, connection);
                cmd.Parameters.Add("title", title);

                var result = (byte[])await cmd.ExecuteScalarAsync();
                connection.Close();
                return result;
            }
        }

        public async Task<int> InsertImageAsync(ImageModel img)
        {
            using (var connection = GetConnection()) {
                connection.Open();

                var quarry = "insert into img_table(title,image) values(:title,:image)";
                var cmd = new OracleCommand(quarry, connection);
                cmd.Parameters.Add("title", img.Title);
                cmd.Parameters.Add("image", img.Image);

                var result = await cmd.ExecuteNonQueryAsync();
                connection.Close();
                return result;
            }
        }

        public async Task<List<Osoba>> GetAllAsync()
        {
            var list = new List<Osoba>();
            using (var connection = GetConnection()) {
                connection.Open();

                var quarry = "select * from p_osoba";
                var cmd = new OracleCommand(quarry, connection);

                using (var reader = await cmd.ExecuteReaderAsync()) {
                    while (reader.Read()) {
                        list.Add(new Osoba { 
                            rod_cislo = reader.GetString(0),
                            meno = reader.GetString(1),
                            priezvisko = reader.GetString(2),
                            PSC = reader.GetString(3),
                            ulica = reader.IsDBNull(4) ? "" : reader.GetString(4)
                        });
                    }
                }
                connection.Close();
            }

            return list;
        }
    }
}
