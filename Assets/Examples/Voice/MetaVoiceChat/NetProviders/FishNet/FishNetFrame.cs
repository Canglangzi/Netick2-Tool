#if FISHNET
using FishNet.Serializing;
using System;
using UnityEngine;

namespace MetaVoiceChat.NetProviders.FishNet
{
    public readonly struct FishNetFrame
    {
        public readonly int index;
        public readonly double timestamp;
        public readonly float additionalLatency;
        public readonly ArraySegment<byte> data;

        public ushort Length => (ushort)data.Count;


        public FishNetFrame(int index, double timestamp, float additionalLatency, ArraySegment<byte> data)
        {
            this.index = index;
            this.timestamp = timestamp;
            this.additionalLatency = additionalLatency;
            this.data = data;
        }

        public FishNetFrame(int index, double timestamp, float additionalLatency)
        {
            this.index = index;
            this.timestamp = timestamp;
            this.additionalLatency = additionalLatency;
            data = ArraySegment<byte>.Empty;
        }
    }

    public static class FishNetFrameReaderWriter
    {
        // There is probably a better place to set this limitation
        private const float MaxAdditionalLatency = 0.2f;

        public static void WriteFishNetFrame(this Writer writer, FishNetFrame value)
        {
            writer.WriteInt32(value.index);
            writer.WriteDouble(value.timestamp);
            {
                float additionalLatency = value.additionalLatency;
                additionalLatency = Mathf.Clamp(additionalLatency, 0, MaxAdditionalLatency);
                float t = additionalLatency / MaxAdditionalLatency;
                writer.WriteUInt8Unpacked((byte)(t * byte.MaxValue));
            }
            writer.WriteUInt16(value.Length);
            if (value.Length != 0)
            {
                writer.WriteUInt8Array(value.data.Array, value.data.Offset, value.Length);
            }
        }

        public static FishNetFrame ReadFishNetFrame(this Reader reader)
        {
            int index = reader.ReadInt32();
            double timestamp = reader.ReadDouble();
            float additionalLatency;
            {
                float t = (float)reader.ReadUInt8Unpacked() / byte.MaxValue;
                additionalLatency = t * MaxAdditionalLatency;
            }
            ushort length = reader.ReadUInt16();
            if (length != 0)
            {
                var data = reader.ReadArraySegment(length);
                return new FishNetFrame(index, timestamp, additionalLatency, data);
            }
            else
            {
                return new FishNetFrame(index, timestamp, additionalLatency);
            }
        }
    }
}
#endif
