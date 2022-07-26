#ifndef STRUCTS_H
#define STRUCTS_H

#include <Arduino.h>


//int8, bool, char, byte    1 Byte
//int16, short              2 Byte
//int32, long, float        4 Byte
//double, long long, int64  8 Byte

typedef struct struct_cmd{
  char cmd;
  char val;
  char opt;
}struct_cmd;

typedef struct struct_euler{
  const u_int8_t id = 'e';      
  float psi;    
  float theta;  
  float phi;    
}struct_euler; // 8 Byte
 
typedef struct struct_rot{
  u_int8_t id = 'g';
  int16_t x;
  int16_t y;
  int16_t z;
}struct_rot;

typedef struct struct_val {
  u_int8_t id;
  u_int8_t data;
} struct_val; // 2 Byte

typedef struct struct_quat {
  const u_int8_t id = 'q';
  float w;
  float x;
  float y;
  float z;
} struct_quat; // 20 Byte (aligned)


#endif