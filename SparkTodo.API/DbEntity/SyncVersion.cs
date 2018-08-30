using System;

namespace SparkTodo.Models
{
    public class SyncVersion
    {
        public long VersionId { get; set; }

        public DateTime SyncTime { get; set; }

        public string SyncData { get; set; }

        public int UserId { get; set; }
    }
}
