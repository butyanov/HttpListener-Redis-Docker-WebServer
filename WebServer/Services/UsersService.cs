using System.Net;
using System.Text;
using WebServer.dto;
using WebServer.Http.Responses;
using WebServer.models;
using WebServer.Repositories;

namespace WebServer.Services;

public class UsersService
{
    private static readonly Lazy<UsersService> _service = new(() => new UsersService());

    private UsersService()
    {
    }

    public static UsersService ServiceInstance => _service.Value;

    public async Task Save(HttpListenerContext context, User model)
    {
        var validationResult = ValidationService.ValidateModel(model);
        if (validationResult != null)
        {
            await ApiResponse.SendFailure(context,
                new ErrorEntity(HttpStatusCode.BadRequest, $"{string.Join(" ", validationResult)}"));
        }
        else
        {
            var repository = UsersRepository.RepositoryInstance;
            if (await repository.Get(model.Email) != null)
            {
                await ApiResponse.SendFailure(context,
                    new ErrorEntity(HttpStatusCode.BadRequest, "User was already registered!"));
            }
            else
            {
                var createdUser = await repository.Save(model);
                if (createdUser == null)
                    await ApiResponse.SendFailure(context,
                        new ErrorEntity(HttpStatusCode.BadGateway, "Something has gone wrong..."));
                else
                    await ApiResponse.SendSuccess(context,
                        new SuccessEntity(new UserDto(createdUser), "User was successfully saved!"));
            }
        }
    }

    public async Task Get(HttpListenerContext context, Guid id)
    {
        var receivedUser = await UsersRepository.RepositoryInstance.Get(id);
        if (receivedUser == null)
            await ApiResponse.SendFailure(context, new ErrorEntity(HttpStatusCode.BadRequest, "User was not found!"));
        else
            await ApiResponse.SendSuccess(context,
                new SuccessEntity(new UserDto(receivedUser), "User was successfully received!"));
    }

    public async Task Get(HttpListenerContext context, string email)
    {
        var receivedUser = await UsersRepository.RepositoryInstance.Get(email);
        if (receivedUser == null)
            await ApiResponse.SendFailure(context, new ErrorEntity(HttpStatusCode.BadRequest, "User was not found!"));
        else
            await ApiResponse.SendSuccess(context,
                new SuccessEntity(new UserDto(receivedUser), "User was successfully received!"));
    }

    public async Task Update(HttpListenerContext context, UpdateUserData model)
    {
        var validationResult = ValidationService.ValidateModel(model);
        if (validationResult != null)
        {
            await ApiResponse.SendFailure(context,
                new ErrorEntity(HttpStatusCode.BadRequest, $"{string.Join(" ", validationResult)}"));
        }
        else
        {
            var currentUser = await UsersRepository.RepositoryInstance.Get(model.Id);
            if (currentUser != null)
            {
                if (IsUpdateDataEqual(currentUser, model))
                {
                    await ApiResponse.SendFailure(context,
                        new ErrorEntity(HttpStatusCode.BadRequest, "Nothing to update."));
                }
                else
                {
                    var updatedUser = await UsersRepository.RepositoryInstance.Update(model);
                    if (updatedUser == null)
                        await ApiResponse.SendFailure(context,
                            new ErrorEntity(HttpStatusCode.BadGateway, "Something has gone wrong."));
                    else
                        await ApiResponse.SendSuccess(context,
                            new SuccessEntity(new UserDto(updatedUser), "User was successfully updated!"));
                }
            }
            else
                await ApiResponse.SendFailure(context,
                    new ErrorEntity(HttpStatusCode.BadGateway, "Something has gone wrong."));
        }
    }

    public async Task Delete(HttpListenerContext context, Guid id)
    {
        var repository = UsersRepository.RepositoryInstance;
        if (await repository.Get(id) == null)
        {
            await ApiResponse.SendFailure(context, new ErrorEntity(HttpStatusCode.BadRequest, "User was not found!"));
        }
        else
        {
            var deletedUser = await repository.Delete(id);
            if (deletedUser == null)
            {
                await ApiResponse.SendFailure(context,
                    new ErrorEntity(HttpStatusCode.BadGateway, "Something has gone wrong..."));
            }
            else
            {
                await AuthService.ServiceInstance.Logout(context);
                await ApiResponse.SendSuccess(context,
                    new SuccessEntity(new UserDto(deletedUser), "User was successfully deleted!"));
            }
        }
    }

    private bool IsUpdateDataEqual(User currentUser, UpdateUserData updatedUser)
    {
        var currentUserProps = typeof(User).GetProperties();
        var notNullUpdateProps = typeof(UpdateUserData).GetProperties()
                .Where(prop => prop.GetValue(updatedUser) != null).ToArray();
        var unequalPropsNum = 0;
        foreach (var updateProp in notNullUpdateProps)
        {
            var currentProp = currentUserProps.Single(prop => prop.Name == updateProp.Name);
            if (currentProp.GetValue(currentUser)!.ToString() != updateProp.GetValue(updatedUser)!.ToString())
                unequalPropsNum++;
        }
        return unequalPropsNum == 0;
    }
}