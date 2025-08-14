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
    /// �R�����w���ݥؿ����t���S�w����r���ɮסA�䴩�U�Φr���]* �M ?�^
    /// </summary>
    /// <param name="remoteDir">���ݥؿ����|</param>
    /// <param name="patterns">�n�z�諸����r�μҦ��C��A�䴩�U�Φr�� * �M ?</param>
    /// <param name="allPatternsMustMatch">�O�_�Ҧ��Ҧ��������ŦX</param>
    /// <returns>�R�����\���ɮ׼ƶq</returns>
    public async Task<int> DeleteFilesByKeywordsAsync(string remoteDir, List<string> patterns, bool allPatternsMustMatch = false)
    {
        try
        {
            // �T�O���ݥؿ��榡���T
            if (!remoteDir.EndsWith("/"))
                remoteDir += "/";

            // ���o�ؿ����Ҧ��ɮ�
            var files = await _ftpClient.GetListing(remoteDir);

            // �N�U�Φr���Ҧ��ഫ�����h��F��
            var regexPatterns = patterns.Select(pattern =>
                new Regex(
                    "^" + Regex.Escape(pattern)
                        .Replace("\\*", ".*")
                        .Replace("\\?", ".") + "$",
                    RegexOptions.IgnoreCase
                )
            ).ToList();

            // �ھڼҦ��z���ɮ�
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

            // �R���ŦX�����ɮ�
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
                    // �����R�����Ѫ����~
                }
            }

            return successCount;
        }
        catch (Exception)
        {
            return 0; // �o�Ϳ��~�ɪ�^0
        }
    }

    /// <summary>
    /// �R�����w�������ɮ�
    /// </summary>
    /// <param name="remoteFilePath">�����ɮק�����|</param>
    /// <returns>�O�_�R�����\</returns>
    public async Task<bool> DeleteFileAsync(string remoteFilePath)
    {
        try
        {
            // �ˬd�ɮ׬O�_�s�b
            bool exists = await _ftpClient.FileExists(remoteFilePath);
            if (!exists)
            {
                return false;
            }

            // �R���ɮ�
            await _ftpClient.DeleteFile(remoteFilePath);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
