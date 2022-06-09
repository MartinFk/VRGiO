#include <FastLED.h>

#define NUM_LEDS      	6
#define LED_TYPE   		NEOPIXEL
#define COLOR_ORDER   	GRB
#define DATA_PIN        13 			//D7
#define BRIGHTNESS 		255   
#define SATURATION 		255  


// Mapping of LED Order corresponding to the Touchpoints 0 to 5
// {11,0,1,9,6,7};

int tMap[12] = {1,2,7,8,10,11,4,5,9,3,6,0};
CRGB leds[NUM_LEDS];


void rainbowTest(){
for (int j = 0; j < 255; j++) {
    for (int i = 0; i < NUM_LEDS; i++) {
      leds[i] = CHSV(i - (j * 2), SATURATION, BRIGHTNESS);
    }
    FastLED.show();
    delay(10); // the lower the value the faster your colors move (and vice versa)
  }
}
