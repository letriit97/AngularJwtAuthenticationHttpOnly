using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class Accounts
    {
        [Key]
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public class AccountDto
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}