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

            // �T�O���ݥؿ��s�b
            bool dirExists = await _ftpClient.DirectoryExists(remoteDir);
            if (!dirExists)
            {
                Console.WriteLine($"�إ߻��ݥؿ�: {remoteDir}");
                await _ftpClient.CreateDirectory(remoteDir);
            }

            // �W���ɮ�
            Console.WriteLine($"�}�l�W���ɮצ�: {remoteFilePath}");
            var status = await _ftpClient.UploadFile(
                localFilePath,
                remoteFilePath,
                FtpRemoteExists.Overwrite,
                true);

            if (status == FtpStatus.Success)
            {
                Console.WriteLine("�ɮפW�Ǧ��\�I");
                return true;
            }
            else
            {
                Console.WriteLine($"�ɮפW�ǥ��ѡA���A: {status}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"�W���ɮ׮ɵo�Ϳ��~: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"�������~: {ex.InnerException.Message}");
            return false;
        }
    }

    // �s�W��k�G�T�{�ɮפW�Ǧ��\ (�����ɮצs�b���ɮפj�p)
    public async Task<bool> VerifyFileUploadedAsync(string localFilePath, string remoteFilePath)
    {
        try
        {
            // �ˬd�����ɮ׬O�_�s�b
            bool fileExists = await _ftpClient.FileExists(remoteFilePath);
            if (!fileExists)
            {
                Console.WriteLine($"�����ɮפ��s�b�A�W�ǥ���: {remoteFilePath}");
                return false;
            }

            // �ˬd�ɮפj�p�O�_�@�P
            long localFileSize = new FileInfo(localFilePath).Length;
            long remoteFileSize = await _ftpClient.GetFileSize(remoteFilePath);

            if (localFileSize != remoteFileSize)
            {
                Console.WriteLine($"�ɮפj�p���@�P�A�W�ǥi�ण����C���a: {localFileSize} �줸��, ����: {remoteFileSize} �줸��");
                return false;
            }

            Console.WriteLine($"�ɮפW�����Ҧ��\: {remoteFilePath}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"�����ɮפW�Ǯɵo�Ϳ��~: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"�������~: {ex.InnerException.Message}");
            return false;
        }
    }

    // ��i���W�Ǥ�k�A�]�t���ҨB�J
    public async Task<bool> UploadAndVerifyFileAsync(string localFilePath, string remoteDir, string fileName)
    {
        try
        {
            string remoteFilePath = remoteDir + fileName;

            // ����W��
            bool uploadSuccess = await UploadFileAsync(localFilePath, remoteDir, fileName);
            if (!uploadSuccess)
            {
                return false;
            }

            // ���ҤW��
            return await VerifyFileUploadedAsync(localFilePath, remoteFilePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"�W�ǩM�����ɮ׮ɵo�Ϳ��~: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"�������~: {ex.InnerException.Message}");
            return false;
        }
    }

    public async Task<bool> UploadDirectoryAsync(string localDir, string remoteDir, FtpFolderSyncMode syncMode = FtpFolderSyncMode.Update)
    {
        try
        {
            // �W�Ǿ�ӥؿ�
            var results = await _ftpClient.UploadDirectory(
                localDir,
                remoteDir,
                syncMode,
                FtpRemoteExists.Overwrite);

            bool allSucceeded = results.All(r => r.IsSuccess);

            if (allSucceeded)
            {
                Console.WriteLine("�ؿ��W�Ǧ��\�I");
                return true;
            }
            else
            {
                Console.WriteLine("�����ɮפW�ǥ��ѡC");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"�W�ǥؿ��ɵo�Ϳ��~: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"�������~: {ex.InnerException.Message}");
            return false;
        }
    }

    // �s�W��k�G���ҥؿ��W�ǵ��G
    public async Task<bool> VerifyDirectoryUploadedAsync(string localDir, string remoteDir)
    {
        try
        {
            // ������a�ɮצC��
            var localFiles = Directory.GetFiles(localDir, "*", SearchOption.AllDirectories)
                                    .Select(f => new FileInfo(f))
                                    .ToList();

            bool allVerified = true;

            foreach (var localFile in localFiles)
            {
                // �p��۹���|
                string relativePath = localFile.FullName.Substring(localDir.TrimEnd('\\').Length).Replace('\\', '/');
                if (relativePath.StartsWith("/"))
                    relativePath = relativePath.Substring(1);

                string remoteFilePath = remoteDir.TrimEnd('/') + "/" + relativePath;

                // �����ɮ�
                bool fileVerified = await VerifyFileUploadedAsync(localFile.FullName, remoteFilePath);
                if (!fileVerified)
                {
                    allVerified = false;
                    Console.WriteLine($"�ɮ����ҥ���: {remoteFilePath}");
                }
            }

            return allVerified;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"���ҥؿ��W�Ǯɵo�Ϳ��~: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"�������~: {ex.InnerException.Message}");
            return false;
        }
    }

    // ��i���W�ǥؿ���k�A�]�t���ҨB�J
    public async Task<bool> UploadAndVerifyDirectoryAsync(string localDir, string remoteDir, FtpFolderSyncMode syncMode = FtpFolderSyncMode.Update)
    {
        try
        {
            // ����W��
            bool uploadSuccess = await UploadDirectoryAsync(localDir, remoteDir, syncMode);
            if (!uploadSuccess)
            {
                return false;
            }

            // ���ҤW��
            return await VerifyDirectoryUploadedAsync(localDir, remoteDir);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"�W�ǩM���ҥؿ��ɵo�Ϳ��~: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"�������~: {ex.InnerException.Message}");
            return false;
        }
    }
}
