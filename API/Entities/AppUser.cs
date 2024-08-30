using Microsoft.EntityFrameworkCore;

namespace API.Entities
{
    public class AppUser
    {
        public int Id { get; set; }

        public required string userName { get; set; }
    }
}
