using System;
using System.Collections.Generic;

namespace NetConnection {
    /// <summary>
    /// An object that stores a collection of registered messages and their associated handler objects
    /// </summary>
    public class MessageRegistry {
        private readonly SortedList<ushort, FrameworkMessage> msgInfo;

        public MessageRegistry() {
            this.msgInfo = new SortedList<ushort, FrameworkMessage>();
        }

        /// <summary>
        /// Creates a message association for the given msg id, so that it may be handled
        /// </summary>
        /// <param name="msgId"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public FrameworkMessage RegisterMessage(ushort msgId, MsgDirection direction = MsgDirection.Bidirectional, string description = "No message desc") {
            if (msgId == 0)
                throw new Exception("Maximum number of messages registered. This shouldn't even be possible realistically");
            if (this.msgInfo.ContainsKey(msgId))
                throw new InvalidOperationException("Message already exists: " + msgId);
            FrameworkMessage msg = new FrameworkMessage(msgId, direction);
            if (description != null)
                msg.Description = description;
            this.msgInfo[msgId] = msg;
            return msg;
        }

        /// <summary>
        /// Invokes the message handler(s) for the given message using the given data
        /// </summary>
        /// <param name="msg">The message id</param>
        /// <param name="uParam">An extra piece of data in the message header</param>
        /// <param name="cbData">The number of bytes available to read from pData</param>
        /// <param name="pData">A pointer to a block of memory which contains the message data. May be NULL when cbData is zero</param>
        public void OnMessageReceived(MessageHeader header, byte[] data) {
            if (this.msgInfo.TryGetValue(header.MsgId, out FrameworkMessage msg))
                msg.OnMessageInternal(header, data);
        }
    }
}