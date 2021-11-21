#define potPin    A0

void setup() {
    Serial.begin(9600);
    pinMode(potPin, INPUT);
}

void loop(){
  
Serial.println(analogRead(potPin));  
} 
