using System.Linq;
using TraceWebApi.EntityFrameworkCore;
using TraceWebApi.Interfaces;
using TraceWebApi.Models.Users;

namespace TraceWebApi.Repositories;

public class UserRepository : Repository<User>, IUserRepository 

{
    private readonly TraceAppDbContext _context;

    public UserRepository(TraceAppDbContext context) : base(context)
    {
        _context = context;
    }

    public User? FindUserByEmail(string email)
    {
        return _context.Users.FirstOrDefault(u => u.Email == email);
    }
}
