# 1 "c:\\Samples\\Robotarm\\controller\\app.ino"
# 1 "c:\\Samples\\Robotarm\\controller\\app.ino"
# 2 "c:\\Samples\\Robotarm\\controller\\app.ino" 2

// testing a stepper motor with a Pololu A4988 driver board or equivalent
// on an Uno the onboard led will flash with each step
// this version uses delay() to manage timing

byte ledPin = 13;
byte dirPin = 8;
byte steP = 9;
int millisbetweenSteps = 80; // milliseconds - or try 1000 for slower StepperTest

const size_t capacity = ((2) * sizeof(ArduinoJson6101_000::VariantSlot)) + ((4) * sizeof(ArduinoJson6101_000::VariantSlot)) + 40;

void setup()
{
    Serial.begin(9600);
    Serial.println("Starting StepperTest");
    digitalWrite(ledPin, 0x0);

    pinMode(dirPin 0x1);
    pinMode(steP, 0x1);
    pinMode(ledPin, 0x1);
}

void loop()
{
    int availableBytes = Serial.available();

    if (availableBytes > 0)
    {
        char command[availableBytes];

        for (int i = 0; i < availableBytes; i++)
        {
            command[i] = Serial.read();
        }

        Serial.println(command);

        DynamicJsonDocument doc(capacity);
        DeserializationError err = deserializeJson(doc, command);

        if (err)
        {
            Serial.print((reinterpret_cast<const __FlashStringHelper *>((__extension__({static const char __c[] __attribute__((__progmem__)) = ("deserializeJson() failed with code "); &__c[0];})))));
            Serial.println(err.c_str());
        }

        // const char* cmdType[] = doc["type"];
        // JsonObject jObj = doc.as<JsonObject>();
        const char *type = doc["type"];

        JsonObject data = doc["data"];
        int stepPin = data["sP"];
        int directionPin = data["dP"];
        int directionValue = data["dVal"];
        int numberOfSteps = data["steps"];

        Serial.println(stepPin);
        if (stepPin > 0)
        {
            Serial.println(directionValue);
            Serial.println(directionPin);
            Serial.println(numberOfSteps);

            if (directionValue == 0)
            {
                digitalWrite(directionPin, 0x1);
            }
            else
            {
                digitalWrite(directionPin, 0x0);
            }

            for (int n = 0; n < numberOfSteps; n++)
            {
                digitalWrite(stepPin, 0x1);
                digitalWrite(stepPin, 0x0);
                delay(millisbetweenSteps);
            }
        }
    }

    delay(500);

    digitalWrite(ledPin, !digitalRead(ledPin));
}

void MoveArm(JsonObject &jObj)
{
    JsonObject data = jObj["data"];
    int stepPin = data["sP"]; // 9
    int directionPin = data["dP"]; // 8
    int directionValue = data["dVal"]; // 0
    int numberOfSteps = data["steps"]; // 50

    Serial.println("Step pin: " + stepPin);
    Serial.println("Direction pin: " + directionPin);
    Serial.println("Direction Value: " + directionValue);
    Serial.println("Number of Steps: " + numberOfSteps);
    // if (directionValue == 1)
    // {
    //     digitalWrite(directionPin, HIGH);
    // }
    // else
    // {
    //     digitalWrite(directionPin, LOW);
    // }

    // for (int n = 0; n < numberOfSteps; n++)
    // {
    //     digitalWrite(stepPin, HIGH);
    //     delay(millisbetweenSteps);
    //     digitalWrite(stepPin, LOW);
    // }
}
