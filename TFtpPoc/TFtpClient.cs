using System.Net;
using System.Text;
using Tftp.Net;

namespace TFtpPoc;

internal class TFtpClient
{
    //private const int DEFAULT_SERVER_PORT = 69;
    private readonly TftpClient _tftpClient;
    private static readonly AutoResetEvent TransferFinishedEvent = new AutoResetEvent(false);
    public TFtpClient(IPAddress ip, int port)
    {
        _tftpClient = new TftpClient(ip, port);
    }
    public TFtpClient(string address)
    {
        _tftpClient = new TftpClient(address);
    }

    public void ReadFile(string fileName)
    {

        using Stream stream = new MemoryStream();
        var content = string.Empty;

        var transfer = _tftpClient.Download(fileName);
        transfer.RetryCount = 3;
        transfer.RetryTimeout= TimeSpan.FromSeconds(2);
        transfer.TransferMode = TftpTransferMode.octet;

        transfer.OnProgress += (_, progress) => { Console.WriteLine("Transfer running. Progress: " + progress); };

        transfer.OnFinished += (_) =>
        {
            Console.WriteLine("Succeed");
            using var reader = new StreamReader(stream);
            content = reader.ReadLine();
            stream.Position = 0;
            TransferFinishedEvent.Set();
        };

        transfer.OnError += (_, error) =>
        {
            Console.WriteLine("error" + error);
            stream.Dispose();
            TransferFinishedEvent.Set();
        };
        
        transfer.Start(stream);

        TransferFinishedEvent.WaitOne();
        Console.ReadKey();
    }
    public void WriteToFile(string fileName, string content)
    {
        try
        {
            var byteArray = Encoding.ASCII.GetBytes(content);
            var stream = new MemoryStream(byteArray);

            var transfer = _tftpClient.Upload(fileName);
            transfer.RetryCount = 3;
            transfer.RetryTimeout = TimeSpan.FromSeconds(1);

            transfer.OnProgress += (_, progress) =>
            {
                Console.WriteLine("Transfer running. Progress: " + progress);
            };

            transfer.OnFinished += _ =>
            {
                Console.WriteLine("Transfer succeeded.");
                stream.Dispose();
                TransferFinishedEvent.Set();
            };

            transfer.OnError += (_, error) =>
            {
                Console.WriteLine("Transfer failed: " + error);
                stream.Dispose();
                TransferFinishedEvent.Set();
            };

            transfer.Start(stream);

            TransferFinishedEvent.WaitOne();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}