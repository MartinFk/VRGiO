#include <ESP8266WiFi.h>
#include <ESP8266HTTPClient.h>
 
const char* ssid = "iPhone";
const char* password = "pass1234";
WiFiServer server(80); // webserver object for listening to HTTP requests

void setup () {
 
  Serial.begin(9600);
 
  WiFi.begin(ssid, password);
 
  while (WiFi.status() != WL_CONNECTED) {
    Serial.print("Connecting..");
    delay(1000);
  }
  Serial.println("");
  Serial.println("WiFi connected");
 
  // start the server
  server.begin();
  Serial.println("Server started");
 
  // print the IP address
  Serial.print("Use this URL : ");
  Serial.print("http://");
  Serial.print(WiFi.localIP());
  Serial.println("/");
  
  // register cube with the server
  registerCube();
}


void registerCube(){
  if (WiFi.status() == WL_CONNECTED) { //Check WiFi connection status
    WiFiClient client; //Declare an object of class WiFiClient
    HTTPClient http;  //Declare an object of class HTTPClient
    http.begin(client, "http://f575-77-21-254-221.ngrok.io/register/component?shape_class=cube&type=main&src_ip="+WiFi.localIP().toString());  //Specify request destination
    int httpCode = http.GET();                                  //Send the request
    if (httpCode > 0) { //Check the returning code
      String payload = http.getString();   //Get the request response payload
      Serial.println(payload);             //Print the response payload
    }
    http.end();   //Close connection
  }
} 
void loop() {
 

}
