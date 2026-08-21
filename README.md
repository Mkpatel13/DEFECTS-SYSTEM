# PCB Defect Inspection Simulator

## Project Structure Overview

The `Assets/` directory is organized as follows to maintain a clean and scalable project structure:

- **Scenes/**: Contains the Unity scene files (e.g., `MainScene.unity`).
- **Scripts/**: All C# scripts for the project, categorized by functionality:
  - **Player/**: Scripts handling player movement, camera controls, and input.
  - **Interaction/**: Scripts for interacting with the environment and 3D objects.
  - **Inspection/**: Scripts related to the PCB inspection logic and mechanics.
  - **UI/**: Scripts for managing user interface elements (menus, HUDs, overlays).
  - **Data/**: Scripts for data models, state management, and saving/loading configurations.
- **Prefabs/**: Reusable GameObject configurations, categorized by type:
  - **Machine/**: Inspection machines and related mechanical parts.
  - **PCB/**: Printed Circuit Board models with and without defects.
  - **Servers/**: Server racks and networking equipment props.
  - **Environment/**: Industrial environment props (floors, walls, lighting fixtures).
- **Materials/**: Materials used to texture 3D models and UI elements.
- **Models/**: Raw 3D model files (e.g., .fbx, .obj) imported into the project.
- **UI/**: UI assets such as sprites, fonts, and UI panels/prefabs.
- **Animations/**: Animation clips and Animator Controllers for moving parts.
- **Data/**: Static data files (e.g., JSON, XML, ScriptableObjects) used for configuration.
- **Resources/**: Assets that need to be loaded dynamically at runtime via `Resources.Load()`.

*Phase 1 focuses purely on the foundation and interactive 3D prototype. Real API connections, YOLOv8 integration, FastAPI, Spring Boot, and MySQL will be implemented in subsequent phases.*
