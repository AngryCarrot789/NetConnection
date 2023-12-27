using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NetConnection {
    // Arduino/ATMEGA328P info:
    // baudrate: 9600 by default
    // 9600 bits/sec + startbit + stopbit = 10 bits per data byte = 960 bytes/sec
    // meaning, roughly 1.04 bytes per millisecond with a 16mhz clock.
    // This struct has to be as small as reasonably possible

    [StructLayout(LayoutKind.Sequential)]
    public struct MessageHeader {
        /// <summary>The maximum number of data bytes a message can send/receive</summary>
        // 1 = MSG(10) | 2 = ExData(10) | 3 = CbData(10)
        private const int BITS_MSG = 12;
        private const int BITS_EXD = 10;
        private const int BITS_CBD = 10;
        private const int BPOS_MSG = 0;
        private const int BPOS_EXD = BPOS_MSG + BITS_MSG;
        private const int BPOS_CBD = BPOS_EXD + BITS_EXD;
        private const uint MAX_MSG = (1U << BITS_MSG) - 1;
        private const uint MAX_EXD = (1U << BITS_EXD) - 1;
        private const  uint MAX_CBD = (1U << BITS_CBD) - 1;
        private const uint MASK_MSG = MAX_MSG << BPOS_MSG;
        private const uint MASK_EXD = MAX_EXD << BPOS_EXD;
        private const uint MASK_CBD = MAX_CBD << BPOS_CBD;
        private const uint MASK_MSG_INV = ~MASK_MSG;
        private const uint MASK_EXD_INV = ~MASK_EXD;
        private const uint MASK_CBD_INV = ~MASK_CBD;

        /// <summary>
        /// The maximum number of allowed data bytes
        /// </summary>
        public const uint MaxDataBytes = MAX_CBD + 1;

        public const int StructSize = sizeof(uint);

        /// <summary>
        /// The raw struct data
        /// </summary>
        public uint m_rawData;

        /// <summary>
        /// The message id
        /// </summary>
        public ushort MsgId {
            get => (ushort) ((this.m_rawData & MAX_MSG << BPOS_MSG) >> BPOS_MSG);
            set => this.m_rawData = (this.m_rawData & MASK_MSG_INV) | ((value & MAX_MSG) << BPOS_MSG);
        }

        /// <summary>
        /// Extra message header data
        /// </summary>
        public ushort ExtraData {
            get => (ushort) ((this.m_rawData & MAX_EXD << BPOS_EXD) >> BPOS_EXD);
            set => this.m_rawData = (this.m_rawData & MASK_EXD_INV) | ((value & MAX_EXD) << BPOS_EXD);
        }

        /// <summary>
        /// The number of data bytes in the full packet
        /// </summary>
        public ushort DataCount {
            get => (ushort) ((this.m_rawData & MAX_CBD << BPOS_CBD) >> BPOS_CBD);
            set => this.m_rawData = (this.m_rawData & MASK_CBD_INV) | ((value & MAX_CBD) << BPOS_CBD);
        }

        public MessageHeader(uint data) {
            this.m_rawData = data;
        }

        public MessageHeader(ushort msgId, ushort extraData, ushort cbData) {
            this.m_rawData = ((msgId & MAX_MSG) << BPOS_MSG & MASK_EXD_INV | (extraData & MAX_EXD) << BPOS_EXD) & MASK_CBD_INV | (cbData & MAX_CBD) << BPOS_CBD;
        }

        // The properties use an inlined version because why not...
        // might help performance if the JIT somehow doesn't inline these
        //
        // public uint MsgId {
        //     get => BitField.ReadBitRange(this.data, BPOS_MSG, BITS_MSG);
        //     set => BitField.WriteBitRange(ref this.data, BPOS_MSG, BITS_MSG, value);
        // }
        // public uint ExData {
        //     get => BitField.ReadBitRange(this.data, BPOS_EXD, BITS_EXD);
        //     set => BitField.WriteBitRange(ref this.data, BPOS_EXD, BITS_EXD, value);
        // }
        // public uint CbData {
        //     get => BitField.ReadBitRange(this.data, BPOS_CBD, BITS_CBD);
        //     set => BitField.WriteBitRange(ref this.data, BPOS_CBD, BITS_CBD, value);
        // }
    }
}