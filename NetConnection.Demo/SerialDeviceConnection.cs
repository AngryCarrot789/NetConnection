using System;
using System.IO;
using System.IO.Ports;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace NetConnection.Demo {
    public class SerialDeviceConnection : AbstractConnection {
        private readonly SerialPort port;
        private readonly byte[] txBuffer8;
        private readonly byte[] rxBuffer8;
        private readonly byte[] readBuffer;
        private BinaryWriter writer;
        private BinaryReader reader;
        private MessagePacket nextMsg;

        public override bool IsConnected => this.port.IsOpen;

        public SerialDeviceConnection(SerialPort port) {
            this.port = port ?? throw new ArgumentNullException(nameof(port));
            this.txBuffer8 = new byte[8];
            this.rxBuffer8 = new byte[8];
            this.readBuffer = new byte[MessageHeader.MaxDataBytes];
        }

        public override void Connect() {
            if (this.IsConnected)
                return;
            this.port.Open();
            this.reader = new BinaryReader(this.port.BaseStream);
            this.writer = new BinaryWriter(this.port.BaseStream);
        }

        public override void Disconnect() {
            if (!this.IsConnected)
                return;
            try {
                this.port.Close();
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
            finally {
                this.writer?.Close();
                this.writer = null;
                this.reader?.Close();
                this.reader = null;
            }
        }

        public override void WriteMessage(ushort msgId, ushort extraData, ushort cbData, IntPtr pData) {
            if (cbData > MessageHeader.MaxDataBytes)
                throw new Exception($"Too much data to send (limit: {MessageHeader.MaxDataBytes}). Send full message as fragments");
            this.writer.Write(new MessageHeader(msgId, extraData, cbData).m_rawData);
            this.WritePtr(pData, cbData);
        }

        public override bool ReadMessageHeader(ref MessageHeader header) {
            if (this.port.BytesToRead < MessageHeader.StructSize) {
                return false;
            }

            header.m_rawData = this.reader.ReadUInt32();
            return true;
        }

        public override void ReadMemory(IntPtr ptr, uint cbData) {
            int total = (int) cbData;
            byte[] tempBuf = this.rxBuffer8;
            int tempSize = tempBuf.Length;
            while (total > 0) {
                int count;
                try {
                    count = this.port.Read(tempBuf, 0, Math.Min(tempSize, total));
                }
                catch (TimeoutException) {
                    count = 0;
                }

                if (count > 0) {
                    Marshal.Copy(tempBuf, 0, ptr, count);
                    ptr += count;
                    total -= count;
                }
                else {
                    // allow some data to be buffered in the meantime
                    Thread.Sleep(1);
                }
            }
        }

        // blocking read
        public unsafe bool ReadMessage<T>(ref MessageHeader header, out T value) where T : unmanaged {
            if (!this.ReadMessageHeader(ref header)) {
                value = default;
                return false;
            }

            T val;
            int size = sizeof(T);
            if (size != header.DataCount) {
                this.ReadMemory(new IntPtr(&val), (ushort) Math.Min(header.DataCount, size));
            }
            else {
                this.ReadMemory(new IntPtr(&val), header.DataCount);
            }

            value = val;
            return true;
        }

        public void WritePtr(IntPtr data, int count) {
            byte[] tempBuf = this.txBuffer8;
            int tempSize = tempBuf.Length;
            while (count > tempSize) {
                Marshal.Copy(data, tempBuf, 0, tempSize);
                this.port.Write(tempBuf, 0, tempSize);
                count -= tempSize;
            }

            if (count > 0) {
                int numBytes = Math.Min(tempSize, count);
                Marshal.Copy(data, tempBuf, 0, numBytes);
                this.port.Write(tempBuf, 0, numBytes);
            }
        }

        protected override void Dispose(bool isDisposing) {
            this.port?.Dispose();
            this.writer?.Dispose();
        }

        /// <summary>
        /// Tries to read the next available message and sends it to our associated <see cref="MessageBus"/>, if any
        /// </summary>
        /// <returns>True when a message header or a remaining message body was read</returns>
        /// <exception cref="Exception">Packet is too large</exception>
        public bool ReadNextMessage() {
            if (!this.port.IsOpen) {
                return false;
            }

            bool hasReadData = false;
            MessageHeader header = this.nextMsg.header;
            if (header.MsgId != 0) {
                if (!this.ReadPacketData(ref this.nextMsg)) {
                    return true;
                }

                this.MessageBus?.OnMessageReceived(header, this.readBuffer);
                this.nextMsg = default;
                hasReadData = true;
            }

            while (this.ReadMessageHeader(ref header)) {
                hasReadData = true;
                MessagePacket packet = new MessagePacket() {
                    header = header, readCount = 0
                };

                if (!this.ReadPacketData(ref packet)) {
                    this.nextMsg = packet;
                    return true;
                }

                this.MessageBus?.OnMessageReceived(header, this.readBuffer);
            }

            return hasReadData;
        }

        private bool ReadPacketData(ref MessagePacket pkt) {
            byte[] data = this.readBuffer;
            int request = Math.Min((int) pkt.header.DataCount - pkt.readCount, this.port.BytesToRead);
            pkt.readCount += this.port.Read(data, 0, request);
            if (pkt.readCount > pkt.header.DataCount)
                throw new Exception("Read more bytes that necessary! This no good");
            return pkt.readCount == pkt.header.DataCount;
        }
    }
}