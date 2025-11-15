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

Game data is defined in JSON files using a schema-based content system:

**Content Structure:**
- Content files live in `Assets/GameAssets/Content/` or `Assets/GameAssets/Entities/`
- Folders prefixed with `#` are content schemas (e.g., `#entities`, `#maps`, `#inputmodes`)
- Each content type has a corresponding C# class with `[ContentSchema("name")]` attribute
- Content objects have an `id` field and optional `properties` array

**Content Properties:**
- Properties use an identifier-based system
- Each property inherits from `AContentProperty`
- Properties are deserialized based on the `identifier` field
- Retrieved via `content.GetProperty<T>()` or `content.GetProperty<T>("IDENTIFIER")`

**Example:**
```json
{
  "id": "entity.player",
  "properties": [
    { "identifier": "VIEW", "assetPath": "..." },
    { "identifier": "MOVABLE", "baseSpeed": 5 }
  ]
}
```

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
- Content files are JSON files in `Assets/GameAssets/`
- Content is loaded at game start via `ContentManager.LoadAsync()`
- Content root path: `Assets/GameAssets` (defined in `ContentManager.ContentRootPath`)
- Folder prefix for schemas: `#` (e.g., `#entities`, `#maps`)

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
├── Content/                       # Global content (maps, input modes)
│   ├── #inputmodes/
│   └── #maps/
└── Entities/                      # Entity-specific content and prefabs
    ├── Enemies/
    └── Player/
        ├── Content/#entities/     # Player entity JSON
        └── Prefabs/               # Player prefabs
```

## Important Conventions

- **Auto-Registration**: Most services use `[AutoRegister]` instead of manual DI registration
- **Extensions over Inheritance**: Use extension interfaces for cross-cutting concerns rather than inheritance
- **Content-Driven**: Entity behavior is defined in JSON content files, not hardcoded
- **Async/UniTask**: Heavy use of UniTask for async operations (loading, entity lifecycle)
- **Reference Wrappers**: ECS uses `ReferenceWrapper<EntityManager>` for entity manager references
- **Logging**: Use `GameLogger` for consistent logging throughout the codebase
- **Early Returns**: Use early return approach for guard clauses and validation to reduce nesting
- **No Comments**: Code should be self-documenting through clear naming and structure; avoid comments in implementation
