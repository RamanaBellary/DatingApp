using System.Security.Cryptography;
using System.Text;
using API.Controllers;
using API.Data;
using API.DTOs;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class AccountController(DataContext dataContext, ITokenService tokenService): BaseApiController
{
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        if(await UserExists(registerDto.UserName))
        {
            return BadRequest("UserName already exist");
        }
        return Ok();
        // using var hmac = new HMACSHA512();
        // var user = new AppUser
        // {
        //     UserName = registerDto.UserName,
        //     PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
        //     PasswordSalt = hmac.Key
        // };

        // dataContext.Users.Add(user);
        // await dataContext.SaveChangesAsync();
        
        // return new UserDto
        // {
        //     username = user.UserName,
        //     Token = tokenService.CreateToken(user)
        // };
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user = await dataContext.Users.FirstOrDefaultAsync(u=>u.UserName.ToLower() == loginDto.UserName.ToLower());

        if(user is null)
        {
            return Unauthorized("Invalid credentials");
        }

        using var hmac = new HMACSHA512(user.PasswordSalt);
        var computeHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

        if(computeHash.Length != user.PasswordHash.Length)
        {
            return Unauthorized("Invalid credentials");
        }

        for(int i=0;i<computeHash.Length;i++)
        {
            if(computeHash[i]!=user.PasswordHash[i]){
                return Unauthorized("Invalid credentials");
            }
        }

        return new UserDto
        {
            username = user.UserName,
            Token = tokenService.CreateToken(user)
        };
    }

    private async Task<bool> UserExists(string userName)
    {
        return await dataContext.Users.AnyAsync(u => u.UserName.ToLower() == userName.ToLower());
    }
}