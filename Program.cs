// Program.cs
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        // FTPS ���A����T
        string host = "192.168.1.15";
        int port = 21;
        string user = "ian";
        string pass = "ianwang";

        // �W���ɮת��Ѽ�
        string localDir = @"d:/temp/FATA/";
        string fileName = "OD0571.7z";
        string localFilePath = localDir + fileName;
        string remoteDir = "/SA/Not ZIP/_TempFile/5000_JIN/";

        // �U���ɮת��Ѽ�
        string downloadDir = @"d:/temp/FATA/Download/";
        string downloadFilePath = downloadDir + "Downloaded_" + fileName;
        
        try
        {
            // �T�O�U���ؿ��s�b
            if (!Directory.Exists(downloadDir))
            {
                Directory.CreateDirectory(downloadDir);
            }

            // �ϥ� using �϶��ӽT�O�귽�Q���T����
            await using (var connection = new FtpConnection(host, port, user, pass))
            {
                // �s�u�� FTP ���A��
                await connection.ConnectAsync();

                // �إߤW�Ǿ�
                var uploader = new FtpFileUploader(connection.Client);

                // �إߤU����
                var downloader = new FtpFileDownloader(connection.Client);

                // �W�ǳ�@�ɮר�����
                bool fileUploadSuccess = await uploader.UploadAndVerifyFileAsync(localFilePath, remoteDir, fileName);

                // �W�Ǿ�ӥؿ�������
                //bool directoryUploadSuccess = await uploader.UploadAndVerifyDirectoryAsync(localDir, remoteDir);
                // �ˬd�W�ǵ��G
                if (fileUploadSuccess)
                {
                    Console.WriteLine("�ɮפW�ǳB�z�����C");
                }
                else
                {
                    Console.WriteLine("�ɮפW�ǳB�z���ѡC");
                }
                // �U���������ɮ�
                bool downloadSuccess = await downloader.DownloadFileAsync(remoteDir + fileName, downloadFilePath);
                // �ˬd�U�����G
                if (downloadSuccess)
                {
                    Console.WriteLine("�ɮפU���B�z�����C");
                }
                else
                {
                    Console.WriteLine("�ɮפU���B�z���ѡC");
                }


            } // �۰ʩI�s DisposeAsync
        }
        catch (Exception ex)
        {
            Console.WriteLine($"�{������ɵo�ͥ��B�z�����~: {ex.Message}");
        }
    }
}
