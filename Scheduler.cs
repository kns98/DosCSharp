namespace SmallOS
{
    public class Scheduler
    {
        private List<Task> taskList;
        private Logger logger;
        private int taskCounter = 1;

        public Scheduler(List<Task> taskList, Logger logger)
        {
            this.taskList = taskList;
            this.logger = logger;
            var schedulerThread = new Thread(RunScheduler)
            {
                IsBackground = true
            };
            schedulerThread.Start();
        }

        public Scheduler()
        {
        }

        private void RunScheduler()
        {
            while (true)
            {
                foreach (var task in taskList)
                {
                    if (task.IsRunning && task.TaskThread != null && task.TaskThread.ThreadState == ThreadState.WaitSleepJoin)
                    {
                        task.PauseEvent.Set();
                    }
                }

                Thread.Sleep(1000);
            }
        }

        public void CreateTask(string name, int priority)
        {
            int id = taskCounter++;
            var newTask = new Task(id, name, priority);
            taskList.Add(newTask);
            logger.LogEvent( LogLevel.Info,$"Created task {id}: {name} with priority {priority}");
        }

        public void DeleteTask(int id)
        {
            var task = taskList.Find(t => t.Id == id);
            if (task != null)
            {
                if (task.IsRunning)
                {
                    StopTask(id);
                }
                taskList.Remove(task);
                logger.LogEvent(LogLevel.Info, $"Deleted task {id}");
            }
            else
            {
                Console.WriteLine("Task not found.");
            }
        }

        public void StartTask(int id)
        {
            var task = taskList.Find(t => t.Id == id);
            if (task != null && !task.IsRunning)
            {
                task.PauseEvent = new ManualResetEvent(true);
                task.CancellationTokenSource = new CancellationTokenSource();

                task.TaskThread = new Thread(() =>
                {
                    Console.WriteLine($"Task {task.Id} started.");
                    while (!task.CancellationTokenSource.Token.IsCancellationRequested)
                    {
                        task.PauseEvent.WaitOne();
                        Thread.Sleep(500);
                        task.ProcessMessages();
                    }
                    Console.WriteLine($"Task {task.Id} stopped.");
                })
                {
                    IsBackground = true
                };

                task.IsRunning = true;
                task.TaskThread.Start();
                logger.LogEvent(LogLevel.Info, $"Started task {id}");
            }
        }

        public void StopTask(int id)
        {
            var task = taskList.Find(t => t.Id == id);
            if (task != null && task.IsRunning)
            {
                task.CancellationTokenSource.Cancel();
                task.TaskThread.Join();
                task.IsRunning = false;
                logger.LogEvent(LogLevel.Info, $"Stopped task {id}");
            }
        }

        public void PauseTask(int id)
        {
            var task = taskList.Find(t => t.Id == id);
            if (task != null && task.IsRunning)
            {
                task.PauseEvent.Reset();
                logger.LogEvent(LogLevel.Info, $"Paused task {id}");
            }
        }

        public void ResumeTask(int id)
        {
            var task = taskList.Find(t => t.Id == id);
            if (task != null && task.IsRunning)
            {
                task.PauseEvent.Set();
                logger.LogEvent(LogLevel.Info, $"Resumed task {id}");
            }
        }
    }
}
