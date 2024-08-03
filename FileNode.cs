
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
