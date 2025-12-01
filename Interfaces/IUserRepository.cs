using TraceWebApi.Models.Users;

namespace TraceWebApi.Interfaces;

public interface IUserRepository : IRepository<User>
{
    User? FindUserByEmail(string email);
}
