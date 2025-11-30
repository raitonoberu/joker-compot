namespace UsersService.Jwt;

public interface IJwtTokenIssuer
{
    string Issue(Guid userId);
}