#include<Servo.h>
Servo PWM1;
Servo PWM2;
Servo PWM3;
Servo PWM4;
void setup() {
  // put your setup code here, to run once:
  PWM1.attach(10);
  PWM2.attach(11);
  PWM3.attach(12);
  PWM4.attach(13);
  Serial.begin(9600);
}

void loop() {
  // put your main code here, to run repeatedly:
  if(Serial.available()){
    String input = Serial.readStringUntil('\n');
    double value = 0;
    int pwmnum = int(input[0]);
    String f;
    for(int i = 2; i < input.length(); i++){
        f += input[i];
      }
      value = f.toDouble();
      if(pwmnum == 0){
        PWM1.write(value);
      }
      else if(pwmnum == 1){
        PWM2.write(value);
      }
      else if(pwmnum == 2){
        PWM3.write(value);
     }
     else if(pwmnum == 3){
        PWM4.write(value); 
     }
  }
}
