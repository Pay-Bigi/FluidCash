using FluidCash.Helpers.ServiceConfigs.Internal;
using FluidCash.IServiceRepo;
using FluidCash.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FluidCash.ServiceRepo;

public sealed class TokenService : ITokenService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly JwtSettings _jwtSettings;
    public TokenService(UserManager<AppUser> userManager,
        IOptions<JwtSettings> jwtSettings)
    {
        _userManager = userManager;
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
            Issuer = _jwtSettings.ValidIssuer,
            Audience = _jwtSettings.ValidAudience,
            Expires = DateTime.UtcNow.AddMinutes(Double.Parse(_jwtSettings.Expires)),
            SigningCredentials = credentials,
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
