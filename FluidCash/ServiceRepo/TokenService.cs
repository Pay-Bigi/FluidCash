using FluidCash.Helpers.ServiceConfigs.Internal;
using FluidCash.IServiceRepo;
using FluidCash.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;

namespace FluidCash.ServiceRepo;

public sealed class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<AppUser> _userManager;
    private readonly JwtSettings _jwtSettings;
    public TokenService(IConfiguration configuration,
        UserManager<AppUser> userManager,
        IOptions<JwtSettings> jwtSettings)
    {
        _userManager = userManager;
        _configuration = configuration;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<string> CreateTokenAsync(AppUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.TokenKey));
        var roles = await _userManager.GetRolesAsync(user);
        var claim = new List<Claim>
        {
            new Claim(ClaimTypes.Email, user.Email)
        };

        foreach (var role in roles)
        {
            claim.Add(new Claim(ClaimTypes.Role, role));
        }
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claim),
            IssuedAt = DateTime.UtcNow,//Had to remove .AddHour(1) because it automatically add one hour for some reason
            Issuer = _jwtSettings.validIssuer,
            Audience = _jwtSettings.validAudience,
            Expires = DateTime.UtcNow.AddMinutes(Double.Parse(_jwtSettings.Expires)),
            SigningCredentials = credentials,
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
