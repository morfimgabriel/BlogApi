using System.Text.Json.Serialization;

namespace Blog.Models
{
    public class UserRole
    {
        public int Id { get; set; }
        public IList<User> Users { get; set; }
        public IList<Role> Roles { get; set; }
    }
}