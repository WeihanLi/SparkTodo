// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using WebExtensions.Net.Notifications;
using WeihanLi.Common.Data;

namespace SparkTodo.WebExtension.Services;

public class TodoScheduler
{
    private static readonly int[] TimeToReminder = { 15, 10, 5 };

    private readonly IWebExtensionsApi _webExtensionApi;
    private readonly ITodoItemRepository _repository;
    private readonly ILogger<TodoScheduler> _logger;

    public TodoScheduler(
        IWebExtensionsApi webExtensionApi,
        ITodoItemRepository repository,
        ILogger<TodoScheduler> logger)
    {
        _webExtensionApi = webExtensionApi;
        _repository = repository;
        _logger = logger;
    }

    public async Task<TodoItem> CreateTask(TodoItem todoItem)
    {
        todoItem.IsDeleted = false;
        todoItem.CreatedTime = DateTime.UtcNow;
        todoItem.UpdatedTime = DateTime.UtcNow;
        await _repository.InsertAsync(todoItem);
        return todoItem;
    }

    public async Task<int> DeleteTask(int id)
    {
        var todoItem = await _repository.DbContext.TodoItems.FindAsync(id);
        if (todoItem is null)
        {
            return 0;
        }
        return await _repository.DeleteAsync(todoItem);
    }

    public async Task<int> UpdateTask(TodoItem todoItem)
    {
        todoItem.UpdatedTime = DateTime.UtcNow;
        return await _repository.UpdateWithoutAsync(todoItem, x => x.CreatedTime, x => x.IsDeleted, x => x.IsCompleted,
            x => x.CompletedTime);
    }

    public async Task<int> CompleteTask(int id)
    {
        var todoItem = new TodoItem() { TodoId = id, IsCompleted = true, CompletedTime = DateTime.UtcNow, };
        return await _repository.UpdateAsync(todoItem, x => x.IsCompleted,
            x => x.CompletedTime);
    }

    public async Task<int> UnCompleteTask(int id)
    {
        var todoItem = new TodoItem() { TodoId = id, IsCompleted = false, CompletedTime = null, };
        return await _repository.UpdateAsync(todoItem, x => x.IsCompleted,
            x => x.CompletedTime);
    }

    public Task<List<TodoItem>> GetAllTasks()
    {
        return _repository.GetAllAsync();
    }

    public Task Start()
    {
        var task = Execute();
        if (task.IsCompleted)
        {
            return task;
        }
        return Task.CompletedTask;
    }

    private async Task Execute()
    {
        while (true)
        {
            var result = await _repository.SelectAsync(x => x.ScheduledTime.HasValue
                                                            && x.ScheduledTime.Value > DateTime.UtcNow
                                                            && x.ScheduledTime.Value < DateTime.UtcNow.AddMinutes(120)
                                                            && !(x.IsDeleted || x.IsCompleted));
            if (result.Count > 0)
            {
                _logger.LogInformation($"{result.Count} todo items may need to be reminded");
                await Task.WhenAll(result.Select(async item =>
                {
                    var scheduledTime = item.ScheduledTime!.Value;
                    _logger.LogInformation($"{item.TodoTitle} scheduled at {scheduledTime:yyyy/MM/dd HH:mm:ss}");
                    foreach (var minutes in TimeToReminder)
                    {
                        var minutesDiff = (scheduledTime - DateTime.UtcNow.AddMinutes(minutes)).TotalMinutes;
                        if (minutesDiff is > 0 and < 1)
                        {
                            await SendNotification(item, (int)(scheduledTime - DateTime.UtcNow).TotalMinutes);
                            _logger.LogInformation($"Notification sent for {item.TodoTitle}({item.TodoId})");
                            break;
                        }
                    }
                }));
                await Task.Delay(TimeSpan.FromSeconds(20));
            }
            else
            {
                _logger.LogInformation("No todo items need to be reminded");
                await Task.Delay(TimeSpan.FromSeconds(10));
            }
        }
    }

    private async ValueTask<string> SendNotification(TodoItem todoItem, int timeToStart)
    {
        try
        {
            var result = await _webExtensionApi.Notifications.Create(todoItem.TodoId.ToString(),
                new CreateNotificationOptions()
                {
                    Title = $"{timeToStart}min to {todoItem.TodoTitle}",
                    Message = todoItem.TodoContent,
                    Type = TemplateType.Basic,
                    IconUrl = "https://www.nuget.org/profiles/weihanli/avatar",
                });
            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "SendNotification Error");
        }
        return string.Empty;
    }
}
