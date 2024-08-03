using System.Collections.Concurrent;

namespace SmallOS
{
    // Task and Scheduler
    public class Task
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsRunning { get; set; }
        public int Priority { get; set; }
        public List<int> Dependencies { get; set; }
        public int MemoryUsage { get; set; }
        public Thread TaskThread { get; set; }
        public ManualResetEvent PauseEvent { get; set; }
        public CancellationTokenSource CancellationTokenSource { get; set; }
        public BlockingCollection<string> MessageQueue { get; set; } = new BlockingCollection<string>();
        public Terminal Terminal { get; set; }

        public Task(int id, string name, int priority)
        {
            Id = id;
            Name = name;
            Priority = priority;
            IsRunning = false;
            Terminal = new Terminal(id);
        }

        public void ProcessMessages()
        {
            foreach (var message in MessageQueue.GetConsumingEnumerable())
            {
                Terminal.WriteOutput($"Processing message: {message}");
            }
        }
    }
}
