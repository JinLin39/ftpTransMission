// FtpFileDeleter.cs
using FluentFTP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public class FtpFileDeleter
{
    private readonly AsyncFtpClient _ftpClient;

    public FtpFileDeleter(AsyncFtpClient ftpClient)
    {
        _ftpClient = ftpClient ?? throw new ArgumentNullException(nameof(ftpClient));
    }

    /// <summary>
    /// 刪除指定遠端目錄中含有特定關鍵字的檔案，支援萬用字元（* 和 ?）
    /// </summary>
    /// <param name="remoteDir">遠端目錄路徑</param>
    /// <param name="patterns">要篩選的關鍵字或模式列表，支援萬用字元 * 和 ?</param>
    /// <param name="allPatternsMustMatch">是否所有模式都必須符合</param>
    /// <returns>刪除成功的檔案數量</returns>
    public async Task<int> DeleteFilesByKeywordsAsync(string remoteDir, List<string> patterns, bool allPatternsMustMatch = false)
    {
        try
        {
            // 確保遠端目錄格式正確
            if (!remoteDir.EndsWith("/"))
                remoteDir += "/";

            // 取得目錄中所有檔案
            var files = await _ftpClient.GetListing(remoteDir);

            // 將萬用字元模式轉換為正則表達式
            var regexPatterns = patterns.Select(pattern =>
                new Regex(
                    "^" + Regex.Escape(pattern)
                        .Replace("\\*", ".*")
                        .Replace("\\?", ".") + "$",
                    RegexOptions.IgnoreCase
                )
            ).ToList();

            // 根據模式篩選檔案
            var filesToDelete = files
                .Where(item => item.Type == FtpObjectType.File)
                .Where(file => {
                    if (allPatternsMustMatch)
                    {
                        return regexPatterns.All(regex => regex.IsMatch(file.Name));
                    }
                    else
                    {
                        return regexPatterns.Any(regex => regex.IsMatch(file.Name));
                    }
                })
                .ToList();

            // 刪除符合條件的檔案
            int successCount = 0;

            foreach (var file in filesToDelete)
            {
                string fullPath = remoteDir + file.Name;
                try
                {
                    await _ftpClient.DeleteFile(fullPath);
                    successCount++;
                }
                catch (Exception)
                {
                    // 忽略刪除失敗的錯誤
                }
            }

            return successCount;
        }
        catch (Exception)
        {
            return 0; // 發生錯誤時返回0
        }
    }

    /// <summary>
    /// 刪除指定的遠端檔案
    /// </summary>
    /// <param name="remoteFilePath">遠端檔案完整路徑</param>
    /// <returns>是否刪除成功</returns>
    public async Task<bool> DeleteFileAsync(string remoteFilePath)
    {
        try
        {
            // 檢查檔案是否存在
            bool exists = await _ftpClient.FileExists(remoteFilePath);
            if (!exists)
            {
                return false;
            }

            // 刪除檔案
            await _ftpClient.DeleteFile(remoteFilePath);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
