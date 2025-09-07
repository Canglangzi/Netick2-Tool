
using System;
using System.Collections.Generic;
using MetaVoiceChat.Utils;
using Netick;
using Netick.Unity;
using UnityEngine;

namespace MetaVoiceChat.NetProviders.Netick
{
    [RequireComponent(typeof(MetaVc))]
    public class NetickNetProvider : NetworkBehaviour, INetProvider
    {
        #region Singleton
        public static NetickNetProvider LocalPlayerInstance { get; private set; }
        private readonly static List<NetickNetProvider> instances = new();
        public static IReadOnlyList<NetickNetProvider> Instances => instances;
        #endregion

        bool INetProvider.IsLocalPlayerDeafened => LocalPlayerInstance?.MetaVc?.isDeafened ?? false;

        public MetaVc MetaVc { get; private set; }

        private const byte VoiceDataId = 5; // 自定义数据ID

        public override unsafe void NetworkStart()
        {
            #region Singleton
            if (IsInputSource)
            {
                LocalPlayerInstance = this;
            }

            instances.Add(this);
            #endregion

            static int GetMaxDataBytesPerPacket(NetworkSandbox sandbox)
            {
                // Netick没有直接提供MTU查询，使用保守估计值
                // 通常UDP MTU为1500字节，减去IP和UDP头部的28字节，再减去Netick的头部开销
                int bytes = 1200 - 28 - 13; // 保守估计
                bytes -= sizeof(int); // Index
                bytes -= sizeof(double); // Timestamp
                bytes -= sizeof(byte); // Additional latency
                bytes -= sizeof(ushort); // Array length
                return bytes;
            }

            MetaVc = GetComponent<MetaVc>();
            MetaVc.StartClient(this,IsInputSource, GetMaxDataBytesPerPacket(Sandbox));
            // 注册数据接收事件
            Sandbox.Events.OnDataReceived += OnDataReceived;
        }

        public override unsafe void NetworkDestroy()
        {
            #region Singleton
            if (IsInputSource)
            {
                LocalPlayerInstance = null;
            }

            instances.Remove(this);
            #endregion

            Sandbox.Events.OnDataReceived -= OnDataReceived;
            MetaVc.StopClient();
        }

        void INetProvider.RelayFrame(int index, double timestamp, ReadOnlySpan<byte> data)
        {
            var array = FixedLengthArrayPool<byte>.Rent(data.Length);
            data.CopyTo(array);

            float additionalLatency = Time.deltaTime;
            NetickFrame frame = new(index, timestamp, additionalLatency, array);

            if (Sandbox.IsServer)
            {
                // 服务器直接处理并转发
                ProcessAndRelayFrame(frame, null);
            }
            else
            {
                // 客户端发送到服务器
                SendFrameToServer(frame);
            }

            FixedLengthArrayPool<byte>.Return(array);
        }

        private void SendFrameToServer(NetickFrame frame)
        {
            byte[] serialized = SerializeFrame(frame);
            Sandbox.ConnectedServer.SendData(VoiceDataId, serialized, serialized.Length, TransportDeliveryMethod.Unreliable);
        }

        private unsafe void OnDataReceived(NetworkSandbox sandbox, NetworkConnection sender, byte id, byte* data, int len, TransportDeliveryMethod transportDeliveryMethod)
        {
            if (id != VoiceDataId) return;

            // 将指针数据复制到托管数组
            byte[] buffer = new byte[len];
            for (int i = 0; i < len; i++)
                buffer[i] = data[i];

            NetickFrame frame = DeserializeFrame(buffer);
            if (Sandbox.IsServer)
            {
                // 服务器收到后处理并转发给其他客户端
                ProcessAndRelayFrame(frame, sender);
            }
            else
            {
                // 客户端直接处理接收到的帧
                MetaVc.ReceiveFrame(frame.index, frame.timestamp, frame.additionalLatency, frame.data.Array);
            }
        }

        private void ProcessAndRelayFrame(NetickFrame frame, NetworkConnection excludeSender)
        {
            if (Sandbox.IsServer)
            {
                // 添加服务器处理延迟
                frame = new NetickFrame(frame.index, frame.timestamp, frame.additionalLatency + Time.deltaTime, frame.data);
                // 转发给所有客户端（排除发送者）
                byte[] serialized = SerializeFrame(frame);
                foreach (var client in Sandbox.ConnectedClients)
                {
                    if (client == excludeSender) continue;
                    client.SendData(VoiceDataId, serialized, serialized.Length, TransportDeliveryMethod.Unreliable);
                }
                // 服务器本地也处理这个帧
                MetaVc.ReceiveFrame(frame.index, frame.timestamp, frame.additionalLatency, frame.data.Array);
            }
        }

        private byte[] SerializeFrame(NetickFrame frame)
        {
            // 序列化帧数据
            int totalSize = sizeof(int) + sizeof(double) + sizeof(float) + sizeof(ushort) + frame.data.Count;
            byte[] buffer = new byte[totalSize];
            int offset = 0;
            Buffer.BlockCopy(BitConverter.GetBytes(frame.index), 0, buffer, offset, sizeof(int));
            offset += sizeof(int);
            Buffer.BlockCopy(BitConverter.GetBytes(frame.timestamp), 0, buffer, offset, sizeof(double));
            offset += sizeof(double);
            Buffer.BlockCopy(BitConverter.GetBytes(frame.additionalLatency), 0, buffer, offset, sizeof(float));
            offset += sizeof(float);
            Buffer.BlockCopy(BitConverter.GetBytes((ushort)frame.data.Count), 0, buffer, offset, sizeof(ushort));
            offset += sizeof(ushort);
            Buffer.BlockCopy(frame.data.Array, frame.data.Offset, buffer, offset, frame.data.Count);
            return buffer;
        }

        private NetickFrame DeserializeFrame(byte[] data)
        {
            int offset = 0;
            int index = BitConverter.ToInt32(data, offset);
            offset += sizeof(int);
            double timestamp = BitConverter.ToDouble(data, offset);
            offset += sizeof(double);
            float additionalLatency = BitConverter.ToSingle(data, offset);
            offset += sizeof(float);
            ushort length = BitConverter.ToUInt16(data, offset);
            offset += sizeof(ushort);
            byte[] frameData = new byte[length];
            Buffer.BlockCopy(data, offset, frameData, 0, length);
            return new NetickFrame(index, timestamp, additionalLatency, new ArraySegment<byte>(frameData));
        }
    }

    public readonly struct NetickFrame
    {
        public readonly int index;
        public readonly double timestamp;
        public readonly float additionalLatency;
        public readonly ArraySegment<byte> data;

        public NetickFrame(int index, double timestamp, float additionalLatency, ArraySegment<byte> data)
        {
            this.index = index;
            this.timestamp = timestamp;
            this.additionalLatency = additionalLatency;
            this.data = data;
        }

        public NetickFrame(int index, double timestamp, float additionalLatency, byte[] data)
            : this(index, timestamp, additionalLatency, new ArraySegment<byte>(data))
        {
        }
    }
}
