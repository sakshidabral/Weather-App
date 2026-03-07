using System;

public interface IAuthService
{
    string Signup(User user);
    string Login(User login);
}
