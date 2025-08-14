# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a Unity 6000.1.9f1 project focused on character customization and body measurement integration using the UMA (Unity Multipurpose Avatar) framework. The project includes a custom body measurement system that loads data from JSON files and converts them to UMA DNA values for character generation.

## Core Architecture

### Key Components

1. **AvatarBodyApplier System** (`Assets/Scripts/AvatarBodyApplier.cs`)
   - MonoBehaviour class that handles loading body measurement data from JSON files or server endpoints
   - Converts measurement data (height, shoulder_width, chest, waist, hip) to UMA DNA values
   - Supports both local file loading (from StreamingAssets) and remote server loading
   - Uses UnityWebRequest for data fetching and includes comprehensive error handling
   - Provides real-time individual parameter adjustment methods

2. **UMA Framework Integration**
   - Extensive UMA (Unity Multipurpose Avatar) system for character generation
   - Located in `Assets/UMA/` with Core, Content, Examples, and Editor components
   - Handles DNA conversion, character recipes, dynamic character system (DCS)
   - Includes addressable asset system integration

3. **Data Structure**
   - Body measurements stored in `Assets/StreamingAssets/body_measurements.json`
   - JSON format with normalized values (0-1 range): uma_height, uma_width, uma_waist, uma_belly, uma_fore_arm, uma_arm, uma_legs
   - DNA mapping: uma_height→height, uma_width→armWidth, uma_waist→waist, uma_belly→belly, uma_fore_arm→forearmLength, uma_arm→armLength, uma_legs→legSize
   - AvatarBodyData class provides serializable structure with Korean language comments for UI labels

### Project Structure

- **Assets/Scripts/**: Custom application scripts (currently contains AvatarBodyApplier.cs)
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
- Use Unity's standard build process through Build Settings (File → Build Settings)
- For development builds: File → Build Settings → Development Build + Script Debugging
- Builds will include StreamingAssets folder for runtime data access
- Multiple C# project files are generated for different assembly definitions (UMA modules)

### C# Project Files
- Main solution file: `AvatarViewer.sln`
- Assembly definition projects: `UMA_Core.csproj`, `UMA_Content.csproj`, `UMA_Examples.csproj`, etc.
- Open solution in Visual Studio/Rider for full IntelliSense support

## Key Dependencies

- Unity 6000.1.9f1 (LTS recommended)
- Universal Render Pipeline (URP) 17.1.0
- Unity Input System 1.14.0
- UMA framework (integrated as project assets)
- AI Navigation package 2.0.8

## Working with Body Measurements

When modifying the body measurement system:
1. JSON data in StreamingAssets must maintain 0-1 normalized values
2. UMA DNA mapping is hardcoded in `ApplyBodyDataToAvatar()` method in AvatarBodyApplier.cs:148-154
3. Server integration requires updating the `serverUrl` field in AvatarBodyApplier component
4. Loading events are handled through `OnBodyDataLoaded` and `OnErrorOccurred` event system
5. Individual parameter adjustment methods (SetHeight, SetWidth, SetWaist, SetBelly, SetForeArm, SetArm, SetLegs) provide real-time updates
6. Use `SetBodyData()` for bulk updates or `SetBodyDataFromJson()` for JSON string input

## UMA Integration Notes

- UMA uses a recipe-based system for character generation
- DNA values control morphological features of characters (0-1 range)
- Core DNA names used: height, armWidth, waist, belly, forearmLength, armLength, legSize
- `DynamicCharacterAvatar.SetDNA()` method applies individual DNA values
- `DynamicCharacterAvatar.BuildCharacter()` rebuilds the avatar after DNA changes
- Slot system handles different body parts and clothing
- Addressable assets are used for optimized loading
- Examples folder contains numerous demo scenes and scripts for reference

## Architecture Notes

- The system uses Korean language comments for UI elements, indicating localization support
- Error handling includes both console logging and event-based error reporting
- The AvatarBodyApplier automatically finds DynamicCharacterAvatar in scene if not assigned
- JSON parsing uses Unity's built-in JsonUtility with try-catch error handling
- Individual parameter setters include Mathf.Clamp01() for value validation