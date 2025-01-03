namespace API.DTOs;

public class UserDto
{
    public required string username { get; set; }
    public required string KnownAs {get;set;}
    public required string token { get; set; }
    public string? photourl { get; set; }
}