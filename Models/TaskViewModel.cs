using System.Diagnostics.CodeAnalysis;

namespace DemoWeb.Models
{
    public class TaskViewModel
    {
        [AllowNull]
        public int finishTaskCount { get; set; }

        [AllowNull]
        public int pendingTaskCount { get; set; }

        [AllowNull]
        public int incompleteTaskCount { get; set; }
    }
}
