using System.Net;

namespace WebServer.Controllers.Interfaces;

public interface ICrudController : IController
{ 
    Task Save(HttpListenerContext context);
    Task Get(HttpListenerContext context);
    Task Update(HttpListenerContext context);
    Task Delete(HttpListenerContext context);
}