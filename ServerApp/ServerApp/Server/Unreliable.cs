﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Gaming.ContainerManager.ImageContracts.V1;
using Gaming.ContainerManager.ImageContracts.V1.Channels;
using XFlow.Utils;

namespace ServerApp.Server
{
    public class Unreliable : IUnreliableChannel
    {
        private readonly Socket _socket;
        private bool _isDisposed;

        private readonly List<IUnreliableChannel.SubscribeDelegate> _subscribers;

        private readonly Dictionary<IUserAddress, EndPoint> _connections;

        private readonly object _locker = new object();

        public Unreliable(Socket socket)
        {
            _socket = socket;
            _subscribers = new List<IUnreliableChannel.SubscribeDelegate>();
            _connections = new Dictionary<IUserAddress, EndPoint>();
        }

        public async void Start()
        {
            var buffer = new ArraySegment<byte>(new byte[64000]);

            try
            {
                var endPoint = new IPEndPoint(IPAddress.Any, 0);
                while (true)
                {
                    var res = await _socket.ReceiveMessageFromAsync(buffer, SocketFlags.None, endPoint);

                    var received = new byte[res.ReceivedBytes];
                    Array.Copy(buffer.Array, buffer.Offset, received, 0, res.ReceivedBytes);


                    var idLength = BitConverter.ToInt32(received[..sizeof(int)]);
                    var id = Encoding.UTF8.GetString(received[sizeof(int)..idLength]);
                    var data = received[idLength..];

                    IUserAddress address;
                    lock (_locker)
                    {
                        address = _connections.Keys.FirstOrDefault(address => address.UserId == id);
                        if (address == null)
                        {
                            address = new UserAddress(id);
                            _connections.Add(address, res.RemoteEndPoint);
                        }
                    }

                    var arguments = new MessageReceivedArguments(address, data);
                    var message = UnreliableChannelMessage.MessageReceived(arguments);
                    foreach (var subscriber in _subscribers)
                        await subscriber.Invoke(message);
                }
            }
            finally
            {
                await DisposeAsync();
            }
        }

        public async ValueTask DisposeAsync()
        {
            _isDisposed = true;
            if (_socket.Connected)
                _socket.Disconnect(false);
            _socket.Dispose();
            _subscribers.Clear();
        }

        public async ValueTask<UnreliableChannelSendResult> SendAsync(IUserAddress userAddress,
            ReadOnlyMemory<byte> message)
        {
            if (_isDisposed)
                return new UnreliableChannelSendResult(UnreliableChannelSendStatus.ChannelIsClosed, null);

            if (!_connections.ContainsKey(userAddress))
                return new UnreliableChannelSendResult(UnreliableChannelSendStatus.Unknown, $"Address not found");

            try
            {
                await _socket.SendToAsync(message.ToArray(), SocketFlags.None, _connections[userAddress]);

                return new UnreliableChannelSendResult(UnreliableChannelSendStatus.Ok, null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                return new UnreliableChannelSendResult(UnreliableChannelSendStatus.Unknown, e.ToString());
            }
        }

        public async ValueTask<IAsyncDisposable> SubscribeAsync(IUnreliableChannel.SubscribeDelegate subscriber)
        {
            _subscribers.Add(subscriber);

            return new AnonymousDisposable(async () => _subscribers.Remove(subscriber));
        }
    }
}