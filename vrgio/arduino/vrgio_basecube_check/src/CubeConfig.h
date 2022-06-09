#include <Arduino.h>
#include <Adafruit_Sensor.h>
#include <Wire.h>
#include <LEDConfig.h>
#include <TouchConfig.h>
#include <MotionConfig.h>


int boot(MotionTracker &mpu, TouchSensor &touch){
	int exitcode = 0;

	if(!Serial.available()){
		Serial.begin(115200);
	}
	
	Serial.println("\n\n|------[ VRGiO CUBE v2.0 ]------\n|");
	int status;
  	// ++++ MPU6050 IMU init
	status = mpu.setup();
    Serial.print("|- MPU6050   ");
	if(status == 1){
		Serial.println("OK!");
	}else{
		Serial.println(status);
	}
  	delay(100);

  	// ++++ MPR121 Touch IMU init
  	status = touch.setup();
  	Serial.print("|- MPR121    ");
	if(status == 1){
		Serial.println("OK!");
	}else{
		Serial.println(status);
	}

  	delay(100);

  	// ++++ WS2812B LED init
  	FastLED.addLeds<NEOPIXEL, DATA_PIN>(leds, NUM_LEDS);
	rainbowTest();
  	Serial.println("|- NEOPIXEL  OK!");
	FastLED.clear();
	FastLED.show();
  	delay(100);


	Serial.println("|\n|------[   INIT PASSED   ]------\n");

	return 1;
}