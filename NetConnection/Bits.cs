namespace NetConnection {
    public static class BitField {
        public static uint ReadBitRange(uint data, int index, int count) {
            return (data & ((1U << count) - 1) << index) >> index;
        }

        public static void WriteBitRange(ref uint data, int index, int count, uint value) {
            data = (data & ~(((1U << count) - 1) << index)) | ((value & ((1u << count) - 1)) << index);
        }
    }
}