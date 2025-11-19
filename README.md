# Overflow

A Unity 6 game project built with a custom ECS (Entity Component System) architecture, featuring data-driven content design and modern async/reactive programming patterns.

## Table of Contents

- [Overview](#overview)
- [Requirements](#requirements)
- [Getting Started](#getting-started)
- [Project Architecture](#project-architecture)
- [Key Features](#key-features)
- [Development Guide](#development-guide)
- [Content System](#content-system)
- [Core Systems](#core-systems)
- [Project Structure](#project-structure)

## Overview

**Overflow** is a 2D action game where players battle waves of enemies. The project demonstrates modern Unity development practices including:

- Custom ECS implementation using UnsafeEcs
- VContainer dependency injection with auto-registration
- Data-driven content system with JSON-based entity definitions
- Extension-based architecture for modular, reusable game systems
- Addressables for dynamic asset loading
- Job-based systems for high-performance gameplay

## Requirements

- **Unity Version**: Unity 6 (6000.2.8f1)
- **Platform**: Windows, macOS, Linux
- **IDE**: Visual Studio, Visual Studio Code, or JetBrains Rider

### Key Dependencies

The project uses the following major packages:

- **UnsafeEcs** - High-performance ECS framework
- **VContainer** - Dependency injection framework
- **UniTask** - Async/await utility for Unity
- **R3** - Reactive programming library
- **MessagePack** - Fast binary serializer
- **Unity Addressables** - Dynamic asset management
- **Unity Input System** - Modern input handling
- **Unity URP** - Universal Render Pipeline

See `Packages/manifest.json` for the complete list.

## Getting Started

### Opening the Project

1. Clone the repository
2. Open Unity Hub
3. Add the project by selecting the root folder `C:\Developer\Overflow`
4. Unity Hub will automatically download Unity 6000.2.8f1 if not installed
5. Open the project

### First Run

1. Open the **MainMenu** scene located at `Assets/Scenes/MainMenu.unity`
2. Click Play in the Unity Editor
3. From the main menu, click "Start Game" to begin gameplay
4. Use WASD or Arrow Keys to move the player
5. Press ESC to pause the game

### Building the Project

1. Go to **File → Build Settings**
2. Ensure the MainMenu and Game scenes are added to the build
3. Select your target platform
4. Click **Build** or **Build and Run**

## Project Architecture

### Assembly Structure

The project uses two main assemblies:

- **Game.asmdef** - Core runtime game logic
- **Game.Editor.asmdef** - Editor-only tools and custom inspectors

Both assemblies allow unsafe code for high-performance ECS operations.

### Design Patterns

#### Extension System

The project uses an **extension-based architecture** for cross-cutting concerns:

```csharp
[AutoRegister]
public class PlayerExtension : IEntityCreatedExtension
{
    public IReadOnlyList<IExtensionFilter> Filters => new List<IExtensionFilter>
    {
        new HasPropertyFilter<PlayerContentProperty>()
    };

    public UniTask OnEntityCreated(Entity entity, ContentEntity contentEntity)
    {
        entity.AddComponent<PlayerTag>();
        return UniTask.CompletedTask;
    }
}
```

Key extension interfaces:
- `IEntityCreatedExtension`, `IEntityDestroyedExtension` - Entity lifecycle
- `IGameStartLoadingExtension`, `IGameFinishLoadingExtension` - Loading lifecycle
- `ISessionStartExtension`, `ISessionEndExtension` - Session lifecycle
- `IGamePausedExtension`, `IGameResumedExtension` - Pause state

#### Dependency Injection

VContainer with auto-registration system:

```csharp
[AutoRegister]
public class MyService : IMyService
{
    // Automatically registered as Singleton
}

[AutoRegister(Lifetime.Transient)]
public class MyTransientService
{
    // Registered with Transient lifetime
}
```

#### ECS Architecture

Systems are organized into groups:

- **InitializationSystemGroup** - Setup systems (runs once)
- **SimulationSystemGroup** - Main game logic
  - **TimeSystemGroup** (PauseAwareSystemGroup) - Pause-aware gameplay systems
- **CleanUpSystemGroup** - Cleanup systems

Example system:

```csharp
[UpdateInGroup(typeof(TimeSystemGroup))]
[UpdateBefore(typeof(MovementSystem))]
public class PlayerMovementSystem : SystemBase
{
    private EntityQuery m_playerQuery;

    [Inject]
    private ISessionTime m_sessionTime;

    public override void OnAwake()
    {
        m_playerQuery = CreateQuery()
            .With<PlayerTag>()
            .With<Velocity>()
            .With<Speed>();
    }

    public override void OnUpdate()
    {
        var dt = m_sessionTime.DeltaTime;
        m_playerQuery.ForEach(dt, (float deltaTime, ref Entity _, ref Velocity vel, ref Speed speed) =>
        {
            // Update logic here
        });
    }
}
```

## Key Features

### Current Features

- **Player Movement** - WASD/Arrow key controls with smooth movement
- **Enemy AI** - Enemies spawn outside the screen and chase the player
- **Combat Stats** - Health system for both player and enemies
- **UI System** - Health bars, pause menu, and game UI
- **Pause System** - ESC to pause/resume gameplay
- **Wave Spawning** - Continuous enemy waves with increasing difficulty
- **Camera Following** - Smooth camera tracking of the player

### Gameplay

- Control the player using WASD or Arrow Keys
- Avoid or fight triangle-shaped enemies
- Enemies spawn every 2 seconds at random positions off-screen
- Enemies follow the player using simple AI
- Each enemy has 50 HP, player has 100 HP
- Health bars display above characters

## Development Guide

### Creating New Entities

1. Create a JSON file in `Assets/GameAssets/Content/#entities/`
2. Define entity properties:

```json
{
  "id": "entity.my.new.entity",
  "properties": [
    {
      "identifier": "VIEW",
      "assetPath": "Assets/GameAssets/Entities/MyEntity/Prefabs/MyEntity.prefab"
    },
    {
      "identifier": "MOVABLE",
      "baseSpeed": 5
    }
  ]
}
```

3. The ContentIndexGenerator automatically updates the content index
4. Spawn the entity using `IEntityFactory`:

```csharp
var entity = await m_entityFactory.CreateEntityAsync(entityManager, "entity.my.new.entity");
```

### Creating New Content Properties

1. Create a property class:

```csharp
[Identifier("MY_PROPERTY")]
public class MyContentProperty : AContentProperty
{
    public float someValue;
    public string someString;
}
```

2. Create an extension to handle the property:

```csharp
[AutoRegister]
public class MyPropertyExtension : IEntityCreatedExtension
{
    public IReadOnlyList<IExtensionFilter> Filters => new List<IExtensionFilter>
    {
        new HasPropertyFilter<MyContentProperty>()
    };

    public UniTask OnEntityCreated(Entity entity, ContentEntity contentEntity)
    {
        var property = contentEntity.GetProperty<MyContentProperty>();
        // Do something with the property
        return UniTask.CompletedTask;
    }
}
```

### Creating New Systems

1. Create a system class:

```csharp
[UpdateInGroup(typeof(TimeSystemGroup))]
public class MyGameplaySystem : SystemBase
{
    private EntityQuery m_query;

    [Inject]
    private ISessionTime m_sessionTime;

    public override void OnAwake()
    {
        m_query = CreateQuery().With<MyComponent>();
    }

    public override void OnUpdate()
    {
        var dt = m_sessionTime.DeltaTime;
        m_query.ForEach(dt, (float deltaTime, ref Entity entity, ref MyComponent component) =>
        {
            // Update logic
        });
    }
}
```

2. The system is automatically discovered and registered

### Best Practices

#### EntityQuery.ForEach Pattern

ALWAYS use `EntityQuery.ForEach` with parameters to avoid closure allocations:

```csharp
// CORRECT - No closure allocation
var dt = m_sessionTime.DeltaTime;
m_query.ForEach(dt, (float deltaTime, ref Entity entity, ref Position pos) =>
{
    pos.value += deltaTime; // Uses parameter
});

// INCORRECT - Closure allocation every frame
var dt = m_sessionTime.DeltaTime;
m_query.ForEach((ref Entity entity, ref Position pos) =>
{
    pos.value += dt; // Captures variable in closure
});
```

#### Unmanaged/Managed Separation

- Components MUST be unmanaged structs
- Use `ManagedRef<T>` to reference managed objects from components
- Use `EntityController` for complex state (dictionaries, lists)

```csharp
// Component - unmanaged struct
public struct Position : IComponent
{
    public float2 value;
}

// Managed reference in component
entity.AddReference(transform); // Creates ManagedRef<Transform>

// Querying for managed references
var query = CreateQuery().With<Position, ManagedRef<Transform>>();
query.ForEach((ref Entity _, ref Position pos, ref ManagedRef<Transform> transformRef) =>
{
    var transform = transformRef.Get();
    transform.position = new Vector3(pos.value.x, pos.value.y, 0);
});
```

#### Async and Ref Variables

C# does NOT allow `ref` in async methods. Copy before async:

```csharp
// INCORRECT
public async UniTask SpawnAsync()
{
    ref var position = ref entity.GetComponent<Position>(); // Error in async!
    await SomeTask();
}

// CORRECT
public async UniTask SpawnAsync()
{
    var position = entity.GetComponent<Position>(); // Copy first
    await SomeTask();
    entity.SetComponent(new Position { value = newValue }); // Set after async
}
```

#### Time Management

Use `ISessionTime` instead of Unity's `Time` class:

```csharp
[Inject]
private ISessionTime m_sessionTime;

public override void OnUpdate()
{
    var dt = m_sessionTime.DeltaTime; // Pause-aware, time-scaled
    var realDt = m_sessionTime.UnscaledDeltaTime; // Real-time, ignores pause
}
```

## Content System

### Content Structure

All game content is defined in JSON files:

- **Location**: `Assets/GameAssets/Content/`
- **Schema Folders**: Prefixed with `#` (e.g., `#entities`, `#maps`, `#inputmodes`)
- **Auto-Indexing**: `ContentIndexGenerator` creates `ContentIndex.json` automatically
- **Addressables Integration**: JSON files are automatically added to the "Content" addressables group

### Available Content Schemas

#### Entities (`#entities`)

```json
{
  "id": "entity.player",
  "properties": [
    { "identifier": "VIEW", "assetPath": "..." },
    { "identifier": "MOVABLE", "baseSpeed": 5 },
    { "identifier": "STATS", "stats": [...] }
  ]
}
```

#### Maps (`#maps`)

```json
{
  "id": "map.default",
  "sceneName": "Game",
  "spawnPosition": { "x": 0, "y": 0 }
}
```

#### Input Modes (`#inputmodes`)

```json
{
  "id": "inputmode.gameplay",
  "actionMap": "Player"
}
```

### Available Content Properties

- `ViewContentProperty` ("VIEW") - Entity prefab
- `PlayerContentProperty` ("PLAYER") - Player marker
- `EnemyContentProperty` ("ENEMY") - Enemy marker
- `MovableContentProperty` ("MOVABLE") - Movement speed
- `StatsContentProperty` ("STATS") - Health and stats
- `ViewComponentContentProperty` ("VIEW_COMPONENT") - UI components
- `CameraFollowingContentProperty` ("CAMERA_FOLLOWING") - Camera tracking
- `WavesGeneratorContentProperty` ("WAVES_GENERATOR") - Wave spawners

## Core Systems

### Movement System

Applies velocity to position using Burst-compiled jobs:

- `MovementSystem` - Job-based position updates
- `PlayerMovementSystem` - Input-driven player movement
- `EnemyFollowSystem` - AI that chases the player
- `TransformSyncSystem` - Syncs ECS positions to Unity Transforms

### Stats System

Health and stat management:

- `StatsController` - Manages entity stats (health, damage, etc.)
- `StatsExtension` - Initializes stats from content
- Uses `Stat` data structure with current/max values

### View System

Entity visual representation:

- `ViewExtension` - Loads and instantiates entity prefabs
- `ViewComponentExtension` - Manages UI components (health bars, etc.)
- Supports multiple UI layers (LOWER, WINDOWS, OVERLAY)

### Enemy System

Enemy spawning and AI:

- `EnemySpawnSystem` - Spawns enemies at intervals
- `EnemyFollowSystem` - Simple chase AI
- Enemies spawn in a circle 15 units from origin

### Pause System

Pause-aware time management:

- `IPauseManager` - Controls pause state
- `TimeSystemGroup` - Systems that respect pause
- `ISessionTime` - Pause-aware time tracking

## Project Structure

```
Assets/
├── Scenes/
│   ├── MainMenu.unity          # Main menu scene
│   └── Game.unity              # Main gameplay scene
├── Scripts/Game/
│   ├── Core/                   # Core framework
│   │   ├── Content/           # Content system
│   │   ├── Extensions/        # Extension system
│   │   ├── Factories/         # Entity factories
│   │   └── UI/                # UI management
│   ├── Features/              # Game features
│   │   ├── Enemies/           # Enemy systems
│   │   │   ├── AI/           # Enemy AI
│   │   │   ├── Common/       # Enemy components/tags
│   │   │   └── Spawning/     # Spawn system
│   │   ├── Players/           # Player systems
│   │   ├── Movement/          # Movement systems
│   │   ├── Stats/             # Stats system
│   │   ├── Camera/            # Camera system
│   │   ├── Pause/             # Pause system
│   │   └── View/              # Rendering
│   └── GameManager.cs         # Game coordinator
├── GameAssets/
│   ├── Content/               # JSON content files
│   │   ├── #entities/         # Entity definitions
│   │   ├── #maps/             # Map definitions
│   │   └── #inputmodes/       # Input configs
│   ├── Entities/              # Entity prefabs
│   │   ├── Player/
│   │   └── Enemies/
│   └── UI/                    # UI prefabs
│       └── HealthBar/
└── InputActions/              # Input System assets

Library/PackageCache/
└── com.iurii88.unsafeecs@*/   # UnsafeEcs package source
```

## Game Lifecycle

1. **Game Start**
   - `GameLifeTimeScope` creates root DI container
   - `GameManager.StartAsync()` creates child scope with auto-registered types
   - Execute `IGameStartLoadingExtension` hooks

2. **Loading**
   - Load content via `ContentManager`
   - Load map via `MapLoader`
   - Bootstrap ECS via `EcsBootstrap`
   - Execute `IGameFinishLoadingExtension` hooks

3. **Session Start**
   - Execute `ISessionStartExtension` hooks
   - Spawn initial entities (player, enemies)
   - Systems begin updating

4. **Gameplay Loop**
   - Systems update in group order
   - Pause-aware systems respect `IPauseManager.IsPaused`
   - Enemies spawn continuously
   - Player controls character

5. **Session End**
   - Execute `ISessionEndExtension` hooks
   - Dispose async resources
   - Return to main menu or restart

## Tips and Tricks

### Debugging

- Use `GameLogger.Log()`, `GameLogger.Warning()`, `GameLogger.Error()` for logging
- Unity's Entity Debugger shows ECS state in play mode
- Check `ContentIndex.json` if entities fail to load

### Performance

- Use job-based systems for performance-critical code
- Leverage `EntityQuery.Fetch()` + `GetComponentArray<T>()` for Burst compilation
- Avoid closures in `EntityQuery.ForEach` by passing parameters

### Content Updates

- Edit JSON files in `Assets/GameAssets/Content/`
- `ContentIndexGenerator` automatically updates on file save
- Addressables rebuilds content on play

### Common Issues

**Entity not spawning?**
- Check if content ID matches JSON filename
- Verify `ContentIndex.json` includes your entity
- Check console for loading errors

**System not updating?**
- Ensure `[UpdateInGroup]` attribute is present
- Check if system is in pause-aware group
- Verify `UpdateMask` is set correctly

**Components not found?**
- Ensure component implements `IComponent`
- Components must be unmanaged structs
- Use `ManagedRef<T>` for managed objects

## Additional Resources

- **CLAUDE.md** - Detailed architecture documentation for AI assistants
- **UnsafeEcs Documentation** - https://github.com/Iurii88/UnsafeEcs
- **VContainer Documentation** - https://github.com/hadashiA/VContainer
- **UniTask Documentation** - https://github.com/Cysharp/UniTask

## License

This project is for educational and development purposes.