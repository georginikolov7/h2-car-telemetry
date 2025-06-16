#include <Arduino.h>
#include <bluefruit.h>
#include <nrf_nvic.h>
#include <nrf_sdm.h>

// CONSTANTS:
const int Sleep_Time = 3000;
#define BatteryPin A0
const char* deviceName = "H2Car";

const int Battery_Min_Voltage = 6.4;
const int Battery_Max_Voltage = 8.6;

// VALUES:
int batteryReadVal = 0;
double batteryPercentage = 0;
int roundedPercentage = 0;

// BLE OBJECTS:
BLEService batService(0x180F);
BLECharacteristic batStatus(0x2A19);

// FUNCTION DECLARATIONS:
void connect_callback(uint16_t conn_handle);
void disconnect_callback(uint16_t conn_handle, uint8_t reason);
void startAdvertising();
void setupBle();

void setup()
{
    // Serial.begin(115200);

    pinMode(BatteryPin, INPUT);
    analogReadResolution(12);
    setupBle();
    delay(20);

    Serial.println("Setup complete!");
}

void loop()
{
    // Read battery level input:
    batteryReadVal = analogRead(BatteryPin);
    Serial.printf("Read val: %i \n\r", batteryReadVal);
    
    // Equation for battery percentage: bat% = 0.21*(analogRead - 477.27*Vbatmin)/(Vbatmax-Vbatmin)
    batteryPercentage = (0.21 * (batteryReadVal - 477.27 * Battery_Min_Voltage) / (Battery_Max_Voltage - Battery_Min_Voltage));

    // Round percentage:
    roundedPercentage = 5 * round(batteryPercentage / 5.0);
    Serial.println(roundedPercentage);

    // Validate boundaries:
    if (roundedPercentage < 0) {
        roundedPercentage = 0;
    } else if (roundedPercentage > 100) {
        roundedPercentage = 100;
    }

    // Notify connected client with the analog value
    batStatus.notify8(roundedPercentage);

    // Serial.printf("Analog read: %i\n\r", batteryReadVal);
    // Serial.printf("Percentage: %f\n\r", batteryPercentage);

    delay(Sleep_Time); // Wait before sending the next value
}

void connect_callback(uint16_t conn_handle)
{
    Serial.println("Connected!");
}

void disconnect_callback(uint16_t conn_handle, uint8_t reason)
{
    Serial.println("Disconnected!");
}
void startAdvertising()
{
    // Bluefruit.Advertising.addService(batService);
    Bluefruit.Advertising.addFlags(BLE_GAP_ADV_FLAGS_LE_ONLY_GENERAL_DISC_MODE);
    Bluefruit.Advertising.addTxPower();
    Bluefruit.Advertising.addName();

    Bluefruit.Advertising.start();
}

void setupBle()
{
    // Initialize Bluefruit
    Serial.println("Starting BLE");
    Bluefruit.begin();
    Bluefruit.setTxPower(4); // Max transmit power
    Bluefruit.setName(deviceName); // BLE device name
    Bluefruit.Periph.setConnectCallback(connect_callback);
    Bluefruit.Periph.setDisconnectCallback(disconnect_callback);

    Serial.println("Attaching service");
    // Add custom BLE service
    err_t error = batService.begin();
    Serial.println(error);
    Serial.println("Attaching characteristic");

    // Add characteristic to the custom service
    batStatus.setProperties(CHR_PROPS_NOTIFY | CHR_PROPS_READ);
    batStatus.setPermission(SECMODE_OPEN, SECMODE_NO_ACCESS);
    batStatus.setFixedLen(2); // 2 bytes for a 16-bit analog value
    error = batStatus.begin();
    Serial.println(error);
    // Start advertising
    startAdvertising();
}