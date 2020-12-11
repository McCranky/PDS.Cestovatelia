namespace PDS.Cestovatelia.Models.User
{
    public enum Role
    {
        User = 1,
        Admin = 2,
        MainAdmin = 3
    }

    public class UserInfo
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Nickname { get; set; }
        public string Password { get; set; }
        public Role Role { get; set; }
    }
}
