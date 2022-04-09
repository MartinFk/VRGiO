#include <ESP8266WiFi.h>
#include <ESP8266HTTPClient.h>
#include <ESP8266WebServer.h>

#include <Arduino.h>

#include <Wire.h>
//#include <Adafruit_GFX.h>
//#include <Adafruit_SSD1306.h>
#include "Adafruit_MPR121.h"

#include <Adafruit_MPU6050.h>
#include <Adafruit_Sensor.h>

#ifndef _BV
#define _BV(bit) (1 << (bit)) 
#endif

Adafruit_MPR121 cap = Adafruit_MPR121();
Adafruit_MPU6050 mpu;


uint16_t lasttouched = 0;
uint16_t currtouched = 0;

const String server_host_name = "192.168.43.68:8000";
const char* ssid = "MiA";
const char* password = "qwer1234";
ESP8266WebServer server(80); // webserver object for listening to HTTP requests

void setup () {

 
  Serial.begin(9600);

  if (!cap.begin(0x5A)) {
    Serial.println("MPR121 not found, check wiring?");
    for(;;);
  }
  WiFi.begin(ssid, password);
 
  while (WiFi.status() != WL_CONNECTED) {
    Serial.print("Connecting..");
    delay(1000);
  }
  Serial.println("");
  Serial.println("WiFi connected");
 
  // print the IP address
  Serial.print("Use this URL : ");
  Serial.print("http://");
  Serial.print(WiFi.localIP());
  Serial.println("/");
  
  // register cube with the server
  registerCube();
  server.on("/actuate", actuate); //listen for any http message to perform actuation
  server.begin(); // start the server
  Serial.println("Server started");
}

// registers cube with the server for centralized event management 
void registerCube(){
  if (WiFi.status() == WL_CONNECTED) { //Check WiFi connection status
    WiFiClient client; //Declare an object of class WiFiClient
    HTTPClient http;  //Declare an object of class HTTPClient
    http.begin(client, "http://"+server_host_name+"/register/component?shape_class=cube&type=main&src_ip="+WiFi.localIP().toString());  //Specify request destination
    int httpCode = http.GET();                                  //Send the request
    if (httpCode > 0) { //Check the returning code
      String payload = http.getString();   //Get the request response payload
      Serial.println(payload);             //Print the response payload
    }
    http.end();   //Close connection
  }

}

void touch_value(int w,int q)
{
  if (WiFi.status() == WL_CONNECTED) { //Check WiFi connection status
    WiFiClient client; //Declare an object of class WiFiClient
    HTTPClient http;  //Declare an object of class HTTPClient
    http.begin(client, "http://"+server_host_name+"/get_value/component?shape_class="+ WiFi.localIP().toString()+"&type=" + String(q)+"&value="+w);  //Specify request destination
    int httpCode = http.GET();                                  //Send the request
    if (httpCode > 0) { //Check the returning code
      String payload = http.getString();   //Get the request response payload
      Serial.println(payload);             //Print the response payload
    }
    http.end();   //Close connection
  }
 }
// actuates any actuator connected to cube
void actuate()
{
  Serial.println("request received");
  for (int i = 0; i < server.args(); i++) {
      Serial.print(server.argName(i));
      Serial.print(server.arg(i));
      Serial.println("");
      /**
      TODO: ADD ANY ACTUATION HERE.
      **/
    }   
  // send successful response back to client
  server.send(200, "application/json", "{\"result\": true}");  // response to the HTTP request
}

void loop() 
{
   //handle incoming http requests
  currtouched = cap.touched();
  //oledSet();
  for (uint8_t i=0; i<12; i++) {
    // it if *is* touched and *wasnt* touched before, alert!
    if ((currtouched & _BV(i)) && !(lasttouched & _BV(i)) ) {
        Serial.println(i); 
        Serial.println(cap.filteredData(i));
        Serial.println(cap.baselineData(i));
        touch_value(cap.filteredData(i),i);
      // oled.setCursor(10,10);
      // oled.print(i); oled.println("touched"); oled.display();
    }

    // if it *was* touched and now *isnt*, alert!
    if (!(currtouched & _BV(i)) && (lasttouched & _BV(i)) ) {
      //Serial.print(i); Serial.println(" released");
      // oled.print(i); oled.println(" released"); oled.display();

    }
  }


  // reset our state
  lasttouched = currtouched;  

  
  delay(60);
  

server.handleClient();   
  
}