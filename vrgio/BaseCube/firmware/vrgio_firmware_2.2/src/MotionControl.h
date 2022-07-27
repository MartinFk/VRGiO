#ifndef MOTIONCONTROL_H
#define MOTIONCONTROL_H

#include <MPU6050_6Axis_MotionApps20.h>
#include <structs.h>

class MotionControl{

	public:
	MotionControl();
	Quaternion getQuaternion();
	uint8_t setup();
	void update();
	void printQuat();
	void printOffsets();

	private:
	MPU6050 mpu;

	Quaternion 	q;					// [w, x, y, z]         quaternion container

	bool 		dmpReady = false;  	// set true if DMP init was successful
	uint8_t 	devStatus;    		// return status after each device operation (0 = success, !0 = error)
	uint16_t 	packetSize;  		// expected DMP packet size (default is 42 bytes)
	uint16_t 	fifoCount;   		// count of all bytes currently in FIFO
	uint8_t 	fifoBuffer[64];  	// FIFO storage buffer

	unsigned long tPrintData = 0; 	// Timer variable

};

MotionControl::MotionControl(){
	mpu = MPU6050();
}

uint8_t MotionControl::setup(){
	Wire.begin();
	Wire.setClock(400000); 

  	mpu.initialize();
  	devStatus = mpu.dmpInitialize();	

  	// mpu.setXAccelOffset(-232);
  	// mpu.setYAccelOffset(-261);
  
  	// mpu.setXGyroOffset(-23);
  	// mpu.setYGyroOffset(-92);
  	// mpu.setZGyroOffset(-7);

	// mpu.setZAccelOffset(920);

	mpu.CalibrateAccel(6);
	mpu.CalibrateGyro(6);

  	// make sure it worked (returns 0 if so)
  	if (devStatus == 0) {
    	mpu.setDMPEnabled(true);
    	dmpReady = true;
    	packetSize = mpu.dmpGetFIFOPacketSize();
  	} else {
    	// ERROR!
    	// 1 = initial memory load failed
    	// 2 = DMP configuration updates failed
    	// (if it's going to break, usually the code will be 1)
    Serial.print(F("DMP Initialization failed (code "));
    Serial.print(devStatus);
    Serial.println(F(")"));
  }
  	return devStatus;

}

void MotionControl::update(){
  if (!dmpReady) return;
  
  int mpuIntStatus = mpu.getIntStatus();
  fifoCount = mpu.getFIFOCount();

  if ((mpuIntStatus & 0x10) || fifoCount == 1024) {
    mpu.resetFIFO();
  } else if (mpuIntStatus & 0x02) {
    while (fifoCount < packetSize) fifoCount = mpu.getFIFOCount();
    mpu.getFIFOBytes(fifoBuffer, packetSize);
    fifoCount -= packetSize;
  }

}


void MotionControl::printQuat(){
		/// Prints All MPU6050 related values every 1000ms

		if (millis() - tPrintData > 200){
			mpu.dmpGetQuaternion(&q, fifoBuffer);
			Serial.print("QUATERNION: ( ");
			Serial.print(q.w);
			Serial.print(" , ");
			Serial.print(q.x);
			Serial.print(" , ");
			Serial.print(q.y);
			Serial.print(" , ");
			Serial.print(q.z);
			Serial.println(" )");

			tPrintData = millis();
		}
}

void MotionControl::printOffsets(){
	mpu.PrintActiveOffsets();
}

Quaternion MotionControl::getQuaternion(){
	mpu.dmpGetQuaternion(&q, fifoBuffer);
	return q;	
}



#endif