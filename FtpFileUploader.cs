// FtpFileUploader.cs
using FluentFTP;
using System;
using System.IO;
using System.Threading.Tasks;

public class FtpFileUploader
{
    private readonly AsyncFtpClient _ftpClient;

    public FtpFileUploader(AsyncFtpClient ftpClient)
    {
        _ftpClient = ftpClient ?? throw new ArgumentNullException(nameof(ftpClient));
    }

    public async Task<bool> UploadFileAsync(string localFilePath, string remoteDir, string fileName)
    {
        try
        {
            string remoteFilePath = remoteDir + fileName;

            // 確保遠端目錄存在
            bool dirExists = await _ftpClient.DirectoryExists(remoteDir);
            if (!dirExists)
            {
                Console.WriteLine($"建立遠端目錄: {remoteDir}");
                await _ftpClient.CreateDirectory(remoteDir);
            }

            // 上傳檔案
            Console.WriteLine($"開始上傳檔案至: {remoteFilePath}");
            var status = await _ftpClient.UploadFile(
                localFilePath,
                remoteFilePath,
                FtpRemoteExists.Overwrite,
                true);

            if (status == FtpStatus.Success)
            {
                Console.WriteLine("檔案上傳成功！");
                return true;
            }
            else
            {
                Console.WriteLine($"檔案上傳失敗，狀態: {status}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"上傳檔案時發生錯誤: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"內部錯誤: {ex.InnerException.Message}");
            return false;
        }
    }

    // 新增方法：確認檔案上傳成功 (驗證檔案存在及檔案大小)
    public async Task<bool> VerifyFileUploadedAsync(string localFilePath, string remoteFilePath)
    {
        try
        {
            // 檢查遠端檔案是否存在
            bool fileExists = await _ftpClient.FileExists(remoteFilePath);
            if (!fileExists)
            {
                Console.WriteLine($"遠端檔案不存在，上傳失敗: {remoteFilePath}");
                return false;
            }

            // 檢查檔案大小是否一致
            long localFileSize = new FileInfo(localFilePath).Length;
            long remoteFileSize = await _ftpClient.GetFileSize(remoteFilePath);

            if (localFileSize != remoteFileSize)
            {
                Console.WriteLine($"檔案大小不一致，上傳可能不完整。本地: {localFileSize} 位元組, 遠端: {remoteFileSize} 位元組");
                return false;
            }

            Console.WriteLine($"檔案上傳驗證成功: {remoteFilePath}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"驗證檔案上傳時發生錯誤: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"內部錯誤: {ex.InnerException.Message}");
            return false;
        }
    }

    // 改進版上傳方法，包含驗證步驟
    public async Task<bool> UploadAndVerifyFileAsync(string localFilePath, string remoteDir, string fileName)
    {
        try
        {
            string remoteFilePath = remoteDir + fileName;

            // 執行上傳
            bool uploadSuccess = await UploadFileAsync(localFilePath, remoteDir, fileName);
            if (!uploadSuccess)
            {
                return false;
            }

            // 驗證上傳
            return await VerifyFileUploadedAsync(localFilePath, remoteFilePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"上傳和驗證檔案時發生錯誤: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"內部錯誤: {ex.InnerException.Message}");
            return false;
        }
    }

    public async Task<bool> UploadDirectoryAsync(string localDir, string remoteDir, FtpFolderSyncMode syncMode = FtpFolderSyncMode.Update)
    {
        try
        {
            // 上傳整個目錄
            var results = await _ftpClient.UploadDirectory(
                localDir,
                remoteDir,
                syncMode,
                FtpRemoteExists.Overwrite);

            bool allSucceeded = results.All(r => r.IsSuccess);

            if (allSucceeded)
            {
                Console.WriteLine("目錄上傳成功！");
                return true;
            }
            else
            {
                Console.WriteLine("部分檔案上傳失敗。");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"上傳目錄時發生錯誤: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"內部錯誤: {ex.InnerException.Message}");
            return false;
        }
    }

    // 新增方法：驗證目錄上傳結果
    public async Task<bool> VerifyDirectoryUploadedAsync(string localDir, string remoteDir)
    {
        try
        {
            // 獲取本地檔案列表
            var localFiles = Directory.GetFiles(localDir, "*", SearchOption.AllDirectories)
                                    .Select(f => new FileInfo(f))
                                    .ToList();

            bool allVerified = true;

            foreach (var localFile in localFiles)
            {
                // 計算相對路徑
                string relativePath = localFile.FullName.Substring(localDir.TrimEnd('\\').Length).Replace('\\', '/');
                if (relativePath.StartsWith("/"))
                    relativePath = relativePath.Substring(1);

                string remoteFilePath = remoteDir.TrimEnd('/') + "/" + relativePath;

                // 驗證檔案
                bool fileVerified = await VerifyFileUploadedAsync(localFile.FullName, remoteFilePath);
                if (!fileVerified)
                {
                    allVerified = false;
                    Console.WriteLine($"檔案驗證失敗: {remoteFilePath}");
                }
            }

            return allVerified;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"驗證目錄上傳時發生錯誤: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"內部錯誤: {ex.InnerException.Message}");
            return false;
        }
    }

    // 改進版上傳目錄方法，包含驗證步驟
    public async Task<bool> UploadAndVerifyDirectoryAsync(string localDir, string remoteDir, FtpFolderSyncMode syncMode = FtpFolderSyncMode.Update)
    {
        try
        {
            // 執行上傳
            bool uploadSuccess = await UploadDirectoryAsync(localDir, remoteDir, syncMode);
            if (!uploadSuccess)
            {
                return false;
            }

            // 驗證上傳
            return await VerifyDirectoryUploadedAsync(localDir, remoteDir);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"上傳和驗證目錄時發生錯誤: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"內部錯誤: {ex.InnerException.Message}");
            return false;
        }
    }
}
