using Microsoft.EntityFrameworkCore;

namespace API.Entities
public class AppUser{
    [Key]
    public int id{get;set;}

    public required string userName{get;set;}
}