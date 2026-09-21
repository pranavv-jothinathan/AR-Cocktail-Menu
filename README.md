## 📖 Overview

An AR cocktail menu application for bars, restaurants, and other drink-serving venues, presenting interactive 3D cocktail visualisations through mobile augmented reality.

The application uses image tracking to anchor the AR experience to a physical coaster, allowing users to explore cocktails, view ingredients and composition details, and interact with their 3D representations.

<br>

## ✨ Key Features

### 📱 AR Cocktail Experience

- **Image-Tracked AR:** Uses a physical coaster/reference image to anchor the cocktail experience in the real world
- **Interactive 3D Drinks:** Displays cocktail models directly within the user's environment
- **Mobile AR Experience:** Designed for compatible Android smartphones using Unity AR Foundation

### 🍸 Cocktail Information & Mixing

- **Drink Navigation:** Browse cocktails using previous and next controls
- **Ingredient Information:** View the ingredients used in each cocktail
- **Cocktail Composition:** Displays the drinks/components used and their mixing ratios
- **Final Cocktail Visualisation:** Shows the completed cocktail formed from the listed ingredients and mixing ratios

### 🔄 Interactive Drink Preview

- **Dedicated Preview Mode:** Opens the selected cocktail in a focused viewing experience
- **360° Model Viewing:** Rotate the cocktail model to inspect it from different angles
- **Touch Interaction:** Uses intuitive mobile touch controls for interacting with and viewing the drink model

<br>

## 🛠️ Technical Specifications

- **Game Engine:** Unity
- **Programming Language:** C#
- **AR Framework:** AR Foundation
- **Android AR Provider:** ARCore
- **Tracking Method:** Image Tracking
- **Target Platform:** Android
- **Interaction Method:** Touch-based mobile interaction
- **3D Content:** Interactive cocktail and drink models

The application uses Unity's AR Foundation framework for image tracking and AR content placement, with ARCore providing the underlying AR functionality on compatible Android devices.

<br>

## 📋 Requirements

### 📱 Hardware Requirements

- **AR Device:** ARCore-compatible Android smartphone
- **Camera:** Rear-facing camera for image tracking
- **Reference Target:** Physical or printed coaster/reference image used by the application
- **Development PC:** A system capable of running Unity and Android development tools
- **Connection:** USB connection for testing and deploying directly to an Android device

### 💻 Software Requirements

- **Unity Hub**
- **Unity Editor:** Unity 6.0 (`6000.0.59f2`)
- **Android Build Support** with SDK, NDK and OpenJDK

<br>

## 🚀 Installation & Setup

### 1. Clone the Repository

```bash
git clone https://github.com/YOUR-USERNAME/AR-Cocktail-Menu.git
cd AR-Cocktail-Menu
```

### 2. Open the Project

- Open **Unity Hub**
- Select **Add → Add project from disk**
- Choose the cloned project folder
- Open the project using **Unity 6.0 (`6000.0.59f2`)**
- Allow Unity to restore the required packages and regenerate the `Library` folder

### 3. Prepare for Android

- Ensure **Android Build Support** is installed through Unity Hub, including:
  - Android SDK
  - Android NDK
  - OpenJDK
- Connect an **ARCore-compatible Android device**
- Enable **USB debugging** on the device if deploying directly from Unity
- Ensure **Android** is selected as the target build platform

### 4. Build & Run

- Open the main AR scene from `Assets/Scenes/`
- Open **File → Build Profiles**
- Select **Android**
- Switch to the Android platform if required
- Select the connected Android device
- Build and run the application

### 5. Start the AR Experience

- Launch the application on the Android device
- Allow camera access when prompted
- Point the camera towards the designated coaster/reference image
- Once the reference image is detected, the AR cocktail menu will appear

<br>

## 📱 How to Use

### Getting Started

1. **Launch the Application:** Open the AR Cocktail Menu on a compatible Android device.
2. **Allow Camera Access:** Grant camera permission when prompted.
3. **Scan the Reference Image:** Point the device camera towards the designated physical coaster/reference image.
4. **Start the AR Experience:** Once the image is detected, the cocktail menu will appear in augmented reality.

### Exploring Cocktails

1. **Browse Drinks:** Use the previous and next controls to move between available cocktails.
2. **View Ingredients:** Check the ingredients used in the selected cocktail.
3. **View Composition:** Explore the ingredients and mixing ratios that make up the drink.
4. **View the Final Cocktail:** See how the listed ingredients come together in the completed cocktail.
5. **Open Preview Mode:** Switch to the dedicated drink preview for a closer look.
6. **Explore in 360°:** Rotate the cocktail model using touch interaction to inspect it from different angles.

### Controls

- **Previous / Next:** Navigate between available cocktails
- **Touch Controls:** Interact with the application interface
- **Drag / Rotate:** Rotate the cocktail model in preview mode
- **Camera Movement:** Move the device naturally to view the AR content from different perspectives

<br>

## 🎯 Core Systems & Interactions

### Image Tracking System

The application uses **AR Foundation** with **ARCore** to detect a physical reference image and position the AR cocktail experience relative to it.

- Detects the designated coaster/reference image
- Anchors virtual cocktail content to the tracked image
- Maintains the relationship between the physical target and AR content

### Cocktail Navigation System

Allows users to browse through the available drinks within the AR menu.

- Previous and next cocktail navigation
- Updates the displayed 3D cocktail model
- Updates the corresponding cocktail information

### Cocktail Information System

Provides information associated with the currently selected cocktail.

- Displays cocktail ingredients
- Presents cocktail composition and mixing ratios
- Shows the completed cocktail produced from the listed ingredients

### Interactive Preview System

Provides a dedicated viewing mode for examining individual cocktail models.

- Opens the selected cocktail in a focused preview
- Supports touch-based model interaction
- Allows 360° rotation of the cocktail model
- Enables closer inspection of the selected drink

<br>

## 📝 Credits

### Development Team

- Pranavv Jothinathan (Project Lead)
- Yifei Liu
- Zhihan Yu
- Skyler Sun

Developed as a group project during the **MSc Immersive Technologies** programme at the **University of Bristol**.

### Third-Party Tools & Packages

The project uses technologies and packages including:

- Unity
- AR Foundation
- ARCore XR Plugin
- TextMesh Pro
- Unity Package Manager dependencies

Additional 3D models, textures, fonts, audio, and other third-party assets used within the project remain subject to their respective licences and terms of use.

<br>

## 🔮 Future Enhancements

Potential future improvements include:

- Expand the available cocktail and drink library
- Show cocktail mixing animations as users change their composition, with the final cocktail according to the user's choices
- Add support for additional mobile platforms
- Explore WebXR deployment for browser-based access

<br>

## 🎥 Demo Video
https://github.com/user-attachments/assets/8173a9c6-d8f4-402e-a13b-f734afce7bbe

