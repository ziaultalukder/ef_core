using Microsoft.AspNetCore.Identity;

namespace EntityFrameWorkWithCore.Models
{
    public class User: IdentityUser
    {
        public int Id { get; set; }
        public string Email { get; set; }=string.Empty;

    }
}
