#ifndef MOTIONCONTROL_H
#define MOTIONCONTROL_H


#include <MPU6050_6Axis_MotionApps20.h>
#include <structs.h>


class MotionControl
{

	public:
	MotionControl();
	Quaternion getQuaternion();
	VectorFloat getEuler();
	VectorFloat getYPR();
	uint8_t setup();
	void update();
	void printQuat();

	VectorInt16 getGyro();
	void resetAccel();

	private:
	MPU6050 mpu;


	// orientation/motion vars
	Quaternion 	q;					// [w, x, y, z]         quaternion container
	VectorInt16 aa;					// [x, y, z]            accel sensor measurements
	VectorInt16	aaReal;				// [x, y, z]            gravity-free accel sensor measurements
	VectorInt16 aaWorld;			// [x, y, z]            world-frame accel sensor measurements
	VectorFloat gravity;			// [x, y, z]            gravity vector

	bool 		dmpReady = false;  	// set true if DMP init was successful
	uint8_t 	devStatus;    		// return status after each device operation (0 = success, !0 = error)
	uint16_t 	packetSize;  		// expected DMP packet size (default is 42 bytes)
	uint16_t 	fifoCount;   		// count of all bytes currently in FIFO
	uint8_t 	fifoBuffer[64];  	// FIFO storage buffer
	float 		euler[3];  			// [psi, theta, phi]    Euler angle container
	float 		ypr[3];  			// [yaw, pitch, roll]   yaw/pitch/roll container and gravity vector

	unsigned long tPrintData = 0; 	// Timer variable

	VectorInt16 s_rot;


};

MotionControl::MotionControl(){
	mpu = MPU6050();
}

uint8_t MotionControl::setup(){
	Wire.begin();
	Wire.setClock(400000);  		// 400kHz I2C clock. Comment this line if having compilation difficulties

  	mpu.initialize();
  	devStatus = mpu.dmpInitialize();

  	// mpu.setXAccelOffset(-232);
  	// mpu.setYAccelOffset(-261);
  	mpu.setXGyroOffset(-23);
  	mpu.setYGyroOffset(-92);
  	mpu.setZGyroOffset(-7);

	mpu.setZAccelOffset(920);
	mpu.PrintActiveOffsets();

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

    // mpu.dmpGetQuaternion(&q, fifoBuffer);
    // mpu.dmpGetEuler(euler, &q);
    // Serial.print("euler\t");
    // Serial.print(euler[0] * 180 / PI);
    // Serial.print("\t");
    // Serial.print(euler[1] * 180 / PI);
    // Serial.print("\t");
    // Serial.println(euler[2] * 180 / PI);

    // mpu.dmpGetQuaternion(&q, fifoBuffer);
    // mpu.dmpGetGravity(&gravity, &q);
    // mpu.dmpGetYawPitchRoll(ypr, &q, &gravity);
    // Serial.print("ypr\t");
    // Serial.print(ypr[0] * 180 / PI);
    // Serial.print("\t");
    // Serial.print(ypr[1] * 180 / PI);
    // Serial.print("\t");
    // Serial.print(ypr[2] * 180 / PI);
    // Serial.println();
    // Serial.println();

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

Quaternion MotionControl::getQuaternion(){
	mpu.dmpGetQuaternion(&q, fifoBuffer);
	return q;	
}


VectorFloat MotionControl::getEuler(){
  	mpu.dmpGetQuaternion(&q, fifoBuffer);
  	mpu.dmpGetEuler(euler, &q);

	VectorFloat v3_euler;
	v3_euler.x = euler[0] * 180 / PI;
	v3_euler.y = euler[1] * 180 / PI;
	v3_euler.z = euler[2] * 180 / PI;
	return v3_euler;
}

VectorFloat MotionControl::getYPR(){
  mpu.dmpGetQuaternion(&q, fifoBuffer);
  mpu.dmpGetGravity(&gravity, &q);
  mpu.dmpGetYawPitchRoll(ypr, &q, &gravity);

	VectorFloat v3_ypr;
	v3_ypr.x = (ypr[0] * 180 / PI) + 180;
	v3_ypr.y = (ypr[1] * 180 / PI) + 180;
	v3_ypr.z = (ypr[2] * 180 / PI) + 180;

	return v3_ypr;
}


void MotionControl::resetAccel(){
	mpu.CalibrateAccel(6);
	mpu.CalibrateGyro(6);
	// mpu.PrintActiveOffsets();

  	// mpu.setXGyroOffset(-23);
  	// mpu.setYGyroOffset(-92);
  	// mpu.setZGyroOffset(-7);

	// mpu.setZAccelOffset(920);


	// mpu.CalibrateAccel(6);
	// mpu.CalibrateGyro(6);
	// mpu.PrintActiveOffsets();

}

VectorInt16 MotionControl::getGyro(){
	mpu.dmpGetGyro(&s_rot, fifoBuffer);
	return s_rot;
}

#endif