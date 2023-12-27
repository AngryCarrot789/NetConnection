using System;

namespace NetConnection.Demo {
    /// <summary>
    /// An interface for a connection between framework devices
    /// </summary>
    public interface IConnection : IDisposable {
        /// <summary>
        /// A bus used to manage the transfer of messages
        /// </summary>
        MessageBus MessageBus { get; set; }

        /// <summary>
        /// Returns the connection status
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Connects to the target device
        /// </summary>
        void Connect();

        /// <summary>
        /// Disconnects from the target device
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Posts a message to the device, and writes the message data (if any)
        /// </summary>
        /// <param name="msgId">The message ID</param>
        /// <param name="extraData">The extra data for this message</param>
        /// <param name="cbData">The number of data bytes to read (may be zero)</param>
        /// <param name="pData">A pointer to the message data, if any. May be NULL if cbData is zero</param>
        void WriteMessage(ushort msgId, ushort extraData, ushort cbData, IntPtr pData);

        /// <summary>
        /// Posts a message to the device, using an unmanaged struct as the message data
        /// </summary>
        /// <param name="msgId">The message ID</param>
        /// <param name="extraData">The extra data for this message</param>
        /// <param name="value">The value to write</param>
        /// <typeparam name="T">The type of unmanaged value to write (e.g. int, long, DateTime, etc.)</typeparam>
        void WriteMessage<T>(ushort msgId, ushort extraData, T value) where T : unmanaged;

        /// <summary>
        /// Tries to read the next message header from this connection
        /// </summary>
        /// <param name="header">A reference to a message header that may be modified by this method</param>
        /// <returns>True when a header was read, false when there were not enough bytes to read from the connection</returns>

        bool ReadMessageHeader(ref MessageHeader header);

        /// <summary>
        /// Reads the given number of bytes into the given pointer. This is a blocking operation
        /// </summary>
        /// <param name="ptr">The destination pointer</param>
        /// <param name="cbData">The number of bytes to read</param>
        void ReadMemory(IntPtr ptr, uint cbData);
    }
}