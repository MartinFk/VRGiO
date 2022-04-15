#include <Arduino.h>
#include <ESP8266WiFi.h>
#include <ESP8266HTTPClient.h>
#include <ESP8266WebServer.h>
#include <WebSocketsClient.h>
#include <sstream>
#include <iostream>
#include <string>

const String server_host_name = "192.168.0.171";
const String port = "8000";
const char *ssid = "PingSlayer";        //"vrgio-iot";
const char* password = "slaytheping117";//"vrgiovrgio";
ESP8266WebServer server(80); // webserver object for listening to HTTP requests
WebSocketsClient webSocket;  // websocket object

void registerCube();
void actuate();
void webSocketEvent(WStype_t type, uint8_t * payload, size_t length){
  switch (type){
    case WStype_DISCONNECTED:
      Serial.printf("[WSc] Disconnected!\n");
      break;

    case WStype_CONNECTED: {
        Serial.printf("[WSc] Connected to url %s\n", payload);
        webSocket.sendTXT("Connected");
      }
      break;

    case WStype_TEXT:
      Serial.printf("[WSc] get text: %s\n", payload);
      if (String((char *)(payload)) == "send data") {
        std::ostringstream oss;
        const char *additional_data = "";
        oss << "{type:json, additional_data:[" << additional_data << "]}";
        webSocket.sendTXT(oss.str().c_str());
      }
      break;

    case WStype_BIN:
      Serial.printf("[WSc] get text: %s\n", payload);
      break;
    }
}

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

  // websocket
  webSocket.begin(server_host_name, port.toInt(), "/ws/" + WiFi.localIP().toString());
  webSocket.onEvent(webSocketEvent);
  webSocket.setReconnectInterval(5000);

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
    http.begin(client, "http://"+server_host_name+":"+port+"/register/component?shape_class=cube&type=main&src_ip="+WiFi.localIP().toString());  //Specify request destination
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

void loop() {
  server.handleClient(); // handle incoming http requests
  webSocket.loop();
}
