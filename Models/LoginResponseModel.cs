namespace WebApplication3.Models
{
    public class LoginResponseModel
    {
        public decimal EmployeeCode { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public string token { get; set; }
        public string auth { get; set; }
    }
}