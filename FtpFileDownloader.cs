// FtpFileDownloader.cs
using FluentFTP;
using System;
using System.IO;
using System.Threading.Tasks;

public class FtpFileDownloader
{
    private readonly AsyncFtpClient _ftpClient;
    
    public FtpFileDownloader(AsyncFtpClient ftpClient)
    {
        _ftpClient = ftpClient ?? throw new ArgumentNullException(nameof(ftpClient));
    }
    
    public async Task<bool> DownloadFileAsync(string remoteFilePath, string localFilePath)
    {
        try
        {
            // 確保本地目錄存在
            string localDir = Path.GetDirectoryName(localFilePath);
            if (!Directory.Exists(localDir))
            {
                Console.WriteLine($"建立本地目錄: {localDir}");
                Directory.CreateDirectory(localDir);
            }
            
            // 檢查遠端檔案是否存在
            bool fileExists = await _ftpClient.FileExists(remoteFilePath);
            if (!fileExists)
            {
                Console.WriteLine($"遠端檔案不存在: {remoteFilePath}");
                return false;
            }
            
            // 下載檔案
            Console.WriteLine($"開始從 {remoteFilePath} 下載檔案至: {localFilePath}");
            var status = await _ftpClient.DownloadFile(
                localFilePath, 
                remoteFilePath,
                FtpLocalExists.Overwrite);
                
            if (status == FtpStatus.Success)
            {
                Console.WriteLine("檔案下載成功！");
                return true;
            }
            else
            {
                Console.WriteLine($"檔案下載失敗，狀態: {status}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"下載檔案時發生錯誤: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"內部錯誤: {ex.InnerException.Message}");
            return false;
        }
    }
    
    public async Task<bool> DownloadDirectoryAsync(string remoteDir, string localDir, FtpFolderSyncMode syncMode = FtpFolderSyncMode.Update)
    {
        try
        {
            // 確保本地目錄存在
            if (!Directory.Exists(localDir))
            {
                Console.WriteLine($"建立本地目錄: {localDir}");
                Directory.CreateDirectory(localDir);
            }
            
            // 下載整個目錄
            var results = await _ftpClient.DownloadDirectory(
                localDir,
                remoteDir,
                syncMode,
                FtpLocalExists.Overwrite);
                
            bool allSucceeded = results.All(r => r.IsSuccess);
            
            if (allSucceeded)
            {
                Console.WriteLine("目錄下載成功！");
                return true;
            }
            else
            {
                Console.WriteLine("部分檔案下載失敗。");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"下載目錄時發生錯誤: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"內部錯誤: {ex.InnerException.Message}");
            return false;
        }
    }
    
    // 新增方法：檢查遠端檔案是否存在
    public async Task<bool> FileExistsAsync(string remoteFilePath)
    {
        try
        {
            return await _ftpClient.FileExists(remoteFilePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"檢查檔案存在時發生錯誤: {ex.Message}");
            return false;
        }
    }
    
    // 新增方法：獲取檔案大小
    public async Task<long> GetFileSizeAsync(string remoteFilePath)
    {
        try
        {
            return await _ftpClient.GetFileSize(remoteFilePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"獲取檔案大小時發生錯誤: {ex.Message}");
            return -1;
        }
    }
    
    // 新增方法：獲取檔案修改時間
    public async Task<DateTime> GetLastModifiedTimeAsync(string remoteFilePath)
    {
        try
        {
            return await _ftpClient.GetModifiedTime(remoteFilePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"獲取檔案修改時間時發生錯誤: {ex.Message}");
            return DateTime.MinValue;
        }
    }
}
