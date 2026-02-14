# Agents

**ValheimVRM** is a BepInEx mod for Valheim that allows players to replace their character models with custom VRM (Virtual Reality Model) avatars. Here's what I've learned about the codebase:

Be sure to update this rule as you make extensive changes to the codebase.

## Core Architecture

The mod is built around several key components:

1. **MainPlugin.cs** - Entry point that initializes the mod using BepInEx
2. **VRM.cs** - Core VRM handling class with import functionality
3. **VrmManager.cs** - Central manager for VRM instances and player mappings
4. **Settings.cs** - Configuration system for both global and per-character settings
5. **VrmController.cs** - Component attached to players to manage their VRM instances

## Key Features

### VRM Import & Management

- Supports both VRM 0.x and VRM 1.0 formats
- Async loading with progress tracking
- Automatic shader management (MToon and Standard Unity shaders)
- Texture processing and optimization
- Spring bone physics simulation

### Animation System

- **VRMAnimationSync.cs** - Synchronizes Valheim's animation system with VRM models
- Human pose mapping between original and VRM animators
- State-based position adjustments for different activities (sitting, sleeping, etc.)
- Smooth transitions between animation states

### Configuration System

- **Per-character settings** stored in `settings_[CharacterName].txt`
- **Global settings** in `global_settings.txt`
- Extensive customization options:
  - Model scaling and positioning
  - Player collider adjustments
  - Equipment positioning offsets
  - Spring bone physics parameters
  - Shader and rendering options

### Multiplayer Support

- VRM sharing between players
- Network synchronization of VRM data
- Hash-based change detection
- Packet-based data transfer system

### Performance Features

- LOD (Level of Detail) system for VRM models
- Player fade when camera gets close
- Wind simulation for spring bones
- Profiling system for performance monitoring

## Technical Implementation

### Harmony Patching

- Uses Harmony library for runtime code modification
- Patches Valheim's shader system to support VRM shaders
- Hooks into animation and rendering systems
- Integrates with Valheim's startup process

### Shader Management

- Custom shader bundle loading (`UniVrm.shaders`, `OldUniVrm.shaders`)
- Automatic shader replacement for VRM materials
- MToon shader support with brightness adjustment
- Texture processing for compatibility

### Physics Integration

- Spring bone physics simulation
- Wind effects on hair/clothing
- Collision detection for wind obstruction
- Individual wind zones per spring bone

## Key Paths

```
ValheimVRM-source/
├── Libs/                    # Some required Unity DLLs to build the mod
├── ValheimVRM/              # Main source code
│   ├── MainPlugin.cs        # BepInEx entry point
│   ├── VRM.cs               # VRM import/management
│   ├── VrmManager.cs        # Central VRM registry
│   ├── Settings.cs          # Configuration system
│   ├── VrmController.cs     # Player VRM controller
│   ├── VRMAnimationSync.cs  # Animation synchronization
│   └── [Other utility classes]
├── Assets/                  # Shader bundles and resources
├── build.bat                # Batch file to build the mod
├── build.sh                 # Bash file to build the mod
└── [Other files]
```

## Configuration Examples

The mod uses a simple key-value configuration format:

```ini
# Character settings
ModelScale=1.1
PlayerHeight=1.85
UseMToonShader=false
SpringBoneStiffness=1.0

# Global settings
ReloadInMenu=false
UseShaderBundle=current
EnableProfileCode=false
```

## Dependencies

- **BepInEx** - Modding framework
- **Harmony** - Runtime patching
- **UniVRM** - VRM format support
- **Unity Engine** - Game engine integration
- **UnityAsyncImageLoader** - Async image loading
