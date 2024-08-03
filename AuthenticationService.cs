using System.Text;
using System.Security.Cryptography;
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
