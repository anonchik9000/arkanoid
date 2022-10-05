using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Gaming.ContainerManager.Client.SocketContracts.V1;

public class UnreliableSocket : BaseSocket
{
    public UnreliableSocket()
    {
        Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    }

    public override async Task Run()
    {
        var rcvBytes = new byte[64000];
        var rcvBuffer = new ArraySegment<byte>(rcvBytes);

        while (true)
        {
            var rcvResult = await Socket.ReceiveAsync(rcvBuffer, SocketFlags.None);

            var msgBytes = rcvBuffer.Skip(rcvBuffer.Offset).Take(rcvResult).ToArray();
            var message = SocketMessage.Message(msgBytes);

            foreach (var subscriber in Subscribers)
                await subscriber.Invoke(message);
        }
    }

    public override async ValueTask<SocketSendResult> SendAsync(ReadOnlyMemory<byte> message)
    {
        if (isDisposed)
            return new SocketSendResult(SocketSendResultType.SocketClosed, null);

        try
        {
            await Socket.SendToAsync(message.ToArray(), SocketFlags.None, Socket.RemoteEndPoint);

            return new SocketSendResult(SocketSendResultType.Ok, null);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new SocketSendResult(SocketSendResultType.Unknown, e.ToString());
        }
    }
}