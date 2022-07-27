#ifndef TOUCHCONTROL_H
#define TOUCHCONTROL_H

#include <Adafruit_MPR121.h>

#ifndef _BV
#define _BV(bit) (1 << (bit))
#endif

typedef void (*touch_callback)(uint8_t);

class TouchSensor {
 public:
  TouchSensor(touch_callback touched, touch_callback released) {
    cap = Adafruit_MPR121();
    t_callback = touched;
    r_callback = released;
    lasttouched = 0;
  }

  uint8_t setup() {
    status = 0;
    if (!cap.begin(0x5A)) {  // Error: MPR121 not found
      status = 1;
    }
    return status;
  }

  void update() {
    int currtouched = cap.touched();
    for (int i = 0; i < 12; i++) {
      // if it *is* touched and *was not* touched before -> touched!
      if (is_touched(currtouched, i) && !is_touched(lasttouched, i)) {
        t_callback(i);
      }
      // if it *was* touched and now *isnt* -> released!
      if (!is_touched(currtouched, i) && is_touched(lasttouched, i)) {
        r_callback(i);
      }
    }

    lasttouched = currtouched;
  }

  void getData(int (&data)[12]) {
    for (int i = 0; i < 12; i++) {
      data[i] = cap.filteredData(i);
    }
  }

  int available() { return status; }

  /// Prints All MPR121 related values every 5000ms
  void printData() {
    if (millis() - tPrintData > 500) {
      int tData[12] = {};
      getData(tData);
      for (int i = 0; i < 12; i++) {
        Serial.print(i);
        Serial.print(": ");
        Serial.print(tData[i]);
        Serial.print(" ");
      }
      Serial.println();
      tPrintData = millis();
    }
  }

 private:
  Adafruit_MPR121 cap;
  touch_callback t_callback;
  touch_callback r_callback;
  int lasttouched;
  uint8_t status = 0;  // Status Codes: 0 = not init ; 1 = running ; -1 = error, not fonud
  unsigned long tPrintData = 0;  // Timer variable

  bool is_touched(int touched, int cap) { return touched & _BV(cap); }
};

#endif