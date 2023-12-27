using System;
using System.Runtime.CompilerServices;

namespace NetConnection {
    /// <summary>
    /// A delegate for a message event handler function
    /// </summary>
    /// <param name="msg">The message header</param>
    /// <param name="dataBuffer">An array containing the message data. This array may be larger than cbData, and may be null when cbData is zero</param>
    public delegate void MessageEventHandler(MessageHeader msg, byte[] dataBuffer);

    /// <summary>
    /// A delegate for a generic struct message event handler function
    /// </summary>
    /// <param name="msg">The message header</param>
    /// <param name="data">
    /// The data, which is created unsafely via the received message data. May be default is no bytes were available
    /// </param>
    /// <typeparam name="T">The type of unmanaged struct</typeparam>
    public delegate void SpecialMessageEventHandler<in T>(MessageHeader msg, T data) where T : unmanaged;

    /// <summary>
    /// A class which stores registered message information
    /// </summary>
    public class FrameworkMessage {
        /// <summary>
        /// Gets the message ID
        /// </summary>
        public ushort Id { get; }

        /// <summary>
        /// Gets the direction that the packet is permitted to travel in
        /// </summary>
        public MsgDirection Direction { get; }

        /// <summary>
        /// Gets or sets a readable description for what this message is for
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// An event fired when a message is received with our specific ID
        /// </summary>
        public event MessageEventHandler OnMessage;

        public FrameworkMessage(ushort id, MsgDirection direction) {
            this.Direction = direction;
            this.Id = id;
        }

        /// <summary>
        /// Adds a handler to <see cref="OnMessage"/> that automatically creates an unmanaged struct instance from the data buffer
        /// </summary>
        /// <param name="handler"></param>
        /// <typeparam name="T"></typeparam>
        public unsafe void AddHandler<T>(SpecialMessageEventHandler<T> handler) where T : unmanaged {
            this.OnMessage += (msg, buffer) => {
                // when cbData is not large enough, that is a catastrophic bug such as a struct
                // size mismatch due to compiler issues maybe or outdated arduino code.
                // What should be done here though?
                // Unsafe.SizeOf<T>() > msg.DataCount
                if (buffer == null ||  msg.DataCount == 0) {
                    handler(msg, default);
                    return;
                }

                fixed (byte* pData = buffer) {
                    handler(msg, Unsafe.Read<T>(pData));
                }
            };
        }

        internal void OnMessageInternal(MessageHeader header, byte[] dataBuffer) {
            if (this.Direction != MsgDirection.Bidirectional && this.Direction != MsgDirection.FromClientToServer)
                throw new Exception("Invalid message direction");
            this.OnMessage?.Invoke(header, dataBuffer);
        }
    }
}