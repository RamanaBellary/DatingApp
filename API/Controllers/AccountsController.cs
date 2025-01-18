using System.Security.Cryptography;
using System.Text;
using API.Controllers;
using API.Data;
using API.DTOs;
using API.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class AccountController(UserManager<AppUser> userManager, ITokenService tokenService, IMapper mapper): BaseApiController
{
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        if(await UserExists(registerDto.UserName))
        {
            return BadRequest("UserName already exist");
        }
        
        // using var hmac = new HMACSHA512();

        var user = mapper.Map<AppUser>(registerDto);

        user.UserName = registerDto.UserName.ToLower();
        // user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
        // user.PasswordSalt = hmac.Key;

        // dataContext.Users.Add(user);
        // await dataContext.SaveChangesAsync();

        var result = await userManager.CreateAsync(user, registerDto.Password);

        if(!result.Succeeded) return BadRequest(result.Errors);
        
        return new UserDto
        {
            username = user.UserName,
            token = await tokenService.CreateToken(user),
            KnownAs = user.KnownAs,
            Gender = user.Gender
        };
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user = await userManager.Users
        .Include(p=>p.Photos)        
        .FirstOrDefaultAsync(u=> u.NormalizedUserName != null && u.NormalizedUserName == loginDto.UserName.ToUpper());

        if(user is null || user.UserName == null) return Unauthorized("Invalid credentials");

        var result = await userManager.CheckPasswordAsync(user, loginDto.Password);

        if(!result) return Unauthorized();

        // using var hmac = new HMACSHA512(user.PasswordSalt);
        // var computeHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

        // if(computeHash.Length != user.PasswordHash.Length)
        // {
        //     return Unauthorized("Invalid credentials");
        // }

        // for(int i=0;i<computeHash.Length;i++)
        // {
        //     if(computeHash[i]!=user.PasswordHash[i]){
        //         return Unauthorized("Invalid credentials");
        //     }
        // }

        return new UserDto
        {
            username = user.UserName,
            KnownAs = user.KnownAs,
            token = await tokenService.CreateToken(user),
            photourl = user.Photos.FirstOrDefault(p=>p.IsMain)?.URL,
            Gender = user.Gender
        };
    }

    private async Task<bool> UserExists(string userName)
    {
        return await userManager.Users.AnyAsync(u => u.NormalizedUserName == userName.ToUpper());
    }
}