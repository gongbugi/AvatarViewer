# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a Unity 6000.1.9f1 project focused on character customization and body measurement integration using the UMA (Unity Multipurpose Avatar) framework. The project includes a custom body measurement system that loads data from JSON files and converts them to UMA DNA values for character generation.

## Core Architecture

### Key Components

1. **BodyMeasurements System** (`Assets/Scripts/BodyMeasurements.cs`)
   - Singleton class that handles loading body measurement data from JSON files or server endpoints
   - Converts measurement data (height, shoulder_width, chest, waist, hip) to UMA DNA values
   - Supports both local file loading (from StreamingAssets) and remote server loading
   - Uses UnityWebRequest for data fetching

2. **UMA Framework Integration**
   - Extensive UMA (Unity Multipurpose Avatar) system for character generation
   - Located in `Assets/UMA/` with Core, Content, Examples, and Editor components
   - Handles DNA conversion, character recipes, dynamic character system (DCS)
   - Includes addressable asset system integration

3. **Data Structure**
   - Body measurements stored in `Assets/StreamingAssets/body_measurements.json`
   - JSON format with normalized values (0-1 range): height, shoulder_width, chest, waist, hip
   - DNA mapping: height→Height, shoulder_width→ArmWidth, chest→BreastSize, waist→Waist, hip→GluteusSize

### Project Structure

- **Assets/Scripts/**: Custom application scripts (currently contains BodyMeasurements.cs)
- **Assets/UMA/**: UMA framework with Core, Content, Examples, and Editor folders
- **Assets/StreamingAssets/**: Runtime data files (body_measurements.json, textures)
- **Assets/Scenes/**: Unity scene files (SampleScene.unity)
- **Assets/Settings/**: Render pipeline and quality settings
- **Library/**: Unity-generated build cache and artifacts
- **Packages/**: Unity package dependencies managed via manifest.json
- **ProjectSettings/**: Unity project configuration

## Development Commands

### Unity Editor
- Open project in Unity Editor 6000.1.9f1 or later
- Use Unity's built-in Build Settings for platform-specific builds
- Package Manager handles dependencies automatically

### Build Process
- Use Unity's standard build process through Build Settings
- Builds will include StreamingAssets folder for runtime data access
- Multiple C# project files are generated for different assembly definitions (UMA modules)

## Key Dependencies

- Unity 6000.1.9f1 (LTS recommended)
- Universal Render Pipeline (URP) 17.1.0
- Unity Input System 1.14.0
- UMA framework (integrated as project assets)
- AI Navigation package 2.0.8

## Working with Body Measurements

When modifying the body measurement system:
1. JSON data in StreamingAssets must maintain 0-1 normalized values
2. UMA DNA mapping is defined in `GetUMADNAValues()` method
3. Server integration requires updating the `serverUrl` field
4. Loading events are handled through the `OnMeasurementsLoaded` event system

## UMA Integration Notes

- UMA uses a recipe-based system for character generation
- DNA values control morphological features of characters
- Slot system handles different body parts and clothing
- Addressable assets are used for optimized loading
- Examples folder contains numerous demo scenes and scripts for reference