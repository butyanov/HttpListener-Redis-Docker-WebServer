using System.Text;
using Dapper;
using Npgsql;
using WebServer.dto;
using WebServer.DTO;
using WebServer.models;
using WebServer.Repositories.Interfaces;

namespace WebServer.Repositories;

public class UsersRepository : ICrudRepository<User>
{
    private readonly string _connectionString;
    private static readonly Lazy<UsersRepository> _repository = new(()=> new UsersRepository());
    public static UsersRepository RepositoryInstance => _repository.Value;
    public UsersRepository()
    {
        _connectionString = "Host=localhost;Username=postgres;Password=postgres;Database=sql";
    }

    public async Task<User?> Save(User model)
    {
        model.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
        var id = Guid.NewGuid();
        var queryExp = $@"INSERT INTO Users VALUES 
            (
             '{id}',
             '{model.Email}',
             '{model.NickName}',
             '{model.Password}')";
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var query = new NpgsqlCommand(queryExp, connection);
        var result = await query.ExecuteNonQueryAsync();
        await connection.CloseAsync();
        
        if (result <= 0) return null;
        model.Id = id;
        return model;
    }

    public async Task<User?> Get(Guid id)
    {
        var queryExp = $@"SELECT * FROM USERS WHERE id = '{id}'";
        await using var connection = new NpgsqlConnection(_connectionString);
        var user = await connection.QuerySingleOrDefaultAsync<User>(queryExp);
        return user;
    }

    public async Task<User?> Get(string email)
    {
        var queryExp = $@"SELECT * FROM USERS WHERE email = '{email}'";
        await using var connection = new NpgsqlConnection(_connectionString);
        var user = await connection.QuerySingleOrDefaultAsync<User>(queryExp);
        return user;
    }
    
    public async Task<UpdateUserData?> Update(UpdateUserData model)
    {
        var queryExp = GenerateUpdateQueryString(model);
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var query = new NpgsqlCommand(queryExp, connection);
        var result = await query.ExecuteNonQueryAsync();
        await connection.CloseAsync();
        return result <= 0 ? null : model;
    }

    public async Task<User?> Delete(Guid userId)
    {
        var user = await Get(userId);
        var queryExp = $@"DELETE FROM USERS WHERE id = '{userId.ToString()}'";
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync(queryExp);
        await connection.CloseAsync();
        return user;
    }

    private string GenerateUpdateQueryString(UpdateUserData model)
    {
        var queryExp = new StringBuilder("UPDATE Users SET").Append(' ');
        var properties = typeof(UpdateUserData).GetProperties();
        var notNullPropsNum = properties
            .Select(pi => pi.GetValue(model)!).Count(value => value != null) - 1;
        for (int i = 1; i < properties.Length; i++)
        {
            var propertyValue = properties[i].GetValue(model);
            if (propertyValue != null)
            {
                if (properties[i].Name.ToLower() == "password")
                    propertyValue = BCrypt.Net.BCrypt.HashPassword(propertyValue.ToString());
                queryExp.Append($@"{properties[i].Name} = '{propertyValue.ToString()}'");
                queryExp.Append(i < notNullPropsNum ? ", " : " ");
            }
        }
        queryExp.Append($@"WHERE Id = '{model.Id}'");

        return queryExp.ToString();
    }
}