using System;

namespace Platibus.Multicast
{
    internal class ByteUtils
    {
        public static void WriteInt64(long num, byte[] dest, int start)
        {
            var numBytes = BitConverter.GetBytes(num);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(numBytes);
            }
            Array.Copy(numBytes, 0, dest, start, numBytes.Length);
        }

        public static void WriteInt32(int num, byte[] dest, int start)
        {
            var numBytes = BitConverter.GetBytes(num);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(numBytes);
            }
            Array.Copy(numBytes, 0, dest, start, numBytes.Length);
        }

        public static void WriteInt16(short num, byte[] dest, int start)
        {
            var numBytes = BitConverter.GetBytes(num);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(numBytes);
            }
            Array.Copy(numBytes, 0, dest, start, numBytes.Length);
        }

        public static long ReadInt64(byte[] src, int start)
        {
            var hostOrderBytes = new byte[sizeof(long)];
            Array.Copy(src, start, hostOrderBytes, 0, hostOrderBytes.Length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(hostOrderBytes);
            }
            return BitConverter.ToInt64(hostOrderBytes, 0);
        }

        public static int ReadInt32(byte[] src, int start)
        {
            var hostOrderBytes = new byte[sizeof(int)];
            Array.Copy(src, start, hostOrderBytes, 0, hostOrderBytes.Length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(hostOrderBytes);
            }
            return BitConverter.ToInt32(hostOrderBytes, 0);
        }

        public static short ReadInt16(byte[] src, int start)
        {
            var hostOrderBytes = new byte[sizeof(int)];
            Array.Copy(src, start, hostOrderBytes, 0, hostOrderBytes.Length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(hostOrderBytes);
            }
            return BitConverter.ToInt16(hostOrderBytes, 0);
        }

    }
}
