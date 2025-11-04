# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Overflow is a Unity game project (Unity 6000.2.8f1) built with a custom Entity Component System (ECS) architecture. The project uses VContainer for dependency injection, UniTask for async operations, and a custom content loading system for game data.

## Build and Development

### Unity Setup
- Unity version: 6000.2.8f1
- Main scene: `Assets/Scenes/SampleScene.unity`
- Solution file: `Overflow.sln`
- .NET with C#, unsafe code enabled

### NuGet Packages
- **MessagePack** (3.1.4): Serialization
- **R3** (1.3.0): Reactive extensions
- **ZLinq** (1.5.2): High-performance LINQ - **ALWAYS use ZLinq instead of standard LINQ**
- **ZString** (2.6.0): Zero-allocation string operations

## Architecture Overview

### Core Systems

#### ECS Bootstrap and Initialization
Entry point is `GameManager` (implements `IAsyncStartable`). Initialization flow:
1. ContentManager loads game data
2. EcsBootstrap creates ECS world and systems
3. Loading screen hides after initialization

#### Dependency Injection (VContainer)
Three lifetime scopes in order:
- **CoreEarlyLifeTimeScope**: Reflection and core services
- **CoreLifeTimeScope**: Auto-registers types with `[AutoRegister]`
- **GameLifeTimeScope**: Game-specific entry points and components

Systems are auto-injected via `WorldBootstrap.onSystemCreated`.

#### Reflection System
`ReflectionManager` provides type discovery with caching. Types must have `[ReflectionInject]` attribute. Query by:
- Derived types
- Interface implementations
- Attributes
- Identifier strings (via `[Identifier]` attribute)

#### Content System
- Root: `Assets/GameAssets/`
- Folders prefixed with `#` (e.g., `#Entities`)
- Content types require:
    - `[ContentSchema("schema_name")]` attribute
    - Inherit from `AContent`
    - `id` field (string)
- JSON files loaded async on startup
- Access via `IContentManager.Get<T>(string id)` or `GetAll<T>()`

#### View System (UI/GameObject Binding)
- **Blackboard**: Key-value store with change notifications
- **AViewComponent**: Base for UI components bound to Blackboard
- **BlackboardViewParameter<T>**: Type-safe parameter binding with auto-subscription
- **LabelViewComponent<T>**: Generic text display using TextMeshPro

View lifecycle: `Reset()` → `Awake()` → `Subscribe()` → `OnDestroy()`

### Code Organization

```
Assets/Scripts/Game/
├── Core/
│   ├── Addressables/          # Asset loading
│   ├── Content/               # JSON content system
│   ├── Initialization/        # Bootstrap
│   ├── Logging/               # GameLogger
│   ├── Pooling/               # Object pooling
│   ├── Reflection/            # Type discovery
│   ├── UI/                    # View components and Blackboard
│   └── VContainer/            # DI scopes
├── Features/
│   ├── Entities/              # Entity definitions
│   ├── LoadingScreen/         # Loading UI
│   ├── Movement/              # Movement system
│   └── View/                  # Entity rendering
├── Settings/                  # Project settings
└── VContainer/                # Game scope
```

### ECS Systems
- Inherit from `SystemBase` (UnsafeEcs)
- Use `[UpdateInGroup(typeof(...))]` for update order
- Auto-discovered and VContainer-injected
- Common groups: `AllWorldInitializationSystemGroup`

### Key Attributes

**Type Registration:**
- `[ReflectionInject]` - Makes types discoverable
- `[AutoRegister]` - Auto-registers with VContainer
- `[Identifier("name")]` - String identifier association

**Content:**
- `[ContentSchema("schema")]` - Maps to folder name
- `[ContentConverter]` - Custom JSON converters

## Development Patterns

### Adding New Content Type
1. Create class inheriting `AContent`
2. Add `[ContentSchema("typename")]` and `[ReflectionInject]`
3. Include `id` field (string)
4. Create folder `Assets/GameAssets/#typename/`
5. Add JSON files

### Adding New System
1. Inherit from `SystemBase`
2. Add `[UpdateInGroup(typeof(...))]`
3. Inject dependencies with `[Inject]`
4. Override `OnAwake()` for initialization
5. Override `OnUpdate()` for per-frame logic
6. Use `CreateQuery()` for entity queries

### Adding New View Component
1. Inherit from `AViewComponent`
2. Declare `BlackboardViewParameter<T>` fields
3. Override `Subscribe()` for parameter bindings
4. Optional: Override `Reset()` for editor auto-config

### Custom JSON Converters
1. Inherit from `ContentConverter<T>`
2. Add `[ContentConverter]` and `[ReflectionInject]`
3. Implement serialization/deserialization
4. Auto-registered with JsonConverterRegistry

## Important Guidelines

### Code Quality
- **Only add code comments for unclear or complex logic** - the code should be self-documenting through clear naming and structure
- Write clear, descriptive variable and method names instead of relying on comments
- Add comments only when explaining "why" something is done, not "what" is being done

### Code Style
- **Use early returns and guard clauses** - avoid deep nesting by returning early for validation and error cases
- Keep the "happy path" at the lowest indentation level

### Performance
- **ALWAYS use ZLinq instead of standard LINQ** for all collection operations
- Use **UniTask** for async operations, not standard Task
- Use **ZString** for zero-allocation string operations
- Unsafe code used extensively for ECS performance
- Make ECS components Burst-compatible where possible

### System Design
- Content files loaded once at startup and cached
- Blackboard values use `SerializeReference` for polymorphism
- View components automatically clean up subscriptions
- All reflection queries are cached for performance