using System.Net;
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
        var tftpTransfer = _tftpClient.Download(fileName);

        using Stream stream = new MemoryStream();
        tftpTransfer.Start(stream);

        TransferFinishedEvent.WaitOne();
        Console.ReadKey();
    }
    public void WriteToFile(string fileName)
    {
        var tftpTransfer = _tftpClient.Upload(fileName);

        Stream stream = new MemoryStream();
        tftpTransfer.Start(stream);
    }
}