/**
 * VRGiO Cube 2.0 Firmware
 * Developed for the Basecubes based on Espressif ESP8266 (ESP-12F) Microcontroller.
 * This program is an overview and a check of the components and observe via Serial.
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
 * @version 2.1.4
 * @date 2022-06-05
 * 
 * @copyright Copyright (c) 2022
 */

#include <Arduino.h>
#include <CubeConfig.h>

/// Function definitions
void pad_touched(int side);
void pad_released(int side);
void printBat();

MotionTracker mpu;
TouchSensor touch(pad_touched, pad_released);

const int analogInPin = 17;   // A0
const int crg_pin     = 12;   // D6
const int stby_pin    = 14;   // D5



/// Arduino Framework setup routine
void setup(){
  Serial.begin(115200);
  boot(mpu, touch);  
}

///Arduino Framework loop routine
void loop(){
  touch.update();     //polling touch detection - needs to be called in loop()!
  
  touch.printData();  
  mpu.printMPU();
  printBat();
  }

/**
 * @brief Fires when a touchpin detects a new touch
 *  
 * @param side  is the index of the MPR121 pin, which was touched 
 */
void pad_touched(int side){
  //turn on touched side (Blue = Touchplate | Green = Neighbour-Detection)
  //Serial.println(side);
  leds[tMap[side] % 6] = tMap[side] < 6 ? (CRGB::Blue) : (CRGB::Green);
  FastLED.show();
}

/**
 * @brief Fires when a touchpin was released
 *  
 * @param side  is the index of the MPR121 pin, which was released 
 */
void pad_released(int side){
  //turn off LED on released side
  leds[tMap[side] % 6] = (CRGB::Black);
  FastLED.show();
}

/// Prints Batery related values
// TODO: Map value to voltage and percentage
long timer = 0;

void printBat(){
  if (millis() - timer > 1000){

    int sensorValue = analogRead(analogInPin);
    
    // print the readings in the Serial Monitor
    Serial.print("Battery = ");Serial.print(sensorValue);
    Serial.print(" | Charge: ");Serial.print(!digitalRead(crg_pin));
    Serial.print(" | Standby: ");Serial.println(!digitalRead(stby_pin));
    Serial.println();
    timer = millis();
	}
}
