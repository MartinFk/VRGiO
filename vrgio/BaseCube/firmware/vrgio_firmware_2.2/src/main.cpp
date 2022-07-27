/**
 * VRGiO Cube 2.0 Firmware
 * Developed for the Basecubes based on Espressif ESP8266 (ESP-12F) Microcontroller.
 * This program establishes a connection to another ESP8266 which acts as a bridge/basestation using the
 * ESPNOW protocol.
 * 
 * It sends:
 * - Quaternion values of the current rotation
 * - the battery voltage
 * - touch / release events
 * 
 * It receives:
 * - changing LED color
 * - changing LED brightness
 * 
 *  
 * I2C  : 0x68 - MPU6050 Gyro/Accelerometer IMU
 *        0x5A - MPR121 Capacitive Touch Controller 
 * 
 * GPIO :   12 - TP4056 Charge-Status (inverted)
 *          13 - Up to 6 addressable Neopixel LEDs
 *          14 - TP4056 Standby-Status (inverted)
 *          17 - Analog Vbat Input via 100K|330K voltage divider (1024 -> 4.2V)
 * 
 * @author Cihan Biyikli (cihan.biyikli@dfki.de)
 * @version 2.2.1
 * @date 2022-07-27
 * 
 * @copyright Copyright (c) 2022
 */


#include <Arduino.h>
#include <ESP8266WiFi.h>
#include <LEDControl.h>
#include <MotionControl.h>
#include <TouchControl.h>
#include <Wire.h>
#include <espnow.h>
#include <structs.h>

void statCheck(uint8_t status);
void touched(uint8_t side);
void released(uint8_t side);
void sendBattery();
void sendQuat();
void fillColor(CRGB color);

void OnDataSent(uint8_t *mac_addr, uint8_t sendStatus);
void OnDataRecv(uint8_t *mac, uint8_t *incomingData, uint8_t len);
uint8_t espNowSetup();

uint8_t broadcastAddress[] = {0x84, 0x0D, 0x8E, 0xB0, 0x61, 0x9B};  // ESP_8266 NODEMCU_V3

float getBatteryVoltage();

TouchSensor touch(touched, released);
MotionControl mpu;

struct_val s_val;
struct_quat s_quat;

long t_quat = 0;
long t_bat = 0;

//                  0           1           2           3             4 5 6
CRGB cDict[] = {CRGB::Red,    CRGB::Blue,       CRGB::Green, CRGB::White,
                CRGB::Yellow, CRGB::DarkViolet, CRGB::Black};

//##################################################### SETUP ##################################################
void setup() {
  Serial.begin(115200);
  uint8_t status = 0;
  Serial.println("\n\n|---------[  VRGiO v2.2  ]---------\n|");

  // ++++ WS2812B LED init
  ledSetup();
  fillColor(CRGB::DarkOrange);
  FastLED.show();
  Serial.println("|- LED      OK?");

  delay(1000);
  // ++++ MPU6050 IMU init
  Serial.print("|- MOTION   ");
  status += mpu.setup();
  statCheck(status);
 	Serial.print("|- ACTIVE OFFSETS: "); mpu.printOffsets();

  delay(100);

  // ++++ MPR121 Touch IMU init
  status += touch.setup();
  Serial.print("|- TOUCH    ");
  statCheck(status);

  delay(100);

  // ++++ ESPNOW init
  status += espNowSetup();
  Serial.print("|- WiFi     ");
  statCheck(status);

  delay(100);

  Serial.print("|\n|---------[  ");
  if (status == 0) {
    Serial.println("INIT PASSED  ]---------\n");
  } else {
    Serial.println("INIT FAILED  ]------\n");
  }

  fillColor(CRGB::Black);
  leds[29] = CRGB::Red;
  FastLED.show();
  delay(100);
}

//##################################################### LOOP
//##################################################

void loop() {
  touch.update();
  mpu.update();
  sendQuat();
  sendBattery();
  delay(4);
}

void touched(uint8_t side) {
  s_val.id = 't';
  s_val.data = side;
  esp_now_send(broadcastAddress, (uint8_t *)&s_val, sizeof(s_val));
}

void released(uint8_t side) {
  s_val.id = 'r';
  s_val.data = side;
  esp_now_send(broadcastAddress, (uint8_t *)&s_val, sizeof(s_val));
}

uint8_t espNowSetup() {
  WiFi.mode(WIFI_STA);
  if (esp_now_init() != 0) {
    Serial.println("Error initializing ESP-NOW");
    return 1;
  }

  // Serial.println(WiFi.macAddress());
  esp_now_set_self_role(ESP_NOW_ROLE_COMBO);
  esp_now_register_send_cb(OnDataSent);
  esp_now_add_peer(broadcastAddress, ESP_NOW_ROLE_COMBO, 1, NULL, 0);
  esp_now_register_recv_cb(OnDataRecv);

  return 0;
}

// Callback when data is sent
void OnDataSent(uint8_t *mac_addr, uint8_t sendStatus) {
  if (sendStatus != 0) {
    Serial.println("[FAIL]");
  }
}

void OnDataRecv(uint8_t *mac, uint8_t *incomingData, uint8_t len) {
  struct_cmd s_cmd;

  memcpy(&s_cmd, incomingData, sizeof(s_val));

  switch (s_cmd.cmd) {
    case 'l':
      leds[(uint8_t)s_cmd.val] = cDict[(uint8_t)s_cmd.opt];
      break;

    case 'F':
      fillColor(cDict[s_cmd.val]);
      break;

    case 'b':
      FastLED.setBrightness(s_cmd.val);
      FastLED.show();
      break;

    default:
      break;
  }

  Serial.print(s_cmd.cmd);
  Serial.print(" ");
  Serial.print((uint8_t)s_cmd.val);
  Serial.print(" ");
  Serial.print((uint8_t)s_cmd.opt);
  Serial.println();
}

void statCheck(uint8_t status) {
  if (status == 0) {
    Serial.println("OK!");
  } else {
    Serial.println(status);
  }
}

//++++++++++++++++++ BATTERY +++++++++++++++++++++

float getBatteryVoltage() {
  // 220K + 100K Volatge Divider
  // Fully Charged Voltage / 1024 = 0.00395 (approx.)
  return (analogRead(A0) * 0.003955f);
}

void sendBattery() {
  if (millis() - t_bat > 5000) {
    s_val.id = 'b';
    s_val.data = getBatteryVoltage() * 100 - 200;
    // Serial.println(s_val.data);

    esp_now_send(broadcastAddress, (uint8_t *)&s_val, sizeof(s_val));
    t_bat = millis();
  }
}

//++++++++++++++++++ MOTION +++++++++++++++++++++

void sendQuat() {
  if (millis() - t_quat > 50) {
    Quaternion q = mpu.getQuaternion();
    s_quat.w = q.w;
    s_quat.x = q.x;
    s_quat.y = q.y;
    s_quat.z = q.z;
    // Serial.println(String(s_quat.w) + " " + String(s_quat.x)  + " " +
    // String(s_quat.y)  + " " + String(s_quat.z));
    esp_now_send(broadcastAddress, (uint8_t *)&s_quat, sizeof(s_quat));
    t_quat = millis();
  }
}

//++++++++++++++++++ LED HELPER +++++++++++++++++++++

void fillColor(CRGB color) {
  FastLED.clear();
  for (int i = 0; i < NUM_LEDS; i++) {
    leds[i] = color;
  }
  FastLED.show();
}
