#include <ESP8266WiFi.h>
#include <ESP8266HTTPClient.h>
#include <ESP8266WebServer.h>


const String server_host_name = "172.20.10.2:8000";
const char* ssid = "iPhone";
const char* password = "pass1234";
ESP8266WebServer server(80); // webserver object for listening to HTTP requests

void setup () {
 
  Serial.begin(9600);
 
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

// actuates any actuator connected to cube
void actuate(){
  Serial.println("received");
  for (int i = 0; i < server.args(); i++) {
      Serial.println(server.argName(i));
      Serial.println(server.arg(i));
      /**
      TODO: ADD ANY ACTUATION HERE.
      **/
    }   
  // send successful response back to client
  server.send(200, "application/json", "{\"result\": true}");  // response to the HTTP request
}

void loop() {
  server.handleClient();    //handle incoming http requests
}
