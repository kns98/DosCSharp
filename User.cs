
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
