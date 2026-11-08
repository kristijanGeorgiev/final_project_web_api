
using ClientTypeService.DTOs;
using ClientTypeService.Entities;
using ClientTypeService.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Storage;

namespace ClientTypeService.Services;

public class ClientTypeManagementService : IClientTypeService
{
    private readonly IClientTypeRepository _repository;
    private readonly ILogger<ClientTypeManagementService> _logger;

    public ClientTypeManagementService(
        IClientTypeRepository repository,
        ILogger<ClientTypeManagementService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    private static ClientTypeListDto ToListDto(ClientType entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Description = entity.Description
    };

    private static ClientTypeDetailDto ToDetailDto(ClientType entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Description = entity.Description
    };

    private async Task<T> ExecuteAsync<T>(
        string operation,
        Func<Task<T>> action)
    {
        try
        {
            _logger.LogInformation(
                "Starting ClientType operation: {Operation}",
                operation);

            var result = await action();

            _logger.LogInformation(
                "Completed ClientType operation: {Operation}",
                operation);

            return result;
        }
        catch (RetryLimitExceededException ex)
        {
            LogDatabaseException(operation, ex);
            throw;
        }
        catch (SqlException ex)
        {
            LogDatabaseException(operation, ex);
            throw;
        }
        catch (Exception ex) when (
            ex is not OperationCanceledException)
        {
            _logger.LogError(
                ex,
                "ClientType operation {Operation} failed. " +
                "Exception: {ExceptionType}. " +
                "Message: {Message}",
                operation,
                ex.GetType().FullName,
                ex.Message);

            throw;
        }
    }

    private void LogDatabaseException(
        string operation,
        Exception exception)
    {
        _logger.LogError(
            exception,
            "Database failure during {Operation}. " +
            "Exception: {ExceptionType}. " +
            "Message: {Message}",
            operation,
            exception.GetType().FullName,
            exception.Message);

        var current = exception;
        var depth = 0;

        while (current != null)
        {
            _logger.LogError(
                "Exception level {Depth}: {Type} - {Message}",
                depth,
                current.GetType().FullName,
                current.Message);

            if (current is SqlException sqlException)
            {
                _logger.LogError(
                    "SQL Server error. Number: {Number}, " +
                    "State: {State}, Class: {Class}, " +
                    "Server: {Server}, Procedure: {Procedure}, " +
                    "Line: {Line}",
                    sqlException.Number,
                    sqlException.State,
                    sqlException.Class,
                    sqlException.Server,
                    sqlException.Procedure,
                    sqlException.LineNumber);

                foreach (SqlError error in sqlException.Errors)
                {
                    _logger.LogError(
                        "SQL error {Number}: {Message}",
                        error.Number,
                        error.Message);
                }
            }

            current = current.InnerException;
            depth++;
        }
    }


    public async Task<List<ClientTypeListDto>> GetAllAsync(
        CancellationToken ct)
    {
        _logger.LogInformation(
            "Starting GetAll operation for ClientTypes.");

        try
        {
            var entities = await _repository.GetAllAsync(ct);

            var result = entities
                .Select(ToListDto)
                .ToList();

            _logger.LogInformation(
                "Successfully retrieved {Count} client types.",
                result.Count);

            return result;
        }
        catch (Microsoft.EntityFrameworkCore.Storage.RetryLimitExceededException ex)
        {
            _logger.LogError(
                ex,
                "SQL Server retry limit exceeded during GetAll.");

            var innerException = ex.InnerException;

            while (innerException != null)
            {
                _logger.LogError(
                    "Inner exception: {ExceptionType} - {Message}",
                    innerException.GetType().FullName,
                    innerException.Message);

                if (innerException is Microsoft.Data.SqlClient.SqlException sqlEx)
                {
                    _logger.LogError(
                        "SQL Error Number: {Number}, Server: {Server}, Message: {Message}",
                        sqlEx.Number,
                        sqlEx.Server,
                        sqlEx.Message);
                }

                innerException = innerException.InnerException;
            }

            throw;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(
                ex,
                "Failed to retrieve client types.");

            throw;
        }
    }

    public Task<ClientTypeDetailDto?> GetByIdAsync(
        int id,
        CancellationToken ct)
    {
        return ExecuteAsync($"GetById({id})", async () =>
        {
            var entity = await _repository.GetByIdAsync(id, ct);

            return entity is null ? null : ToDetailDto(entity);
        });
    }

    public Task<int> CountAsync(CancellationToken ct)
    {
        return ExecuteAsync("Count", () =>
            _repository.CountAsync(ct));
    }

    public Task<ClientTypeDetailDto> CreateAsync(
        ClientTypeCreateDto dto,
        CancellationToken ct)
    {
        return ExecuteAsync("Create", async () =>
        {
            var entity = new ClientType
            {
                Name = dto.Name,
                Description = dto.Description
            };

            await _repository.AddAsync(entity, ct);
            await _repository.SaveChangesAsync(ct);

            return ToDetailDto(entity);
        });
    }

    public Task<bool> UpdateAsync(
        int id,
        ClientTypeUpdateDto dto,
        CancellationToken ct)
    {
        return ExecuteAsync($"Update({id})", async () =>
        {
            var entity = await _repository.GetByIdAsync(id, ct);

            if (entity is null)
            {
                _logger.LogWarning(
                    "ClientType {Id} was not found.",
                    id);

                return false;
            }

            entity.Name = dto.Name;
            entity.Description = dto.Description;

            await _repository.SaveChangesAsync(ct);

            return true;
        });
    }

    public Task<bool> DeleteAsync(
        int id,
        CancellationToken ct)
    {
        return ExecuteAsync($"Delete({id})", async () =>
        {
            var entity = await _repository.GetByIdAsync(id, ct);

            if (entity is null)
            {
                _logger.LogWarning(
                    "ClientType {Id} was not found.",
                    id);

                return false;
            }

            _repository.Delete(entity);

            await _repository.SaveChangesAsync(ct);

            return true;
        });
    }
}