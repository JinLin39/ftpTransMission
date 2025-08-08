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
            // �T�O���a�ؿ��s�b
            string localDir = Path.GetDirectoryName(localFilePath);
            if (!Directory.Exists(localDir))
            {
                Console.WriteLine($"�إߥ��a�ؿ�: {localDir}");
                Directory.CreateDirectory(localDir);
            }
            
            // �ˬd�����ɮ׬O�_�s�b
            bool fileExists = await _ftpClient.FileExists(remoteFilePath);
            if (!fileExists)
            {
                Console.WriteLine($"�����ɮפ��s�b: {remoteFilePath}");
                return false;
            }
            
            // �U���ɮ�
            Console.WriteLine($"�}�l�q {remoteFilePath} �U���ɮצ�: {localFilePath}");
            var status = await _ftpClient.DownloadFile(
                localFilePath, 
                remoteFilePath,
                FtpLocalExists.Overwrite);
                
            if (status == FtpStatus.Success)
            {
                Console.WriteLine("�ɮפU�����\�I");
                return true;
            }
            else
            {
                Console.WriteLine($"�ɮפU�����ѡA���A: {status}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"�U���ɮ׮ɵo�Ϳ��~: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"�������~: {ex.InnerException.Message}");
            return false;
        }
    }
    
    public async Task<bool> DownloadDirectoryAsync(string remoteDir, string localDir, FtpFolderSyncMode syncMode = FtpFolderSyncMode.Update)
    {
        try
        {
            // �T�O���a�ؿ��s�b
            if (!Directory.Exists(localDir))
            {
                Console.WriteLine($"�إߥ��a�ؿ�: {localDir}");
                Directory.CreateDirectory(localDir);
            }
            
            // �U����ӥؿ�
            var results = await _ftpClient.DownloadDirectory(
                localDir,
                remoteDir,
                syncMode,
                FtpLocalExists.Overwrite);
                
            bool allSucceeded = results.All(r => r.IsSuccess);
            
            if (allSucceeded)
            {
                Console.WriteLine("�ؿ��U�����\�I");
                return true;
            }
            else
            {
                Console.WriteLine("�����ɮפU�����ѡC");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"�U���ؿ��ɵo�Ϳ��~: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"�������~: {ex.InnerException.Message}");
            return false;
        }
    }
    
    // �s�W��k�G�ˬd�����ɮ׬O�_�s�b
    public async Task<bool> FileExistsAsync(string remoteFilePath)
    {
        try
        {
            return await _ftpClient.FileExists(remoteFilePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"�ˬd�ɮצs�b�ɵo�Ϳ��~: {ex.Message}");
            return false;
        }
    }
    
    // �s�W��k�G����ɮפj�p
    public async Task<long> GetFileSizeAsync(string remoteFilePath)
    {
        try
        {
            return await _ftpClient.GetFileSize(remoteFilePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"����ɮפj�p�ɵo�Ϳ��~: {ex.Message}");
            return -1;
        }
    }
    
    // �s�W��k�G����ɮ׭ק�ɶ�
    public async Task<DateTime> GetLastModifiedTimeAsync(string remoteFilePath)
    {
        try
        {
            return await _ftpClient.GetModifiedTime(remoteFilePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"����ɮ׭ק�ɶ��ɵo�Ϳ��~: {ex.Message}");
            return DateTime.MinValue;
        }
    }
}
