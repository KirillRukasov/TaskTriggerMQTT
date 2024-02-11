#define SELECT_KEY 1
#define LEFT_KEY 2
#define DOWN_KEY 3
#define UP_KEY 4
#define RIGHT_KEY 5

#include <LiquidCrystal.h>
#include <EspMQTTClient.h>

const int rs = 21, en = 22, d4 = 23, d5 = 19, d6 = 18, d7 = 5; // Обновленные пины для ESP32
LiquidCrystal lcd(rs, en, d4, d5, d6, d7);                     // Выбор пинов, используемых на панели LCD

EspMQTTClient client(
    "your_ssid",
    "your_wifi_password",
    "192.168.0.32", // MQTT Broker server ip
    "guest",        // Can be omitted if not needed
    "guest",        // Can be omitted if not needed
    "ESP32Client",  // Client name that uniquely identify your device
    1883            // The MQTT port, default to 1883. this line can be omitted
);

const char *MQTT_INBOX_TOPIC = "ESP32Client/inbox";
const char *MQTT_OUTBOX_TOPIC = "ESP32Client/outbox";

#ifndef LED_BUILTIN
#define LED_BUILTIN 2
#endif

int getKeyID()
{
  int aRead = analogRead(34);
  if (aRead < 300)
    return RIGHT_KEY;
  if (aRead < 600)
    return UP_KEY;
  if (aRead < 1000)
    return DOWN_KEY;
  if (aRead < 1250)
    return LEFT_KEY;
  if (aRead < 1500)
    return SELECT_KEY;
  return 0;
}

unsigned long startTime;
unsigned long endTime;
int targetPresses = 0;
int currentPresses = 0;
bool buttonPressedLast = false;
bool taskInProgress = false;

void blinkLed(int times, int delayTime)
{
  for (int i = 0; i < times; i++)
  {
    digitalWrite(LED_BUILTIN, HIGH);
    delay(delayTime);
    digitalWrite(LED_BUILTIN, LOW);
    if (i < times - 1)
    { // Avoid delay after the last blink
      delay(delayTime);
    }
  }
}

void onConnectionEstablished()
{
  // Subscribe to "mytopic/test" and display received message to Serial
  client.subscribe(MQTT_INBOX_TOPIC, [](const String &payload)
                   {
    Serial.println(payload);

    if (!taskInProgress)
    {
      targetPresses = payload.toInt();
      if (targetPresses > 0)
      {
        taskInProgress = true;
        startTime = millis();
        currentPresses = 0;
        lcd.clear();
        lcd.setCursor(0, 0);
        lcd.print("Target: ");
        lcd.print(targetPresses);
        lcd.setCursor(0, 1);
        lcd.print("Presses: 0");
      }
    } });

  // Subscribe to "mytopic/wildcardtest/#" and display received message to Serial
  client.subscribe("ESP32Client/wildcardtest/#", [](const String &topic, const String &payload)
                   { Serial.println("(From wildcard) topic: " + topic + ", payload: " + payload); });

  // Publish a message to "mytopic/test"
  client.publish("ESP32Client/test", "This is a message"); // You can activate the retain flag by setting the third parameter to true

  // Execute delayed instructions
  client.executeDelayed(5 * 1000, []()
                        { client.publish("ESP32Client/wildcardtest/test123", "This is a message sent 5 seconds later"); });
}

void mqttCallback(char *topic, byte *payload, unsigned int length)
{
  if (!taskInProgress)
  { // Проверяем, не выполняется ли уже задание
    char message[length + 1];
    for (int i = 0; i < length; i++)
    {
      message[i] = (char)payload[i];
    }
    message[length] = '\0';
    targetPresses = atoi(message);
    if (targetPresses > 0)
    { // Устанавливаем задание только если полученное число больше 0
      taskInProgress = true;
      startTime = millis();
      currentPresses = 0; // Обнуляем счетчик нажатий
      lcd.clear();
      lcd.setCursor(0, 0);
      lcd.print("Target: ");
      lcd.print(targetPresses);
      lcd.setCursor(0, 1);
      lcd.print("Presses: 0");
    }
  }
}

void checkButtonPress()
{
  int key = getKeyID();
  bool buttonPressedNow = key != 0;

  if (buttonPressedNow && !buttonPressedLast)
  {
    if (key == DOWN_KEY && taskInProgress)
    {                         // Если нажата кнопка Down и задание активно
      taskInProgress = false; // Отменяем задание
      client.publish(MQTT_OUTBOX_TOPIC, "Task cancelled by user");
      lcd.clear();
      lcd.print("Task cancelled");
      delay(2000); // Показываем сообщение о сбросе на 2 секунды
      lcd.clear();
    }
    else if (key == SELECT_KEY && taskInProgress)
    {
      currentPresses++;
      lcd.setCursor(0, 1);
      lcd.print("Presses: ");
      lcd.print(currentPresses);

      if (currentPresses == targetPresses)
      {
        endTime = millis();
        long timeTaken = (endTime - startTime) / 1000;
        taskInProgress = false; // Задание выполнено
        lcd.clear();
        lcd.setCursor(0, 0);
        lcd.print("Done");
        lcd.setCursor(0, 1);
        lcd.print("Time: ");
        lcd.print(timeTaken);
        lcd.print(" sec");
        client.publish(MQTT_OUTBOX_TOPIC, "Task completed in " + String(timeTaken) + " seconds");
      }
    }
  }
  buttonPressedLast = buttonPressedNow;
}

void setup()
{
  Serial.begin(115200);
  lcd.begin(16, 2);
  pinMode(LED_BUILTIN, OUTPUT);

  client.enableDebuggingMessages(); // Enable debugging messages sent to serial output
  client.enableHTTPWebUpdater();    // Enable the web updater. User and password default to values of MQTTUsername and MQTTPassword. These can be overridded with enableHTTPWebUpdater("user", "password").
  client.enableLastWillMessage("TestClient/lastwill", "I am going offline");
}

void loop()
{
  client.loop();
  checkButtonPress();
  delay(100);
}
