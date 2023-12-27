using System.Runtime.InteropServices;

namespace NetConnection.Demo {
    [StructLayout(LayoutKind.Sequential)]
    public struct MessagePacket {
        public MessageHeader header;
        public int readCount;
    }
}