using WebServer.models;

namespace WebServer.Repositories.Interfaces;

public interface ICrudRepository<T> : IRepository
{
    Task<T?> Save(T model);
    Task<T?> Get(Guid id);
    Task<T?> Get(string email);
    Task<UpdateUserData?> Update(UpdateUserData model);
    Task<T?> Delete(Guid id);
}