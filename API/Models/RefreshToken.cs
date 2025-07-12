using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class RefreshToken
    {
        [Key]
        public string UserName { get; set; }

        [Key]
        public string Token { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime ExpiredTime { get; set; }
    }
}