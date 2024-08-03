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
