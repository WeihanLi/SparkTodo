using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SparkTodo.API.Extensions;
using SparkTodo.API.Models;
using SparkTodo.DataAccess;
using SparkTodo.Models;
using WeihanLi.EntityFramework;
using WeihanLi.Extensions;

namespace SparkTodo.API.Controllers
{
    [Authorize]
    [ApiVersion("2")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class SyncController : Controller
    {
        private readonly ISyncVersionRepository _versionRepository;
        private readonly ITodoItemRepository _todoItemRepository;

        public SyncController(ISyncVersionRepository versionRepository, ITodoItemRepository todoItemRepository)
        {
            _versionRepository = versionRepository;
            _todoItemRepository = todoItemRepository;
        }

        [HttpGet("version")]
        public async Task<IActionResult> GetVersion()
        {
            var userId = User.GetUserId();
            var version = (await _versionRepository.SelectAsync(1, _ => _.UserId == userId, _ => _.VersionId)).FirstOrDefault();
            return Ok(new
            {
                VersionId = version?.VersionId ?? -1
            });
        }

        //pull
        [HttpGet]
        public async Task<IActionResult> Get(long version)
        {
            var userId = User.GetUserId();
            var latestVersionId = (await _versionRepository.FetchAsync(_ => _.UserId == userId, _ => _.VersionId))?.VersionId ?? -1;
            if (latestVersionId <= 0)
            {
                return Ok(new SyncTodoModel()
                {
                    SyncTodoItems = Array.Empty<SyncTodoItemModel>(),
                    Version = latestVersionId
                });
            }
            if (version > 0 && latestVersionId == version)
            {
                return Ok(new SyncTodoModel()
                {
                    SyncTodoItems = Array.Empty<SyncTodoItemModel>(),
                    Version = version
                });
            }
            //
            if (version <= 0)
            {
                var todoItems = await _todoItemRepository.SelectAsync(_ => _.UserId == userId);
                return Ok(new SyncTodoModel()
                {
                    SyncTodoItems = todoItems.Select(_ => new SyncTodoItemModel()
                    {
                        TodoItem = _,
                        Type = OperationType.Add,
                    }).ToArray(),
                    Version = latestVersionId
                });
            }
            else
            {
                var versionInfo = await _versionRepository.FetchAsync(_ => _.VersionId == latestVersionId && _.UserId == userId);
                if (null == versionInfo)
                {
                    return Ok(new SyncTodoModel()
                    {
                        SyncTodoItems = Array.Empty<SyncTodoItemModel>(),
                        Version = -1
                    });
                }

                var items = await _todoItemRepository
                        .SelectAsync(_ => _.UpdatedTime > versionInfo.SyncTime && _.UserId == userId)
                    ;
                items.RemoveAll(_ => _.IsDeleted && _.CreatedTime > versionInfo.SyncTime); // remove add then delete items

                return Ok(new SyncTodoModel()
                {
                    SyncTodoItems = items.Select(_ => new SyncTodoItemModel()
                    {
                        TodoItem = _,
                        Type = _.CreatedTime > versionInfo.SyncTime ?
                            OperationType.Add :
                            (_.IsDeleted ? OperationType.Delete : OperationType.Update),
                    }).ToArray(),
                    Version = latestVersionId
                });
            }
        }

        // push
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]SyncTodoModel syncData)
        {
            if (syncData?.SyncTodoItems == null || syncData.SyncTodoItems.Length == 0)
            {
                return BadRequest();
            }

            var userId = User.GetUserId();
            var latestVersionId = (await _versionRepository.SelectAsync(1, _ => _.UserId == userId, _ => _.VersionId)).FirstOrDefault()?.VersionId ?? -1;
            if (latestVersionId > 0 && latestVersionId > syncData.Version)
            {
                return BadRequest(new { Error = "请先同步数据" });
            }

            // TODO: 使用 redis 分布式锁，锁当前用户的操作，顺序同步

            // 处理 todoItem
            var toAddTodoItems = syncData.SyncTodoItems.Where(_ => _.Type == OperationType.Add).Select(_ => _.TodoItem).ToArray();
            var todoRepository = HttpContext.RequestServices.GetRequiredService<ITodoItemRepository>();
            await todoRepository.InsertAsync(toAddTodoItems);
            var toDeleteTodoItemIds = syncData.SyncTodoItems.Where(_ => _.Type == OperationType.Delete).Select(_ => _.TodoItem.TodoId).Distinct().ToList();
            await todoRepository.UpdateAsync(todo => toDeleteTodoItemIds.Contains(todo.TodoId), t => t.IsDeleted, true);
            var toUpdateItems = syncData.SyncTodoItems.Where(_ => _.Type == OperationType.Update).Select(_ => _.TodoItem).ToArray();
            foreach (var item in toUpdateItems)
            {
                await todoRepository.UpdateAsync(item);
            }

            var version = new SyncVersion()
            {
                SyncData = syncData.ToJson(),
                UserId = userId,
                SyncTime = DateTime.UtcNow
            };
            await _versionRepository.InsertAsync(version);

            return Ok(new
            {
                VersionId = version.VersionId
            });
        }
    }
}
