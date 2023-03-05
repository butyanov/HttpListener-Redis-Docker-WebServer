using WebServer.DTO;
using WebServer.models;

namespace WebServer.dto;

public class UserDto
{
    public Guid Id { get; set; }
    public string? Email { get; set; }
    
    public string? NickName { get; set; }

    public UserDto(User user)
    {
        Id = user.Id;
        Email = user.Email;
        NickName = user.NickName;
    }
    public UserDto(UpdateUserData user)
    {
        Id = user.Id;
        Email = user.Email;
        NickName = user.NickName;
    }
}