// Program.cs
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        // FTPS 伺服器資訊
        string host = "192.168.1.15";
        int port = 21;
        string user = "ian";
        string pass = "ianwang";

        // 上傳檔案的參數
        string localDir = @"d:/temp/FATA/";
        string fileName = "OD0571.7z";
        string localFilePath = localDir + fileName;
        string remoteDir = "/SA/Not ZIP/_TempFile/5000_JIN/";

        // 下載檔案的參數
        string downloadDir = @"d:/temp/FATA/Download/";
        string downloadFilePath = downloadDir + "Downloaded_" + fileName;
        
        try
        {
            // 確保下載目錄存在
            if (!Directory.Exists(downloadDir))
            {
                Directory.CreateDirectory(downloadDir);
            }

            // 使用 using 區塊來確保資源被正確釋放
            await using (var connection = new FtpConnection(host, port, user, pass))
            {
                // 連線到 FTP 伺服器
                await connection.ConnectAsync();

                // 建立上傳器
                var uploader = new FtpFileUploader(connection.Client);

                // 建立下載器
                var downloader = new FtpFileDownloader(connection.Client);

                // 上傳單一檔案並驗證
                bool fileUploadSuccess = await uploader.UploadAndVerifyFileAsync(localFilePath, remoteDir, fileName);

                // 上傳整個目錄並驗證
                //bool directoryUploadSuccess = await uploader.UploadAndVerifyDirectoryAsync(localDir, remoteDir);
                // 檢查上傳結果
                if (fileUploadSuccess)
                {
                    Console.WriteLine("檔案上傳處理完成。");
                }
                else
                {
                    Console.WriteLine("檔案上傳處理失敗。");
                }
                // 下載並驗證檔案
                bool downloadSuccess = await downloader.DownloadFileAsync(remoteDir + fileName, downloadFilePath);
                // 檢查下載結果
                if (downloadSuccess)
                {
                    Console.WriteLine("檔案下載處理完成。");
                }
                else
                {
                    Console.WriteLine("檔案下載處理失敗。");
                }


            } // 自動呼叫 DisposeAsync
        }
        catch (Exception ex)
        {
            Console.WriteLine($"程式執行時發生未處理的錯誤: {ex.Message}");
        }
    }
}
