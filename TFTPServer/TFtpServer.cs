using System.Net;
using Tftp.Net;

namespace TFTPServer
{
    internal class TFtpServer

    {
        private static string? _serverDirectory;

        public TFtpServer()
        {
            var server = new TftpServer();
            _serverDirectory = Environment.CurrentDirectory;
            server.OnReadRequest += new TftpServerEventHandler(server_OnReadRequest);
            server.Start();
            Console.Read();
        }

        private void server_OnReadRequest(ITftpTransfer transfer, EndPoint client)
        {
            var path = Path.Combine(_serverDirectory, transfer.Filename);
            var file = new FileInfo(path);

            if (!file.FullName.StartsWith(_serverDirectory, StringComparison.InvariantCultureIgnoreCase))
            {
                CancelTransfer(transfer, TftpErrorPacket.AccessViolation);
            }
            else if (!file.Exists)
            {
                CancelTransfer(transfer, TftpErrorPacket.FileNotFound);
            }
            else
            {
                OutputTransferStatus(transfer, "Accepting request from " + client);
                StartTransfer(transfer, new FileStream(file.FullName, FileMode.Open));
            }
        }

        private static void CancelTransfer(ITftpTransfer transfer, TftpErrorPacket reason)
        {
            OutputTransferStatus(transfer, "Cancelling transfer: " + reason.ErrorMessage);
            transfer.Cancel(reason);
        }
        private static void OutputTransferStatus(ITftpTransfer transfer, string message)
        {
            Console.WriteLine("[" + transfer.Filename + "] " + message);
        }

        private static void StartTransfer(ITftpTransfer transfer, Stream stream)
        {
            transfer.Start(stream);
        }

    }
}
