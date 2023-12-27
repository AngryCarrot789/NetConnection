# NetConnection

A library for sending/receiving messages to/from embedded devices (using C/C++) and a host/controller deivce (using C#). 
The API is still WIP at the moment, but I hope to support more than just serial communications (sockets, etc.)

## Message header
Contains a message id (msgId), an extra field for anything really (exData/extraData) and the number of 
message data bytes (cbData). 

- MsgId  == 12 bits (1-4095, meaning 4094 possible message IDs. 0 reserved for invalid message id)
- ExData == 10 bits (0-1023)
- CbData == 10 bits (0-1023. data struct size limit is 1023 bytes)

This is to make the header as small as possible, since with a baud rate of 9600, the arduino can send data at
roughly 1 byte per millisecond, meaning the header packet takes ~4ms to send/read

Binary layout (little endian): M = msgId, E = exData, C = cbData
CCCC CCCC CCEE EEEE EEEE MMMM MMMM MMMM ... (data bytes after this)

## Arduino

Currently, serial data is read in the loop() method, so there's a possibility of a read buffer overflow (since it's 64 bytes on the arduino). 
Not sure how to overcome this, so make sure to occasionally process incomming messages in custom code
