#ifndef LEDCONTROL_H
#define LEDCONTROL_H

#include <FastLED.h>

#define NUM_LEDS    6
#define LED_TYPE    NEOPIXEL
#define COLOR_ORDER GRB
#define DATA_PIN    14  // D5
#define BRIGHTNESS  100

#define SATURATION  255

CRGB leds[NUM_LEDS];

void ledSetup(){
  FastLED.addLeds<LED_TYPE, DATA_PIN>(leds, NUM_LEDS);
  FastLED.clear();
  FastLED.setBrightness(BRIGHTNESS);
  FastLED.show();
}

void rainbowTest() {
  for (int j = 0; j < 255; j++) {
    for (int i = 0; i < NUM_LEDS; i++) {
      leds[i] = CHSV(i - (j * 2), SATURATION, BRIGHTNESS);
    }
    FastLED.show();
    delay(2);  // the lower the value the faster the colors move (and vice versa)
  }
}

#endif

