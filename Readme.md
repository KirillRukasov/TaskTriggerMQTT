This project is a complete solution for interacting with an ESP32 microcontroller through the MQTT protocol using RabbitMQ as a message broker. The main goal of the project is to demonstrate the capabilities of asynchronous message exchange between a C# server, a React/TypeScript client, and an ESP32 microcontroller.

## Project Components

- **TaskTriggerMQTT.Server:** A backend server in C#, providing an API for interaction with the frontend and sending commands to ESP32 through RabbitMQ.
- **tasktriggermqtt.client:** A client application in React using TypeScript, offering a user interface for sending commands to ESP32 and displaying responses. The payload sent is a digit from a button, which is then used as the required number of button presses.
- **Docker:** Docker Compose configuration for convenient deployment of RabbitMQ with the necessary plugins, including MQTT support. The project is created with the name `rabbitmq_mqtt_project`.
- **ESP32_Firmware:** Firmware for the ESP32 microcontroller, implementing the logic for processing commands received via MQTT and sending responses back to the web application. Includes information about pinout and microcontroller settings. Upon receiving a command, it requires a certain number of presses of the Select button on the 1602A module. To cancel a task, press the Down button.

## Functionality

- **Sending commands from the web interface:** Users can send commands to ESP32 using a convenient web interface.
- **Asynchronous interaction:** Thanks to the use of MQTT through RabbitMQ, the project supports asynchronous message exchange, allowing ESP32 to send responses to commands, which can be displayed in the web interface.
- **Scalability and flexibility:** The project is easily scalable and can be adapted for various tasks of interaction between the server, clients, and IoT devices.

## Project Setup

1. **Start RabbitMQ via Docker Compose:**
   ```
   cd Docker
   docker-compose up -d
   ```

2. **Start the TaskTriggerMQTT.Server:**
   - Open the solution in Visual Studio and run the project.

3. **Start the client application:**
   ```
   cd tasktriggermqtt.client
   npm install
   npm start
   ```

4. **Load the firmware onto ESP32:**
   - Open the ESP32_Firmware project in a development environment supporting ESP32 development (e.g., Arduino IDE) and upload the firmware to the device. Connect the LCD 1602 keypad shield to the ESP32 according to the pinout, and don't forget the 1kΩ resistor.