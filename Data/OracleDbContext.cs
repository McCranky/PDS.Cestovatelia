using Oracle.ManagedDataAccess.Client;
using PDS.Cestovatelia.Models;
using PDS.Cestovatelia.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<bool> InsertUserAsync(UserRegisterRequest user)
        {
            return false;
            using (var connection = GetConnection()) {
                connection.Open();

                //var cmdTxt = "insert into s_user values()";
                connection.Close();
            }
        }

        public async Task<UserInfo> GetUserInfoAsync(UserLoginRequest user)
        {
            var info = new UserInfo { Nickname = user.Nickname, Password = user.Password };

            using (var connection = GetConnection()) {
                connection.Open();

                var cmdTxt = "select name, surname, role_id from s_user where nickname=:nickname and password=:password";
                var cmd = new OracleCommand(cmdTxt, connection);
                cmd.Parameters.Add("nickname", user.Nickname);
                cmd.Parameters.Add("password", user.Password);

                var reader = await cmd.ExecuteReaderAsync();
                if (reader.Read()) {
                    info.Name = reader.GetString(0);
                    info.Surname = reader.GetString(1);
                    info.Role =  (Role)reader.GetInt32(2);
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
