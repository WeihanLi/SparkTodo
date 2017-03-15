using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SparkTodo.API.Models
{
    public class JsonResponseModel<T>
    {
        public string Msg { get; set; }

        public T Data { get; set; }

        public JsonResponseStatus Status { get; set; }
    }

    public enum JsonResponseStatus
    {
        Success = 0,
        RequestError = 1,
        AuthFail = 2,
        ProcessFail = 3
    }
}
