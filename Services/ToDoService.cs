using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using ToDoGrpc.Data;
using ToDoGrpc.Models;

namespace ToDoGrpc.Services
{
    public class ToDoService : ToDoIt.ToDoItBase
    {
        private readonly ILogger<ToDoService> _logger;
        private readonly AppDbContext _dbContext;
        private readonly List<ToDoItem> _toDoItems = new List<ToDoItem>();

        public ToDoService(ILogger<ToDoService> logger, AppDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }
        public override async Task<CreateToDoResponse> CreateToDo(CreateToDoRequest request, ServerCallContext context)
        {
            if (request == null || request.Title == string.Empty || request.Description == string.Empty)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Title and Description are required"));
            }
            var toDoItem = new ToDoItem
            {
                Title = request.Title,
                Description = request.Description
            };
            await _dbContext.AddAsync(toDoItem);
            await _dbContext.SaveChangesAsync();
            return await Task.FromResult(new CreateToDoResponse
            {
                Id = toDoItem.Id
            });
        }

        public override async Task<ReadToDoResponse> ReadToDo(ReadToDoRequest request, ServerCallContext context)
        {
            if (request.Id <= 0)
                throw new RpcException(new Status(StatusCode.InvalidArgument, "resouce index must be greater than 0"));

            var toDoItem = await _dbContext.ToDoItems.FirstOrDefaultAsync(t => t.Id == request.Id);

            if (toDoItem != null)
            {
                return await Task.FromResult(new ReadToDoResponse
                {
                    Id = toDoItem.Id,
                    Title = toDoItem.Title,
                    Description = toDoItem.Description,
                    ToDoStatus = toDoItem.ToDoStatus
                });
            }

            throw new RpcException(new Status(StatusCode.NotFound, $"No Task with id {request.Id}"));
        }
    
        public override async Task<GetAllResponse> ListToDo(GetAllRequest request, ServerCallContext context)
        {
            var toDoItems = await _dbContext.ToDoItems.ToListAsync();
            var response = new GetAllResponse();
            foreach (var toDoItem in toDoItems)
            {
                response.ToDo.Add(new ReadToDoResponse
                {
                    Id = toDoItem.Id,
                    Title = toDoItem.Title,
                    Description = toDoItem.Description,
                    ToDoStatus = toDoItem.ToDoStatus
                });
            }
            return await Task.FromResult(response);
        }
    
        public override async Task<UpdateToDoResponse> UpdateToDo(UpdateToDoRequest request, ServerCallContext context)
        {
            if (request.Id <= 0 || request.Title == string.Empty || request.Description == string.Empty)
                throw new RpcException(new Status(StatusCode.InvalidArgument, "resouce index must be greater than 0"));

            var toDoItem = await _dbContext.ToDoItems.FirstOrDefaultAsync(t => t.Id == request.Id);

            if (toDoItem != null)
            {
                toDoItem.Title = request.Title;
                toDoItem.Description = request.Description;
                toDoItem.ToDoStatus = request.ToDoStatus;
                await _dbContext.SaveChangesAsync();
                return await Task.FromResult(new UpdateToDoResponse
                {
                    Id = toDoItem.Id
                });
            }

            throw new RpcException(new Status(StatusCode.NotFound, $"No Task with id {request.Id}"));
        }
    
        public override async Task<DeleteToDoResponse> DeleteToDo(DeleteToDoRequest request, ServerCallContext context)
        {
            if (request.Id <= 0)
                throw new RpcException(new Status(StatusCode.InvalidArgument, "resouce index must be greater than 0"));

            var toDoItem = await _dbContext.ToDoItems.FirstOrDefaultAsync(t => t.Id == request.Id);

            if (toDoItem != null)
            {
                _dbContext.Remove(toDoItem);
                await _dbContext.SaveChangesAsync();
                return await Task.FromResult(new DeleteToDoResponse
                {
                    Id = toDoItem.Id
                });
            }

            throw new RpcException(new Status(StatusCode.NotFound, $"No Task with id {request.Id}"));
        }
    }
}