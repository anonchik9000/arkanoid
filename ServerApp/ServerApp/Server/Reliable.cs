﻿using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using Gaming.ContainerManager.ImageContracts.V1;
using Gaming.ContainerManager.ImageContracts.V1.Channels;
using XFlow.P2P;

namespace ServerApp.Server
{
    public class Reliable : IReliableChannel
    {
        private readonly Socket _socket;
        private bool _isDisposed;

        private readonly List<IReliableChannel.SubscribeDelegate> _subscribers;

        private readonly Dictionary<IUserAddress, Socket> _connections;
        private readonly Dictionary<IUserAddress, bool> _disposedConnections;

        public Reliable(Socket socket)
        {
            _socket = socket;
            _connections = new Dictionary<IUserAddress, Socket>();
            _disposedConnections = new Dictionary<IUserAddress, bool>();
            _subscribers = new List<IReliableChannel.SubscribeDelegate>();
        }

        public async ValueTask DisposeAsync()
        {
            _isDisposed = true;

            foreach (var kvp in _connections)
            {
                kvp.Value.Disconnect(false);
                kvp.Value.Dispose();
            }

            _socket.Dispose();
            _subscribers.Clear();
            _connections.Clear();
        }

        public async void Start()
        {
            try
            {
                while (true)
                {
                    var newSocket = await _socket.AcceptAsync();

                    var buffer = new ArraySegment<byte>(new byte[64000]);
                    var received = await newSocket.ReceiveAsync(buffer, SocketFlags.None);

                    Console.WriteLine($"Receive id packet {received}");
                    if (received != sizeof(long) + sizeof(int))
                    {
                        Console.WriteLine($"New connection receive wrong message");
                        continue;
                    }

                    var id = BitConverter.ToInt32(buffer[sizeof(long)..]);

                    var address = new UserAddress(id.ToString());
                    _connections.Add(address, newSocket);
                    _disposedConnections.Add(address, false);

                    await NotifySubscribers(ReliableChannelMessage.UserConnected(new UserConnectedArguments(address)));

                    StartReceiving(newSocket, address);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                await DisposeAsync();
            }
        }

        private async void StartReceiving(Socket socket, IUserAddress address)
        {
            try
            {
                const int packetSizeHeader = sizeof(long);

                var dataStream = new Queue<byte>();
                var rcvBuffer = new ArraySegment<byte>(new byte[64000]);

                var nextPacketSize = -1L;

                var messages = new List<byte[]>();

                while (true)
                {
                    var receivedLength = await socket.ReceiveAsync(rcvBuffer, SocketFlags.None);

                    if (receivedLength == 0)
                    {
                        await NotifySubscribers(
                            ReliableChannelMessage.UserDisconnected(new UserDisconnectedArguments(address)));
                        await NotifySubscribers(ReliableChannelMessage.ChannelClosed(new ChannelClosedArguments()));

                        socket.Dispose();
                        _disposedConnections[address] = true;
                        _connections.Remove(address);

                        break;
                    }

                    for (var i = rcvBuffer.Offset; i < receivedLength; i++)
                        dataStream.Enqueue(rcvBuffer[i]);

                    while (true)
                    {
                        if (nextPacketSize == -1)
                        {
                            if (dataStream.Count < packetSizeHeader)
                                break;

                            var packetSizeBuffer = new byte[packetSizeHeader];
                            for (var i = 0; i < packetSizeHeader; i++)
                                packetSizeBuffer[i] = dataStream.Dequeue();
                            nextPacketSize = BitConverter.ToInt64(packetSizeBuffer);
                        }

                        // already read packet size (packetSizeHeader)
                        if (nextPacketSize - packetSizeHeader > dataStream.Count)
                            break;

                        var message = new byte[nextPacketSize];
                        for (var i = 0; i < nextPacketSize - packetSizeHeader; i++)
                            message[i] = dataStream.Dequeue();
                        messages.Add(message);

                        nextPacketSize = -1;
                    }

                    foreach (var message in messages)
                    {
                        var arguments = new MessageReceivedArguments(address, message);
                        await NotifySubscribers(ReliableChannelMessage.MessageReceived(arguments));
                    }

                    messages.Clear();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async ValueTask<ReliableChannelSendResult> SendAsync(IUserAddress userAddress,
            ReadOnlyMemory<byte> message)
        {
            if (userAddress == null)
                return new ReliableChannelSendResult(ReliableChannelSendStatus.Unknown, "User address not specified");

            if (_isDisposed)
                return new ReliableChannelSendResult(ReliableChannelSendStatus.ChannelIsClosed, userAddress.ToString());

            try
            {
                var packet = P2P.PackMessage(message.ToArray());

                if (!_connections.ContainsKey(userAddress))
                    return new ReliableChannelSendResult(ReliableChannelSendStatus.ClientNotFound,
                        userAddress.ToString());

                if (_disposedConnections[userAddress])
                    return new ReliableChannelSendResult(ReliableChannelSendStatus.ChannelIsClosed,
                        userAddress.ToString());

                await _connections[userAddress].SendAsync(packet, SocketFlags.None);

                return new ReliableChannelSendResult(ReliableChannelSendStatus.Ok, null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                
                await DisposeAsync();

                return new ReliableChannelSendResult(ReliableChannelSendStatus.Unknown,
                    $"{userAddress}\n{e}");
            }
        }

        public async ValueTask<IAsyncDisposable> SubscribeAsync(IReliableChannel.SubscribeDelegate subscriber)
        {
            Console.WriteLine($"SubscribeAsync c={_subscribers.Count}");
            _subscribers.Add(subscriber);

            return new AnonymousDisposable(async () => _subscribers.Remove(subscriber));
        }

        private async Task NotifySubscribers(ReliableChannelMessage message)
        {
            Console.WriteLine($"Notify {message.Type} {_subscribers.Count}");
            for (var i = 0; i < _subscribers.Count; i++)
                await _subscribers[i].Invoke(message);
        }
    }
}