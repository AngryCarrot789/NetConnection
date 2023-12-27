namespace NetConnection.Demo {
    /// <summary>
    /// Used to stored queued messages and deliver them to appropriate handlers
    /// </summary>
    public class MessageBus {
        public MessageRegistry MessageRegistry { get; set; }

        public MessageBus() {

        }

        public MessageBus(MessageRegistry messageRegistry) {
            this.MessageRegistry = messageRegistry;
        }

        public void OnMessageReceived(MessageHeader header, byte[] data) {
            this.MessageRegistry?.OnMessageReceived(header, data);
        }
    }
}