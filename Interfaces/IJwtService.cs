using TraceWebApi.Models.Users;

namespace TraceWebApi.Interfaces;

public interface IJwtService
{
    (string Token, DateTime Exp) GenerateToken(User user);
}
