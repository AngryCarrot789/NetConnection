// Use arduino IDE/VSCODE+Arduino plugin to compile and upload this sketch

#include <stdint.h>
#include "MessagePump.h"

typedef struct {
  uint8_t ledId;
  uint8_t state;
} MSG_TOGGLE_LED;

typedef struct {
  uint8_t state;
} MSG_LED_STATUS_RESPONCE;

// Writes a message to the Serial serial port.
// Uses a pointer to MSG_HEADER to save stack space
void WriteMessage(MSG_HEADER* pMsg, char* pData);
void OnMessage(MSG_HEADER*);

void MessageHandler_ToggleLED(MSG_TOGGLE_LED* data); // MSGID = 1

void setup() {
  pinMode(13, OUTPUT);
  Serial.begin(9600);

  while (Serial.available()) { 
    Serial.read(); 
  }
}

// Serial.readBytes read the exact amout of requested bytes, blocking until all bytes are read
// Framework protocol:
// u32: msgId    -- message id
// u16: exData -- custom data
// u16: cbData     -- number of data bytes (may be 0)
// ...: the data bytes. there will be none to read if cbData is zero


void loop() {
  int count = Serial.available();
  if (count >= sizeof(MSG_HEADER)) {
    MSG_HEADER header;
    Serial.readBytes((char*)&header, sizeof(MSG_HEADER));
    if (header.msgId) { // invalid message id. WTF TO DO HERE???
      OnMessage(&header);
    }
    sizeof(MSG_HEADER);
  }
}

void OnMessage(MSG_HEADER* msg) {
  switch (msg->msgId)
  {
  case 1:
    MSG_TOGGLE_LED data;
    Serial.readBytes((char*) &data, min(sizeof(MSG_TOGGLE_LED), msg->cbData));
    MessageHandler_ToggleLED(&data);
    break;
  
  default:
    break;
  }
}

void MessageHandler_ToggleLED(MSG_TOGGLE_LED* data) {
  pinMode(data->ledId, OUTPUT);
  digitalWrite(data->ledId, data->state > 0 ? HIGH : LOW);

  MSG_HEADER header;
  header.msgId = 2;
  header.exData = 0;

  MSG_LED_STATUS_RESPONCE responce;
  responce.state = data->state;
  header.cbData = sizeof(MSG_LED_STATUS_RESPONCE);
  WriteMessage(&header, (char*) &responce);
}

void WriteMessage(MSG_HEADER* pMsg, char* pData) {
  Serial.write((char*) pMsg, sizeof(MSG_HEADER));
  size_t cbData = pMsg->cbData;
  if (cbData) {
    if (pData) {
      Serial.write(pData, cbData);
    }
    else { // tststs
      for (int i = 0; i < cbData; i++) {
        Serial.write((uint8_t) 0);
      }
    }
  }
}