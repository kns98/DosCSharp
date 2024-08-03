
// Device implementation handling block allocations and data storage
class Device : IDevice
{
    private readonly ILogger logger;
    private readonly IAllocationStrategy allocationStrategy;
    private readonly object lockObj = new object();
    private bool[] allocationTable;
    private byte[] storage;
    private string status;
    private readonly Guid deviceId;
    private readonly FileNode fileSystemRoot;

    public Device(string deviceName, int blockSize, DeviceType deviceType, int totalSize, IAllocationStrategy allocationStrategy, ILogger logger)
    {
        DeviceName = deviceName;
        BlockSize = blockSize;
        Type = deviceType;
        this.allocationStrategy = allocationStrategy;
        this.logger = logger;

        allocationTable = new bool[totalSize / blockSize];
        storage = new byte[totalSize];
        status = "Initialized";
        deviceId = Guid.NewGuid();
        fileSystemRoot = FileNodeFactory.CreateDirectory("/");
    }

    public string DeviceName { get; }
    public int BlockSize { get; }
    public DeviceType Type { get; }
    public Guid DeviceId => deviceId;
    public string Status { get => status; set => status = value; }
    public byte[] Storage => storage;
    public bool[] AllocationTable => allocationTable;
    public FileNode FileSystemRoot => fileSystemRoot;

    public int AllocateBlocks(int numberOfBlocks)
    {
        lock (lockObj)
        {
            try
            {
                int startIndex = allocationStrategy.Allocate(allocationTable, numberOfBlocks);
                if (startIndex >= 0)
                {
                    for (int i = startIndex; i < startIndex + numberOfBlocks; i++)
                    {
                        allocationTable[i] = true;
                    }
                    logger.LogEvent(LogLevel.Info, $"Allocated {numberOfBlocks} blocks starting at index {startIndex} in device {DeviceName}.");
                    return startIndex;
                }
                else
                {
                    logger.LogEvent(LogLevel.Warning, $"Failed to allocate {numberOfBlocks} blocks in device {DeviceName}.");
                    return -1;
                }
            }
            catch (Exception ex)
            {
                logger.LogEvent(LogLevel.Error, $"Error during block allocation: {ex.Message}");
                return -1;
            }
        }
    }

    public void DeallocateBlocks(int startIndex, int numberOfBlocks)
    {
        lock (lockObj)
        {
            try
            {
                for (int i = startIndex; i < startIndex + numberOfBlocks; i++)
                {
                    allocationTable[i] = false;
                }
                logger.LogEvent(LogLevel.Info, $"Deallocated {numberOfBlocks} blocks starting at index {startIndex} in device {DeviceName}.");
            }
            catch (Exception ex)
            {
                logger.LogEvent(LogLevel.Error, $"Error during block deallocation: {ex.Message}");
            }
        }
    }

    public void WriteData(int startIndex, byte[] data)
    {
        lock (lockObj)
        {
            try
            {
                if (startIndex >= 0 && startIndex + data.Length <= storage.Length)
                {
                    Array.Copy(data, 0, storage, startIndex, data.Length);
                    logger.LogEvent(LogLevel.Info, $"Written data starting at index {startIndex} in device {DeviceName}.");
                }
                else
                {
                    logger.LogEvent(LogLevel.Warning, $"Failed to write data: insufficient space or invalid index at {startIndex} in device {DeviceName}.");
                }
            }
            catch (Exception ex)
            {
                logger.LogEvent(LogLevel.Error, $"Error during data write: {ex.Message}");
            }
        }
    }

    public byte[] ReadData(int startIndex, int length)
    {
        lock (lockObj)
        {
            try
            {
                if (startIndex >= 0 && startIndex + length <= storage.Length)
                {
                    var data = new byte[length];
                    Array.Copy(storage, startIndex, data, 0, length);
                    logger.LogEvent(LogLevel.Info, $"Read data starting at index {startIndex} in device {DeviceName}.");
                    return data;
                }
                else
                {
                    logger.LogEvent(LogLevel.Warning, $"Failed to read data: requested range is out of bounds in device {DeviceName}.");
                    return Array.Empty<byte>();
                }
            }
            catch (Exception ex)
            {
                logger.LogEvent(LogLevel.Error, $"Error during data read: {ex.Message}");
                return Array.Empty<byte>();
            }
        }
    }
}
