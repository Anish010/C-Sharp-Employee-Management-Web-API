using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EMS.API.Helpers;
using EMS.BAL;
using EMS.BAL.Interfaces;
using EMS.BAL.Models;
using EMS.DAL;
using EMS.DAL.DTO;
using EMS.DAL.Interfaces;
using EMS.DAL.Models;
using EMS.DB.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;
using Serilog;

namespace EMS.API.Controllers;

[Route("/Auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthBAL _authBal;
    private readonly Serilog.ILogger _logger;
    private IConfiguration _configuration;

    public AuthController( Serilog.ILogger logger, IConfiguration configuration, IAuthBAL authBal)
    {
        _configuration = configuration;
        _logger = logger;
        _authBal = authBal;
    }

    [AllowAnonymous]
    [HttpPost("Login")]
    public async Task<IActionResult> LoginAsync(AuthenticateRequest model)
    {
        try
        {
            AuthenticateResponse? authResponse = await _authBal.AuthenticateAsync(model.Email);
            if (authResponse == null || !BCrypt.Net.BCrypt.Verify(model.Password, authResponse.Password))
            {
                return ResponseHelper.WrapResponse(401, "error", "Invalid email or password");
            }

            authResponse.Password = null;

            var token = GenerateJwtToken(authResponse);

            var responseData = new
            {
                AuthResponse = authResponse,
                Token = token
            };

            return ResponseHelper.WrapResponse(200, StatusMessage.SUCCESS.ToString(), responseData);
        }
        catch (Exception)
        {
            return ResponseHelper.WrapResponse(500, "error", null, ErrorCodes.FAILED_TO_AUTHENTICATE.ToString());
        }
    }

    [AllowAnonymous]
    [HttpPost("Register")]
    public async Task<IActionResult> RegisterAsync([FromBody] EmployeeDto employee)
    {
        try
        {
            employee.Password = BCrypt.Net.BCrypt.HashPassword(employee.Password, 12);
            var result = await _authBal.SignUpAsync(employee);
            if (result == null || result.IsAuthenticated == false)
            {
                throw new InvalidOperationException("Failed to register user");
            }

            var token = GenerateJwtToken(result);
            result.Password = null;
            var responseData = new
            {
                Response = result,
                Token = token
            };

            return ResponseHelper.WrapResponse(200, StatusMessage.SUCCESS.ToString(), responseData);
        }
        catch (Exception)
        {
            return ResponseHelper.WrapResponse(500, StatusMessage.FAILURE.ToString(), null, ErrorCodes.FAILED_TO_REGISTER.ToString());
        }
    }

    [Authorize]
    [HttpGet("Logout")]
    public async Task<IActionResult> LogoutAsync([FromQuery] int EmployeeId)
    {
        try
        {
            await _authBal.LogoutAsync(EmployeeId);
            return ResponseHelper.WrapResponse(200,StatusMessage.SUCCESS.ToString(), null);
        }
        catch(Exception)
        {
            return ResponseHelper.WrapResponse(500, StatusMessage.FAILURE.ToString(), null, ErrorCodes.FAILED_TO_LOGOUT.ToString());
        }
    }

    private string GenerateJwtToken(AuthenticateResponse authResponse)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, authResponse!.Id.ToString()!),
            new Claim(ClaimTypes.Email, authResponse.Email!),
            new Claim(ClaimTypes.GivenName, authResponse.FirstName!),
            new Claim(ClaimTypes.Surname, authResponse.LastName!),
            new Claim(ClaimTypes.Name, authResponse.UID!),
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}