# 🏎️ Hydrogen Racing Car Telemetry – Battery Monitoring

This project enables **real-time battery telemetry** for a **hydrogen-powered racing car**, built for high school competitions. It reads battery capacity using a Seeed XIAO nRF52 BLE board and transmits it via **Bluetooth Low Energy (BLE)** to a **.NET MAUI mobile app**.

> Please read this document carefully to understand the setup, components, and how to continue development.

---

## 📦 Contents

- 📁 `xiaoble-source/` – Firmware code for the Seeed XIAO board (C++ with PlatformIO)
- 📁 `pcb/` – KiCad files for the electrical design
- 📁 `maui-source/` – code for the mobile app (C# MAUI framework)
- 📄 `README.md` – Project documentation (you are here)

---

## ⚙️ System Overview

- **Board**: Seeed XIAO nRF52 BLE
- **Function**: Reads battery voltage/capacity and transmits data via BLE
- **Receiver**: Mobile app built with .NET MAUI
- **Firmware**: C++ using PlatformIO + Bluefruit library

---

## 🧰 Tools & Dependencies

### Hardware

- Seeed XIAO nRF52 BLE board
- Battery and voltage measuring circuit (see `pcb/`)
- Bluetooth-capable mobile device (Android)

### Software

- [PlatformIO](https://platformio.org/) (for microcontroller firmware)
- [KiCad](https://kicad.org/) (for circuit schematics)
- [.NET MAUI](https://learn.microsoft.com/en-us/dotnet/maui/) (for the mobile app)

---

## 📚 Used Libraries

### 🧠 Microcontroller (Seeed XIAO nRF52 BLE)

- **[Adafruit Bluefruit nRF52 Library](https://github.com/adafruit/Adafruit_nRF52_Arduino)**  
  Provides BLE support for nRF52 chips. Used to advertise services, send battery data, and manage BLE characteristics.

> No additional installation required

### 📱 Mobile App (MAUI)

- [Plugin.BLE](https://github.com/xabre/xamarin-bluetooth-le) by xabre  
  Cross-platform BLE plugin for Xamarin/.NET MAUI. Used to scan, connect, and read BLE data from the XIAO board.

Add via NuGet:

```bash
dotnet add package Plugin.BLE
```
---
## 🛠️ Getting Started

### 🚀 Firmware Setup (Microcontroller)

1. Install [Visual Studio Code](https://code.visualstudio.com/) with the [PlatformIO extension](https://platformio.org/install).
2. Open the project folder in VSCode.
3. Connect the Seeed XIAO nRF52 BLE board via USB.
4. Upload the firmware:

```bash
pio run --target upload
```
## 💡 Future Improvements

### 🧪 Add sensors for:

- Hydrogen flow measurement

- Motor/controller temperature

- Wheel speed or RPM

### 📱 Improve the mobile app:

- Real-time graphs

- Database

## 🙏 Acknowledgments
Originally developed by Georgi Nikolov as part of the high school hydrogen racing program.
Now handed off to the next generation — good luck and keep innovating!
