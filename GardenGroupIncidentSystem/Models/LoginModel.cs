namespace GardenGroupIncidentSystem.Models
{
    public class LoginModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }

        public LoginModel()
        {
            //
        }

        public LoginModel(string userName, string password)
        {
            UserName = userName;
            Password = password;
        }
    }
}