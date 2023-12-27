namespace NetConnection {
    public enum MsgDirection {
        None = 0,     // message cannot be send or received; usually invalid message
        FromClientToServer = 1, // client sends to server only
        FromServerToClient = 2, // server sends to client only
        Bidirectional = 4 // well... bidirectional; client->server and server->client permitted
        // custom message data information may need to be used to handle bidirectional flows
    }
}