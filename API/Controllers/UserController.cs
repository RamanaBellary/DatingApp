using System.Security.Claims;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class UsersController(IUnitOfWork unitOfWork, IMapper mapper, IPhotoService photoService) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery]UserParams userParams)
    {
        userParams.CurrentUsername = User.GetUserName();
        var users = await unitOfWork.UserRepository.GetMembersAsync(userParams);
        Response.AddPaginationHeader(users);
        return Ok(users);
    }

   [HttpGet("{username}")]
    public async Task<ActionResult<MemberDto>> GetUser(string username)
    {
        var user = await unitOfWork.UserRepository.GetMemberAsync(username);

        if(user is null)
            return NotFound();

        return user;
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
    {
        var user = await unitOfWork.UserRepository.GetUserByUserNameAsync(User.GetUserName());

        if(user == null) return BadRequest("Could not find user");

        mapper.Map(memberUpdateDto, user);

        if(await unitOfWork.Complete()) return NoContent();

        return BadRequest("Failed to update the user");
    }

    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
    {
        var user = await unitOfWork.UserRepository.GetUserByUserNameAsync(User.GetUserName());

        if(user == null) return BadRequest("Could not find user");

        var result = await photoService.AddPhotoAsync(file);

        if(result.Error != null) return BadRequest(result.Error.Message);

        var photo = new Photo
        {
            URL = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId
        };

        if(user.Photos.Count == 0)
        {
            photo.IsMain = true;
        }

        user.Photos.Add(photo);

        if(await unitOfWork.Complete()) 
            return CreatedAtAction(nameof(GetUser), new {username = user.UserName}, mapper.Map<PhotoDto>(photo));

        return BadRequest("Problem adding Photo");
    }

    [HttpPut("set-main-photo/{photoId:int}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        var user = await unitOfWork.UserRepository.GetUserByUserNameAsync(User.GetUserName());

        if(user == null) return BadRequest("Could not find user");

        var photo = user.Photos.FirstOrDefault(p=> p.Id == photoId);

        if(photo == null || photo.IsMain)
        {
            return BadRequest("Cannot use this as main photo");
        }

        var currentPhoto = user.Photos.FirstOrDefault(p=>p.IsMain);

        if(currentPhoto != null) currentPhoto.IsMain = false;

        photo.IsMain = true;

        if(await unitOfWork.Complete()) return NoContent();

        return BadRequest("Problem setting main photo");
    }

    [HttpDelete("delete-photo/{photoId:int}")]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        var user = await unitOfWork.UserRepository.GetUserByUserNameAsync(User.GetUserName());

        if(user == null) return BadRequest("Could not find user");

        var photo = user.Photos.FirstOrDefault(p=> p.Id == photoId);

        if(photo == null || photo.IsMain)
        {
            return BadRequest("This photo cannot be deleted");
        }

        if(photo.PublicId != null)
        {
            var result = await photoService.DeletePhotoAsync(photo.PublicId);
            if(result.Error != null) return BadRequest(result.Error.Message);
        }

        user.Photos.Remove(photo);

        if(await unitOfWork.Complete()) return Ok();

        return BadRequest("Problem deleting photo");
    }
}