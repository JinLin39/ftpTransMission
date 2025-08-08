// FtpConnection.cs
using FluentFTP;
using FluentFTP.Exceptions;
using System;
using System.Security.Authentication;
using System.Threading.Tasks;

public class FtpConnection : IDisposable, IAsyncDisposable
{
    private readonly AsyncFtpClient _ftpClient;
    
    public FtpConnection(string host, int port, string user, string password)
    {
        // �إ� AsyncFtpClient
        _ftpClient = new AsyncFtpClient(host, user, password, port);
        
        // �]�w FTPS
        _ftpClient.Config.SslProtocols = SslProtocols.Tls;
        _ftpClient.Config.EncryptionMode = FtpEncryptionMode.Explicit;
        _ftpClient.Config.ValidateAnyCertificate = true;
    }
    
    public AsyncFtpClient Client => _ftpClient;
    
    public async Task ConnectAsync()
    {
        try
        {
            Console.WriteLine("���ճs���� FTP ���A��...");
            await _ftpClient.Connect();
            Console.WriteLine("FTPS �s�u���\�I");
        }
        catch (FtpAuthenticationException authEx)
        {
            Console.WriteLine($"���ҥ���: {authEx.Message}");
            if (authEx.InnerException != null)
                Console.WriteLine($"�������~: {authEx.InnerException.Message}");
            Console.WriteLine("�нT�{�Τ�W�M�K�X�O�_���T�C");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"�s�u����: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"�������~: {ex.InnerException.Message}");
            throw;
        }
    }
    
    public async Task DisconnectAsync()
    {
        if (_ftpClient != null && _ftpClient.IsConnected)
        {
            await _ftpClient.Disconnect();
        }
    }
    
    public void Dispose()
    {
        _ftpClient?.Dispose();
    }
    public async ValueTask DisposeAsync()
    {
        if (_ftpClient != null)
        {
            await _ftpClient.Disconnect(); // Corrected method name
            _ftpClient.Dispose();
        }
    }
}
