using WebServer.dto;

namespace WebServer.Http.Responses;

public class SuccessEntity
{
    public UserDto? Model { get; }
    public string Message { get; }

    public SuccessEntity(UserDto? model, string message)
    {
        Model = model;
        Message = message;
    }
}