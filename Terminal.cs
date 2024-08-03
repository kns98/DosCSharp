namespace SmallOS
{
    // Terminal Interaction
    public class Terminal
    {
        private readonly int taskId;
        private readonly Queue<string> inputQueue = new Queue<string>();
        private readonly Queue<string> outputQueue = new Queue<string>();
        private readonly Queue<string> errorQueue = new Queue<string>();

        public Terminal(int taskId)
        {
            this.taskId = taskId;
        }

        public void WriteOutput(string message)
        {
            outputQueue.Enqueue(message);
            Console.WriteLine($"[Task {taskId} Output] {message}");
        }

        public void WriteError(string message)
        {
            errorQueue.Enqueue(message);
            Console.Error.WriteLine($"[Task {taskId} Error] {message}");
        }

        public void WriteInput(string input)
        {
            inputQueue.Enqueue(input);
            Console.WriteLine($"[Task {taskId} Input] {input}");
        }

        public string ReadInput()
        {
            return inputQueue.Count > 0 ? inputQueue.Dequeue() : string.Empty;
        }

        public string ReadOutput()
        {
            return outputQueue.Count > 0 ? outputQueue.Dequeue() : string.Empty;
        }

        public string ReadError()
        {
            return errorQueue.Count > 0 ? errorQueue.Dequeue() : string.Empty;
        }
    }
}
