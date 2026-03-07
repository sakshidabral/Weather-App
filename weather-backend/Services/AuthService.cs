using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly string _filePath = "users.json";

    public AuthService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string Signup(User user)
    {
        var users = System.IO.File.Exists(_filePath)
            ? JsonSerializer.Deserialize<List<User>>(System.IO.File.ReadAllText(_filePath))
            : new List<User>();

        if (users == null) users = new List<User>();

        if (users.Any(u => u.Username == user.Username))
            throw new Exception("Username already exists");

        users.Add(user);

        System.IO.File.WriteAllText(_filePath, JsonSerializer.Serialize(users));

        return "User created successfully";
    }

    public string Login(User login)
    {
        var users = System.IO.File.Exists(_filePath)
            ? JsonSerializer.Deserialize<List<User>>(System.IO.File.ReadAllText(_filePath))
            : new List<User>();

        if (users == null) users = new List<User>();

        var user = users.FirstOrDefault(u =>
            u.Username == login.Username &&
            u.Password == login.Password);

        if (user == null)
            throw new Exception("Invalid username or password");

        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

        var creds = new SigningCredentials(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: new[] { new Claim(ClaimTypes.Name, user.Username) },
            expires: DateTime.Now.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}