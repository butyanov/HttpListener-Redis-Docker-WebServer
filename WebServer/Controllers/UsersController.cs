using System.Net;
using Newtonsoft.Json;
using WebServer.Controllers.Interfaces;
using WebServer.DTO;
using WebServer.Http;
using WebServer.Http.Responses;
using WebServer.models;
using WebServer.Services;

namespace WebServer.Controllers;

internal class UsersController : ICrudController
{
    [RouteProps("users", "POST")]
    public async Task Save(HttpListenerContext context)
    {
        using var data = new StreamReader(context.Request.InputStream);
        try
        {
            var userModel = JsonConvert.DeserializeObject<User>(await data.ReadToEndAsync());
            if (userModel != null)
                await UsersService.ServiceInstance.Save(context, userModel);
            else
                await ApiResponse.SendFailure(context, new ErrorEntity(HttpStatusCode.BadGateway, "Something has gone wrong..."));
        }
        catch (Exception ex)
        {
            await ApiResponse.SendFailure(context, new ErrorEntity(HttpStatusCode.BadRequest, ex.Message));
        }
    }
    
    [RouteProps("users", "GET", RoutePropsAttribute.UserRole.Authorized)]
    public async Task Get(HttpListenerContext context)
    {
        try
        {
            var requestCookie = context.Request.Cookies["session-id"];
            var sessionDataString = await Redis.RedisStorage.RedisConnection.StringGetAsync(requestCookie!.Value);
            var sessionData = JsonConvert.DeserializeObject<IdentityDto>(sessionDataString!);
            if (sessionData != null)
                await UsersService.ServiceInstance.Get(context, sessionData.UserId);
            else
                await ApiResponse.SendFailure(context, new ErrorEntity(HttpStatusCode.BadGateway, "Something has gone wrong..."));
        }
        catch (Exception ex)
        {
            await ApiResponse.SendFailure(context, new ErrorEntity(HttpStatusCode.BadRequest, ex.Message));
        }
    }
    
    [RouteProps("users", "PATCH", RoutePropsAttribute.UserRole.Authorized)]
    public async Task Update(HttpListenerContext context)
    {
        using var data = new StreamReader(context.Request.InputStream);
        try
        {
            var userUpdateModel = JsonConvert.DeserializeObject<UpdateUserData>(await data.ReadToEndAsync());
            if (userUpdateModel != null)
            {
                await UsersService.ServiceInstance.Update(context, userUpdateModel);
            }
            else
                await ApiResponse.SendFailure(context, new ErrorEntity(HttpStatusCode.BadGateway, "Empty form was given."));
        }
        catch (Exception ex)
        {
            await ApiResponse.SendFailure(context, new ErrorEntity(HttpStatusCode.BadRequest, ex.Message));
        }
    }
    
    [RouteProps("users", "DELETE", RoutePropsAttribute.UserRole.Authorized)]
    public async Task Delete(HttpListenerContext context)
    {
        using var data = new StreamReader(context.Request.InputStream);
        try
        {
            var userModel = JsonConvert.DeserializeObject<User>(await data.ReadToEndAsync());
            if (userModel != null)
                await UsersService.ServiceInstance.Delete(context, userModel.Id);
            else
                await ApiResponse.SendFailure(context, new ErrorEntity(HttpStatusCode.BadGateway, "Something has gone wrong..."));
        }
        catch (Exception ex)
        {
            await ApiResponse.SendFailure(context, new ErrorEntity(HttpStatusCode.BadRequest, ex.Message));
        }
    }
}