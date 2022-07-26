#include <Arduino.h>
#include <structs.h>

#ifdef ESP8266
#include <ESP8266WiFi.h>
#include <espnow.h>
#elif ESP32
#include <WiFi.h>
#include <esp_now.h>
#endif


#include <string.h>

// uint8_t broadcastAddress[] = {0xE8, 0xDB, 0x84, 0x9B, 0x5D, 0xDE};   // NODE_1
// uint8_t broadcastAddress[] = {0x94, 0xB9, 0x7E, 0xE5, 0x1B, 0xD8};      // ESP_DEV
// uint8_t basecube1[] = {0x44,0x17,0x93, 0x1e, 0xee, 0x5f};           
uint8_t basecube2[] = {0x98,0xcd,0xac, 0x25, 0x7b, 0x74};   // basecube #2 98:cd:ac:25:7b:74

struct_quat   s_quat;
struct_val    s_val;


char buffer[5];

void espNowSetup();
  void OnDataRecv(uint8_t * mac, uint8_t *incomingData, uint8_t len);
  void OnDataSent(uint8_t *mac_addr, uint8_t sendStatus);
  void passCmd(char * buffer);

void setup(void) {
  Serial.begin(115200);
  espNowSetup();
}


long t_tmr1 = 0;
long t_pass = 0;

void loop(void) {
  if(millis() - t_tmr1 > 10){

  size_t num_read = Serial.readBytesUntil('$', buffer, sizeof(buffer)-1 );
  buffer[num_read] = '\0';  
  
  if(num_read>2) passCmd(buffer);

  t_tmr1 = millis();
  }

}


//Passes incoming text from SerialRead (e.g. Unity) to the destination via espnow
void passCmd(char* token){
  Serial.println(token);
  
   struct_cmd    s_cmd;
  s_cmd.cmd = token[0];
  s_cmd.val = token[1];
  s_cmd.opt = token[2];

  esp_now_send(basecube2, (uint8_t*) &s_cmd, sizeof(s_cmd));

}

void espNowSetup() {
  WiFi.mode(WIFI_STA);
  if (esp_now_init() != 0) {
    Serial.println("Error initializing ESP-NOW");
    delay(5000);
    return;
  }else{
  esp_now_set_self_role(ESP_NOW_ROLE_COMBO);
  esp_now_register_send_cb(OnDataSent);
  esp_now_add_peer(basecube2, ESP_NOW_ROLE_COMBO, 1, NULL, 0);  
  esp_now_register_recv_cb(OnDataRecv);  
  }
}

// Callback when data is sent
void OnDataSent(uint8_t *mac_addr, uint8_t sendStatus) {
  if (sendStatus != 0) {
    Serial.println("[FAIL]");
  }
}


  void OnDataRecv(uint8_t * mac, uint8_t *incomingData, uint8_t len){
  // String mac = String(mac_addr[0],HEX)+":"+String(mac_addr[1],HEX)+":"+String(mac_addr[2],HEX)+":"+String(mac_addr[3],HEX)+":"+String(mac_addr[4],HEX)+":"+String(mac_addr[5],HEX);
  // memcpy(&s_val, incomingData, sizeof(s_val));

  String data;

  switch (incomingData[0]) {
    case 'b':
      Serial.println("b/" + String(incomingData[1]));
      break;

    case 't':
      Serial.println("t/" + String(incomingData[1]));
      break;

    case 'r':
      Serial.println("r/" + String(incomingData[1]));
      break;

    case 'q':
      memcpy(&s_quat, incomingData, sizeof(s_quat));
      data = "q/" + String(s_quat.w, 4) + "/" + String(s_quat.x, 4) + "/" +
             String(s_quat.y, 4) + "/" + String(s_quat.z, 4);
      Serial.println(data);
      break;

    default:
      // Serial.print(String(len) + " / ");
      for (int i = 0; i < len; i++) {
        Serial.print(incomingData[i]);
        Serial.print(" : ");
      }
      Serial.println();
      break;
  }
}

