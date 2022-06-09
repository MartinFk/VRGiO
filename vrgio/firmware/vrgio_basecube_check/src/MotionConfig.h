#include <Adafruit_MPU6050.h>

class MotionTracker
{

public:
	MotionTracker(){
		imu = Adafruit_MPU6050();
	};

	int setup(){
		if (!imu.begin()) {
			return -1;
		}
  
		imu.setAccelerometerRange(MPU6050_RANGE_8_G);
		imu.setGyroRange(MPU6050_RANGE_500_DEG);
		imu.setFilterBandwidth(MPU6050_BAND_21_HZ);
		return 1;
	}
    
	/// Prints All MPU6050 related values every 1000ms
	void printMPU(){
		if (millis() - tPrintData > 1000){
			imu.getEvent(&a, &g, &temp);
			Serial.print("ACC: ( ");
			Serial.print(a.acceleration.x);
			Serial.print(" , ");
			Serial.print(a.acceleration.y);
			Serial.print(" , ");
			Serial.print(a.acceleration.z);
			Serial.println(" ) [m/s^2]");

			Serial.print("GYR: ( ");
			Serial.print(g.gyro.x);
			Serial.print(" , ");
			Serial.print(g.gyro.y);
			Serial.print(" , ");
			Serial.print(g.gyro.z);
			Serial.println(" ) [rad/s]");

			Serial.print("TMP: ( ");
			Serial.print(temp.temperature);
			Serial.println(" ) °C");

			tPrintData = millis();
		}
	}

private:
	Adafruit_MPU6050 imu;
	sensors_event_t a, g, temp;
	unsigned long tPrintData = 0; 		// Timer variable
	int status = 0;             //Status Codes: 0 = not init ; 1 = running ; -1 = error, not fonud

};
