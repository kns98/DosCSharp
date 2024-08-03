using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;

// Define log levels for better logging management
enum LogLevel
{
    Info,
    Warning,
    Error
}

// Define a logger interface
interface ILogger
{
    void LogEvent(LogLevel level, string message);
    void Log(LogLevel level, string message);
}

// Logger implementation
class Logger : ILogger
{
    public void LogEvent(LogLevel level, string message)
    {
        Console.WriteLine($"[{level}] Event: {message}");
    }

    public void Log(LogLevel level, string message)
    {
        Console.WriteLine($"[{level}] {message}");
    }
}

// User data structure
class User
{
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public bool IsLoggedIn { get; set; }

    public User(string username, string passwordHash)
    {
        Username = username;
        PasswordHash = passwordHash;
        IsLoggedIn = false;
    }
}

// Authentication service handling user registration and login
class AuthenticationService
{
    private readonly string usersFilePath = "users.txt";
    private List<User> users = new List<User>();
    private readonly ILogger logger;

    public AuthenticationService(ILogger logger)
    {
        this.logger = logger;
        try
        {
            LoadUsersFromDisk();
        }
        catch (Exception ex)
        {
            logger.LogEvent(LogLevel.Error, $"Failed to load users: {ex.Message}");
        }
    }

    private void LoadUsersFromDisk()
    {
        if (File.Exists(usersFilePath))
        {
            try
            {
                var lines = File.ReadAllLines(usersFilePath);
                foreach (var line in lines)
                {
                    var parts = line.Split(',');
                    if (parts.Length == 2)
                    {
                        users.Add(new User(parts[0], parts[1]));
                    }
                    else
                    {
                        logger.Log(LogLevel.Warning, $"Invalid user data format: {line}");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"Error reading user file: {ex.Message}");
            }
        }
    }

    private void SaveUsersToDisk()
    {
        try
        {
            var lines = new List<string>();
            foreach (var user in users)
            {
                lines.Add($"{user.Username},{user.PasswordHash}");
            }
            File.WriteAllLines(usersFilePath, lines);
        }
        catch (Exception ex)
        {
            logger.LogEvent(LogLevel.Error, $"Error saving users: {ex.Message}");
        }
    }

    private string HashPassword(string password)
    {
        try
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
        catch (Exception ex)
        {
            logger.LogEvent(LogLevel.Error, $"Error hashing password: {ex.Message}");
            return string.Empty;
        }
    }

    public bool AuthenticateUser(string username, string password)
    {
        if (IsValidUsername(username) && IsValidPassword(password))
        {
            var user = users.Find(u => u.Username == username);
            if (user != null && user.PasswordHash == HashPassword(password))
            {
                user.IsLoggedIn = true;
                logger.LogEvent(LogLevel.Info, $"User {username} logged in successfully.");
                return true;
            }
            else
            {
                logger.LogEvent(LogLevel.Warning, "Invalid username or password.");
                return false;
            }
        }
        else
        {
            logger.LogEvent(LogLevel.Warning, "Invalid input format.");
            return false;
        }
    }

    public bool RegisterUser(string username, string password)
    {
        if (!IsValidUsername(username))
        {
            logger.LogEvent(LogLevel.Warning, "Invalid username format.");
            return false;
        }
        if (!IsValidPassword(password))
        {
            logger.LogEvent(LogLevel.Warning, "Invalid password format.");
            return false;
        }
        if (users.Exists(u => u.Username == username))
        {
            logger.LogEvent(LogLevel.Warning, "Username already exists.");
            return false;
        }

        var passwordHash = HashPassword(password);
        users.Add(new User(username, passwordHash));
        SaveUsersToDisk();
        logger.LogEvent(LogLevel.Info, $"User {username} registered.");
        return true;
    }

    private bool IsValidUsername(string username)
    {
        return !string.IsNullOrWhiteSpace(username) && username.Length >= 3;
    }

    private bool IsValidPassword(string password)
    {
        return !string.IsNullOrWhiteSpace(password) && password.Length >= 6;
    }
}

// Define a strategy interface for block allocation
interface IAllocationStrategy
{
    int Allocate(bool[] allocationTable, int numberOfBlocks);
}

// Implement contiguous block allocation strategy
class ContiguousAllocationStrategy : IAllocationStrategy
{
    public int Allocate(bool[] allocationTable, int numberOfBlocks)
    {
        int contiguousCount = 0;
        for (int i = 0; i < allocationTable.Length; i++)
        {
            if (!allocationTable[i])
            {
                contiguousCount++;
                if (contiguousCount == numberOfBlocks)
                {
                    return i - numberOfBlocks + 1;
                }
            }
            else
            {
                contiguousCount = 0;
            }
        }
        return -1;
    }
}

// Define types and factory for file system nodes
class FileNode
{
    public string Name { get; set; }
    public bool IsDirectory { get; set; }
    public List<FileNode> Children { get; set; }

    public FileNode(string name, bool isDirectory)
    {
        Name = name;
        IsDirectory = isDirectory;
        Children = new List<FileNode>();
    }

    public void AddChild(FileNode child)
    {
        if (IsDirectory)
        {
            Children.Add(child);
        }
        else
        {
            throw new InvalidOperationException("Cannot add children to a file node.");
        }
    }

    public void RemoveChild(string name)
    {
        Children.RemoveAll(child => child.Name == name);
    }

    public FileNode FindChild(string name)
    {
        return Children.Find(child => child.Name == name);
    }

    public List<FileNode> ListChildren()
    {
        if (IsDirectory)
        {
            return Children;
        }
        else
        {
            throw new InvalidOperationException("Cannot list children of a file node.");
        }
    }
}

static class FileNodeFactory
{
    public static FileNode CreateDirectory(string name)
    {
        return new FileNode(name, true);
    }

    public static FileNode CreateFile(string name)
    {
        return new FileNode(name, false);
    }
}

// Define a device type enumeration
enum DeviceType
{
    Fixed,
    Removable
}

// Define an interface for devices
interface IDevice
{
    string DeviceName { get; }
    int BlockSize { get; }
    Guid DeviceId { get; }
    DeviceType Type { get; }
    string Status { get; set; }
    byte[] Storage { get; }
    bool[] AllocationTable { get; }
    FileNode FileSystemRoot { get; }
}

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
class Program
{
    static void Main(string[] args)
    {
        ILogger logger = new Logger();
        var authService = new AuthenticationService(logger);

        // User registration
        Console.WriteLine("Registering users...");
        authService.RegisterUser("user1", "password123");
        authService.RegisterUser("user2", "securePassword");

        // User authentication
        Console.WriteLine("Authenticating users...");
        var isAuthenticatedUser1 = authService.AuthenticateUser("user1", "password123");
        Console.WriteLine($"User1 authentication success: {isAuthenticatedUser1}");

        var isAuthenticatedUser2 = authService.AuthenticateUser("user2", "wrongPassword");
        Console.WriteLine($"User2 authentication success: {isAuthenticatedUser2}");

        var isAuthenticatedUser3 = authService.AuthenticateUser("user2", "securePassword");
        Console.WriteLine($"User2 authentication success: {isAuthenticatedUser3}");

        // Device operations
        Console.WriteLine("Creating a device...");
        var allocationStrategy = new ContiguousAllocationStrategy();
        var device = new Device("TestDevice", 512, DeviceType.Fixed, 1024 * 1024, allocationStrategy, logger);

        // Allocate and write data
        Console.WriteLine("Allocating blocks and writing data...");
        var startIndex = device.AllocateBlocks(1);
        var dataToWrite = new byte[] { 10, 20, 30 };
        device.WriteData(startIndex, dataToWrite);

        // Read data
        Console.WriteLine("Reading data from device...");
        var readData = device.ReadData(startIndex, dataToWrite.Length);
        Console.WriteLine("Read data: " + string.Join(", ", readData));
    }
}
