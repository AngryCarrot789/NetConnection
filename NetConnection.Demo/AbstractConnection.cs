using System;

namespace NetConnection.Demo {
    public abstract class AbstractConnection : IConnection {
        public MessageBus MessageBus { get; set; }

        public abstract bool IsConnected { get; }

        protected AbstractConnection() {
            this.MessageBus = new MessageBus();
        }

        ~AbstractConnection() {
            this.Dispose(false);
        }

        public abstract void Connect();

        public abstract void Disconnect();

        public abstract void WriteMessage(ushort msgId, ushort extraData, ushort cbData, IntPtr pData);

        public unsafe void WriteMessage<T>(ushort msgId, ushort extraData, T value) where T : unmanaged {
            if (sizeof(T) < 0 || sizeof(T) > MessageHeader.MaxDataBytes)
                throw new ArgumentException("Struct is too big");
            this.WriteMessage(msgId, extraData, (ushort) sizeof(T), new IntPtr(&value));
        }
        
        public abstract bool ReadMessageHeader(ref MessageHeader header);

        public abstract void ReadMemory(IntPtr ptr, uint cbData);

        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool isDisposing);
    }
}