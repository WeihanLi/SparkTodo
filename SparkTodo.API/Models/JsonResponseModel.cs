using SparkTodo.API.JWT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SparkTodo.API.Models
{
    public class JsonResponseModel
    {
        /// <summary>
        /// 消息
        /// </summary>
        public string Msg { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public JsonResponseStatus Status { get; set; }
    }

    public class JsonResponseModel<T>:JsonResponseModel
    {
        public T Data { get; set; }
    }

    public enum JsonResponseStatus
    {
        Success = 0,
        RequestError = 1,
        AuthFail = 2,
        ProcessFail = 3
    }
}
