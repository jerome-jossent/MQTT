// OLED
#include <U8g2lib.h>
// Pins soudé sur le module
#define OLED_SDA 5
#define OLED_SCL 6
// OLED 72x40 sur son propre bus I2C (Software I2C)
U8G2_SSD1306_72X40_ER_F_SW_I2C u8g2(U8G2_R0, OLED_SCL, OLED_SDA, U8X8_PIN_NONE);

// Wifi + MQTT
#include <WiFi.h>
#include <PubSubClient.h>
const char* ssid = "Freebox-3ECC2D-invité";
const char* password = "JérômeJérôme";
const char* mqtt_server = "192.168.0.242";
WiFiClient espClient;
PubSubClient client(espClient);

// Lidar
#define RXD2 4 // ne pas utiliser RX qui correspond à la COM du port USB
#define LIDAR_CTL_PIN 3
HardwareSerial LD06Serial(1);


// buffer concaténé
uint16_t lidarBuffer[3600];
// 1 tour = ~400 points max (12 points × ~33 trames)
// On prend 500 pour avoir de la marge
#define MAX_POINTS_PER_SCAN 500

struct LidarPoint {
  uint16_t angle;     // en 1/100 de degré (0 à 35999)
  uint16_t distance;  // en mm
};

LidarPoint scanBuffer[MAX_POINTS_PER_SCAN];


int scanIndex = 0;

void connectWiFi()
{
  WiFi.begin(ssid, password);

  while (WiFi.status() != WL_CONNECTED)
    delay(500);
}

void connectMQTT()
{
  while (!client.connected())
  {
    client.connect("ESP32_LD06");
    delay(500);
  }
}

void readLidarLD06()
{
  static uint8_t frame[47];
  static int frameIndex = 0;
  static float lastStartAngle = -1;

  while (LD06Serial.available())
  {
    uint8_t b = LD06Serial.read();

    if (frameIndex == 0 && b != 0x54)
      continue;

    frame[frameIndex++] = b;

    if (frameIndex == 47)
    {
      frameIndex = 0;

      if (frame[1] != 0x2C)
        continue;

      // ── Angle de début de trame (en 1/100°) ──
      float startAngle = (float)(frame[4] | (frame[5] << 8)) / 100.0f;
      float endAngle   = (float)(frame[42] | (frame[43] << 8)) / 100.0f;

      // ── Détection nouveau tour ──────────────
      // Si l'angle repart en arrière → on a bouclé
      if (lastStartAngle >= 0 && startAngle < lastStartAngle - 10.0f)
      {
        // ✅ Tour complet détecté → on publie
        if (scanIndex > 0)
          publishData();
      }

      lastStartAngle = startAngle;

      // ── Calcul du pas angulaire entre les 12 points ──
      float angleStep;
      if (endAngle > startAngle)
        angleStep = (endAngle - startAngle) / 11.0f;
      else
        angleStep = (endAngle + 360.0f - startAngle) / 11.0f;

      // ── Stockage des 12 points ──────────────
      for (int i = 0; i < 12; i++)
      {
        uint16_t distance =
          (uint16_t)frame[6 + i * 3] |
          ((uint16_t)frame[7 + i * 3] << 8);

        float angle = startAngle + angleStep * i;
        if (angle >= 360.0f) angle -= 360.0f;

        if (scanIndex < MAX_POINTS_PER_SCAN)
        {
          // Stocke angle en 1/100° et distance
          scanBuffer[scanIndex].angle    = (uint16_t)(angle * 100);
          scanBuffer[scanIndex].distance = distance;
          scanIndex++;
        }
      }
    }
  }
}

void publishData()
{
  // Payload : chaque point = 4 bytes (2 angle + 2 distance)
  uint8_t payload[MAX_POINTS_PER_SCAN * 4];
  int payloadSize = scanIndex * 4;

  for (int i = 0; i < scanIndex; i++)
  {
    payload[i * 4 + 0] = scanBuffer[i].angle & 0xFF;
    payload[i * 4 + 1] = scanBuffer[i].angle >> 8;
    payload[i * 4 + 2] = scanBuffer[i].distance & 0xFF;
    payload[i * 4 + 3] = scanBuffer[i].distance >> 8;
  }

  bool ok = client.publish(
    "lidar/raw",
    payload,
    payloadSize,d:\Projets\Photobooth\Photobooth_Arduino\UNO_BTN_RGBStrip_RGBRing_CameraShot_TFT1.44\UNO_BTN_RGBStrip_RGBRing_CameraShot_TFT1.44.ino
    false
  );

  if (!ok)
    print("Pub FAIL");

  scanIndex = 0;
}

void setupLidar(){
  ledcAttach(LIDAR_CTL_PIN, 10000, 8);
  ledcWrite(LIDAR_CTL_PIN, 255); // 50%

  LD06Serial.begin(230400, SERIAL_8N1, RXD2, -1);
}


void setup()
{
  Serial.begin(115200);
  delay(1000); 

  setupOLED();
  print("OLED OK");

  connectWiFi();  
  print("Wifi OK");

  client.setServer(mqtt_server, 1883);
  client.setBufferSize(8192);
  print("mqtt OK");

  connectMQTT();

  client.publish("lidar/test","ok");
  print("mqtt test");

  setupLidar();
  print("Lidar OK");
}


void setupOLED(){
  u8g2.begin();
  u8g2.setContrast(255);
}

void loop()
{
  if (!client.connected())
  {
    print("MQTT reconnexion");
    connectMQTT();
  }

  client.loop();
  readLidarLD06();

  if (scanIndex >= 3600)
    publishData();
}

void print(String val_txt){
  Serial.println(val_txt);

  u8g2.clearBuffer();
  u8g2.setFont(u8g2_font_5x7_tr);//u8g2_font_5x7_tr //u8g2_font_9x15_tr
  u8g2.drawStr(10, 15, val_txt.c_str());
  u8g2.sendBuffer();
}