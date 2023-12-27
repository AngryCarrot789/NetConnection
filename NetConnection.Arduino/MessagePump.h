#ifndef MESSAGE_PUMP_H
#define MESSAGE_PUMP_H

#include <stdint.h>

typedef struct {
  int msgId : 12;  // message ID (12 bits)
  int exData : 10; // additional data, e.g. message fragment index (10 bits)
  int cbData : 10; // number of data bytes to read (if any) (10 bits)
} MSG_HEADER;
static_assert(sizeof(MSG_HEADER) == 4ULL, "MSG_HEADER is not 4 bytes");

typedef void (*MessageCallback) (MSG_HEADER* d);

#endif // !MESSAGE_PUMP_H
