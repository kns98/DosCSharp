
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
