using System.Security.Cryptography;
using System.Text;
using API.Controllers;
using API.Data;
using API.DTOs;
using API.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class AccountController(DataContext dataContext, ITokenService tokenService, IMapper mapper): BaseApiController
{
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        if(await UserExists(registerDto.UserName))
        {
            return BadRequest("UserName already exist");
        }
        
        using var hmac = new HMACSHA512();

        var user = mapper.Map<AppUser>(registerDto);

        user.UserName = registerDto.UserName.ToLower();
        user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
        user.PasswordSalt = hmac.Key;

        dataContext.Users.Add(user);
        await dataContext.SaveChangesAsync();
        
        return new UserDto
        {
            username = user.UserName,
            token = tokenService.CreateToken(user),
            KnownAs = user.KnownAs
        };
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user = await dataContext.Users
        .Include(p=>p.Photos)        
        .FirstOrDefaultAsync(u=>u.UserName.ToLower() == loginDto.UserName.ToLower());

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
            KnownAs = user.KnownAs,
            token = tokenService.CreateToken(user),
            photourl = user.Photos.FirstOrDefault(p=>p.IsMain)?.URL
        };
    }

    private async Task<bool> UserExists(string userName)
    {
        return await dataContext.Users.AnyAsync(u => u.UserName.ToLower() == userName.ToLower());
    }
}