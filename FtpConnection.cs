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
        // 建立 AsyncFtpClient
        _ftpClient = new AsyncFtpClient(host, user, password, port);
        
        // 設定 FTPS
        _ftpClient.Config.SslProtocols = SslProtocols.Tls;
        _ftpClient.Config.EncryptionMode = FtpEncryptionMode.Explicit;
        _ftpClient.Config.ValidateAnyCertificate = true;
    }
    
    public AsyncFtpClient Client => _ftpClient;
    
    public async Task ConnectAsync()
    {
        try
        {
            Console.WriteLine("嘗試連接到 FTP 伺服器...");
            await _ftpClient.Connect();
            Console.WriteLine("FTPS 連線成功！");
        }
        catch (FtpAuthenticationException authEx)
        {
            Console.WriteLine($"驗證失敗: {authEx.Message}");
            if (authEx.InnerException != null)
                Console.WriteLine($"內部錯誤: {authEx.InnerException.Message}");
            Console.WriteLine("請確認用戶名和密碼是否正確。");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"連線失敗: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"內部錯誤: {ex.InnerException.Message}");
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
