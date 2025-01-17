namespace Lombiq.EmailClient.Models;

public class ImapSettings
{
    public string Host { get; set; }
    public int Port { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public bool UseSsl { get; set; }

    public void CopyTo(ImapSettings target)
    {
        target.Host = Host;
        target.Port = Port;
        target.Username = Username;
        target.Password = Password;
        target.UseSsl = UseSsl;
    }
}
