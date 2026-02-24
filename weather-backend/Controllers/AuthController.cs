using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly string _filePath = "users.json"; // store users in project root

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // ------------------ SIGNUP ------------------
    [HttpPost("signup")]
    public IActionResult Signup([FromBody] User user)
    {
        // Read existing users from JSON
        var users = System.IO.File.Exists(_filePath)
            ? JsonSerializer.Deserialize<List<User>>(System.IO.File.ReadAllText(_filePath))
            : new List<User>();

        if (users == null) users = new List<User>();

        // Check if username already exists
        if (users.Any(u => u.Username == user.Username))
            return BadRequest("Username already exists");

        // Add new user
        users.Add(user);
        System.IO.File.WriteAllText(_filePath, JsonSerializer.Serialize(users));

        return Ok("User created successfully");
    }

    // ------------------ LOGIN ------------------
    [HttpPost("login")]
    public IActionResult Login([FromBody] User login)
    {
        // Read users from JSON
        var users = System.IO.File.Exists(_filePath)
            ? JsonSerializer.Deserialize<List<User>>(System.IO.File.ReadAllText(_filePath))
            : new List<User>();

        if (users == null) users = new List<User>();

        // Check username/password
        var user = users.FirstOrDefault(u => u.Username == login.Username && u.Password == login.Password);
        if (user == null)
            return Unauthorized("Invalid username or password");

        // Generate JWT token
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
        var creds = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: new[] { new Claim(ClaimTypes.Name, user.Username) },
            expires: DateTime.Now.AddHours(1),
            signingCredentials: creds
        );

        return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
    }
}

// ------------------ USER MODEL ------------------
public class User
{
    public string Username { get; set; }
    public string Password { get; set; }
}