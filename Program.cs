using System;
using System.IO;
using System.Collections.Generic;
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

        var s = new SmallOS.Scheduler();
        // Task operations
        s.CreateTask("Task 1", 1);
        s.CreateTask("Task 2", 2);
        s.StartTask(1);
        //s.SendMessage(1, 2, "Hello, Task 2!");
        s.PauseTask(1);
        s.ResumeTask(1);
        s.StopTask(1);
        s.DeleteTask(1);


        // Read data
        Console.WriteLine("Reading data from device...");
        var readData = device.ReadData(startIndex, dataToWrite.Length);
        Console.WriteLine("Read data: " + string.Join(", ", readData));
    }
}
