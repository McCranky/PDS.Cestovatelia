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

        public async Task<bool> InsertPostAsync(PostModel post) //string description, byte[] picture, int userId, string nickname
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
                //return new Post { 
                //    CreationDate = date,
                //    Description = description,
                //    EditableByMe = true,
                //    LikedByMe = false,
                //    Likes = 0,
                //    Nickname = nickname,
                //    PictureSource = $"data:{"image/png"};base64,{Convert.ToBase64String(picture)}",
                //    PostId = id,
                //    UserId = userId
                //};
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
                                join s_user_post using(user_id)
                                join s_post using(post_id)
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
                        Description = reader.GetString(3),
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
                                join s_user_post using(user_id)
                                join s_post post using(post_id))
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
                        Description = reader.GetString(3),
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
                //var test = xmlDoc.SelectSingleNode("//posts").InnerText;
                //var posts = xmlDoc.FirstChild.ChildNodes[0].InnerText;
                //var followers = xmlDoc.FirstChild.ChildNodes[1].InnerText;
                //var following = xmlDoc.FirstChild.ChildNodes[2].InnerText;
                var userStats = new UserStats {
                    PostsCount = int.Parse(xmlDoc.FirstChild.ChildNodes[0].InnerText),
                    FollowersCount = int.Parse(xmlDoc.FirstChild.ChildNodes[1].InnerText),
                    FollowingCount = int.Parse(xmlDoc.FirstChild.ChildNodes[2].InnerText)
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
