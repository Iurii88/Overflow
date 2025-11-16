# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Overflow** is a Unity 6 (6000.2.8f1) game project built with a custom ECS (Entity Component System) architecture using the UnsafeEcs library. The project uses VContainer for dependency injection and implements a data-driven content system with JSON-based entity definitions.

## Key Architecture Patterns

### Assembly Structure

The project uses two main assemblies:
- **Core** (Game.asmdef): Runtime game logic with dependencies on UnsafeEcs, VContainer, UniTask, R3, MessagePack, and custom libraries (ZLinq, ZString)
- **Game.Editor** (Game.Editor.asmdef): Editor-only tools and custom inspectors

Both assemblies allow unsafe code and override references to include precompiled DLLs.

### Extension System

The project uses an **extension-based architecture** for cross-cutting concerns and lifecycle hooks. Extensions are interfaces implementing `IExtension` that are executed via `IExtensionExecutor`.

Key extension interfaces:
- `IEntityCreatedExtension`, `IEntityDestroyedExtension` - Entity lifecycle
- `IGameStartLoadingExtension`, `IGameFinishLoadingExtension`, `IGameLoadProgressExtension` - Loading lifecycle
- `ISessionStartExtension`, `ISessionEndExtension` - Session lifecycle
- `IGamePausedExtension`, `IGameResumedExtension` - Pause state
- `IFilterableExtension` - Extensions that can be filtered by entity/content

Extensions are:
- Auto-registered via `[AutoRegister]` attribute
- Cached and sorted by priority using `[ExtensionPriority(int)]` attribute
- Executed in priority order (lower values execute first)
- Can be filtered using `IExtensionFilter` implementations

### Dependency Injection & Auto-Registration

The project uses VContainer with an auto-registration system:
- Classes marked with `[AutoRegister]` are automatically registered in the DI container
- `[AutoRegister(Lifetime.X)]` controls the lifetime (default: Singleton)
- Registration happens in `GameManager.AutoRegistration()` via reflection
- The `GameLifeTimeScope` is the root scope, with child scopes created per game session

### Content System

Game data is defined in JSON files using a schema-based content system with automatic indexing and addressables integration.

**Content Structure:**
- All content JSON files are in `Assets/GameAssets/Content/` organized by schema folders
- Folders prefixed with `#` are content schemas (e.g., `#entities`, `#maps`, `#inputmodes`)
- Each content type has a corresponding C# class with `[ContentSchema("name")]` attribute
- Content objects have an `id` field and optional `properties` array
- Related assets (prefabs, sprites) are stored in `Assets/GameAssets/Entities/` or `Assets/GameAssets/UI/`

**Content Loading Pipeline:**
1. `ContentIndexGenerator` (editor tool) scans all `#*` folders and auto-generates `ContentIndex.json`
2. JSON files are automatically added to the "Content" addressables group
3. `ContentManager.LoadAsync()` loads the index and deserializes all content via addressables
4. Content is cached by type and ID for fast retrieval via `contentManager.Get<T>(id)`

**Content Properties:**
- Properties use an identifier-based system for polymorphic deserialization
- Each property inherits from `AContentProperty` with `[Identifier("NAME")]` attribute
- Properties are deserialized based on the `identifier` field in JSON
- Retrieved via `content.GetProperty<T>()` or `content.GetProperty<T>("IDENTIFIER")`
- Extension filters can target entities with specific properties using `HasPropertyFilter<T>`

**Example Content File** (`Assets/GameAssets/Content/#entities/entity.player.json`):
```json
{
  "id": "entity.player",
  "properties": [
    {
      "identifier": "VIEW",
      "assetPath": "Assets/GameAssets/Entities/Player/Prefabs/Player.prefab"
    },
    {
      "identifier": "MOVABLE",
      "baseSpeed": 5
    },
    {
      "identifier": "STATS",
      "stats": [
        { "id": "HEALTH", "value": 100, "max": 100 }
      ]
    }
  ]
}
```

**Available Content Schemas:**
- `ContentEntity` (schema: "entities") - Entity definitions with properties
- `ContentMap` (schema: "maps") - Map/level definitions with scene and spawn position
- `ContentInputMode` (schema: "inputmodes") - Input configuration with action maps

**Available Content Properties:**
- `ViewContentProperty` ("VIEW") - Prefab asset path for entity visuals
- `MovableContentProperty` ("MOVABLE") - Movement speed configuration
- `StatsContentProperty` ("STATS") - Health and other stat definitions
- `ViewComponentContentProperty` ("VIEW_COMPONENT") - UI components with layer and state
- `PlayerContentProperty` ("PLAYER") - Marker for player entities
- `WavesGeneratorContentProperty` ("WAVES_GENERATOR") - Marker for wave spawners

### ECS Architecture

The project uses UnsafeEcs with custom system groups:

**System Groups:**
- `InitializationSystemGroup` - Runs first, for setup systems
- `SimulationSystemGroup` - Main game logic
  - `PauseAwareSystemGroup` - Systems that respect pause state (child of SimulationSystemGroup)
- `CleanUpSystemGroup` - Runs last, for cleanup

**System Registration:**
- Systems inherit from `SystemBase` (or `SystemGroup` for groups)
- Marked with `[UpdateInGroup(typeof(GroupType))]`
- Automatically discovered and registered via reflection

**Pause System:**
- `PauseAwareSystemGroup` skips execution when `IPauseManager.IsPaused` is true
- Game systems should be in `PauseAwareSystemGroup` to respect pause
- Initialization and cleanup systems typically stay in their respective groups

### Game Lifecycle

**Initialization Flow:**
1. `GameLifeTimeScope` creates root DI container
2. `GameManager.StartAsync()` creates child scope with auto-registered types
3. Execute `IGameStartLoadingExtension` hooks
4. Load content via `ContentManager`
5. Load map via `MapLoader`
6. Bootstrap ECS via `EcsBootstrap`
7. Execute `IGameFinishLoadingExtension` hooks
8. Execute `ISessionStartExtension` hooks
9. Spawn initial entities (e.g., player)

**Entity Creation:**
- `IEntityFactory.CreateEntityAsync(entityManager, contentId)` creates entities from content
- Entity gets a reference to its `ContentEntity`
- `IEntityCreatedExtension` hooks execute for each property
- Properties can add components, load prefabs, etc.

**Session Management:**
- `GameManager.RestartAsync()` - Disposes child scope, re-runs initialization
- `GameManager.GoToMainMenuAsync()` - Executes cleanup, loads MainMenu scene
- Child scope contains all per-session services marked with `IUniTaskAsyncDisposable`

## Common Development Commands

### Unity Operations
Since this is a Unity project, most operations happen within the Unity Editor. There are no build scripts or test runners in this repository.

### Working with Content
- Content files are JSON files in `Assets/GameAssets/Content/` organized by schema folders
- Schema folders are prefixed with `#` (e.g., `#entities`, `#maps`, `#inputmodes`)
- The `ContentIndexGenerator` automatically generates `ContentIndex.json` when content files change
- Manual regeneration: Unity menu → Tools/Content/Generate Content Index
- Content is loaded at game start via `ContentManager.LoadAsync()` using addressables
- Retrieve content via `contentManager.Get<ContentType>("content.id")` or `contentManager.GetAll<ContentType>()`

**Adding New Content:**
1. Create JSON file in appropriate schema folder (e.g., `#entities/entity.newenemy.json`)
2. ContentIndexGenerator automatically updates `ContentIndex.json` and addressables
3. Content becomes available after next game start

### Working with Extensions
When creating a new extension:
1. Define interface inheriting from `IExtension` (or `IFilterableExtension`)
2. Implement extension class with `[AutoRegister]`
3. Add `[ExtensionPriority(int)]` if execution order matters
4. Extension will be auto-discovered and executed by `ExtensionExecutor`

### Working with Systems
When creating a new ECS system:
1. Inherit from `SystemBase`
2. Add `[UpdateInGroup(typeof(GroupType))]` attribute
3. Use `[Inject]` for dependencies (resolved via VContainer)
4. Override `UpdateMask` to control when system updates
5. System will be auto-discovered by UnsafeEcs bootstrap

## Code Organization

```
Assets/Scripts/Game/
├── Core/                          # Core framework code
│   ├── Addressables/              # Addressables management
│   ├── Content/                   # Content system (JSON-based data)
│   ├── Extensions/                # Extension system
│   ├── Factories/                 # Entity and object factories
│   ├── Initialization/            # Async loader infrastructure
│   ├── Input/                     # Input mode system
│   ├── Lifecycle/                 # Entity lifecycle extensions
│   ├── Reflection/                # Reflection utilities and auto-registration
│   ├── UI/                        # UI layer management
│   └── VContainer/                # VContainer setup
├── Features/                      # Game features
│   ├── Bootstraps/                # ECS bootstrap
│   ├── Entities/                  # Entity content definitions
│   ├── LoadingScreen/             # Loading screen and progress
│   ├── MainMenu/                  # Main menu scene
│   ├── Maps/                      # Map loading and content
│   ├── Movement/                  # Movement systems
│   ├── Pause/                     # Pause system and input
│   ├── Players/                   # Player-specific logic
│   ├── Stats/                     # Stats system (health, etc.)
│   └── View/                      # View/rendering systems
├── GameManager.cs                 # Main game coordinator
└── InitializationSystem.cs        # Initial entity spawning

Assets/GameAssets/
├── Content/                       # All content JSON files
│   ├── #entities/                 # Entity definitions (player, enemies)
│   ├── #inputmodes/               # Input mode configurations
│   ├── #maps/                     # Map/level definitions
│   └── ContentIndex.json          # Auto-generated content index
├── Entities/                      # Entity-related assets (prefabs, sprites)
│   ├── Enemies/
│   │   └── Triangle/Prefabs/
│   └── Player/Prefabs/
└── UI/                            # UI prefabs and assets
    └── HealthBar/
```

## Important Conventions

- **Auto-Registration**: Most services use `[AutoRegister]` instead of manual DI registration
- **Extensions over Inheritance**: Use extension interfaces for cross-cutting concerns rather than inheritance
- **Content-Driven**: Entity behavior is defined in JSON content files, not hardcoded
- **Async/UniTask**: Heavy use of UniTask for async operations (loading, entity lifecycle)
- **Reference Wrappers**: ECS uses `ReferenceWrapper<EntityManager>` for entity manager references
- **Delta Time**: Use `IGameDeltaTime` (injected via VContainer) instead of `Time.deltaTime` in ECS systems for pause-aware time tracking
- **Logging**: Use `GameLogger.Log()`, `GameLogger.Warning()`, and `GameLogger.Error()` for all logging.
- **Early Returns**: Use early return approach for guard clauses and validation to reduce nesting
- **No Comments**: Code should be self-documenting through clear naming and structure; avoid comments in implementation
