# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Overflow is a Unity game project (Unity 6000.2.8f1) built with a custom Entity Component System (ECS) architecture. The project uses VContainer for dependency injection, UniTask for async operations, and a custom content loading system for game data.

## Build and Development Commands

### Unity Editor
- Open the project in Unity 6000.2.8f1
- The main scene is located in `Assets/Scenes/SampleScene.unity`
- Use Unity's standard build process (File > Build Settings)

### Solution and Projects
- Solution file: `Overflow.sln`
- Main game assembly: `Assets/Scripts/Game/Game.asmdef`
- Editor assembly: `Assets/Scripts/Game/Editor/Game.Editor.asmdef`
- The project uses .NET with C# and has `allowUnsafeCode` enabled

### NuGet Packages
The project uses NuGet packages managed via `Assets/packages.config`:
- **MessagePack** (3.1.4): Serialization
- **R3** (1.3.0): Reactive extensions
- **ZLinq** (1.5.2): High-performance LINQ
- **ZString** (2.6.0): Zero-allocation string operations

## Architecture

### Core Systems

#### 1. ECS Bootstrap and Initialization
- Entry point: `GameManager` (implements `IAsyncStartable`)
- Bootstrap: `EcsBootstrap` initializes the custom UnsafeEcs world
- Initialization order:
  1. ContentManager loads game data
  2. EcsBootstrap creates ECS world and systems
  3. Loading screen is hidden after initialization

#### 2. Dependency Injection (VContainer)
- **CoreEarlyLifeTimeScope**: Parent scope for reflection and core services
- **CoreLifeTimeScope**: Auto-registers types with `[AutoRegister]` attribute
- **GameLifeTimeScope**: Registers `GameManager` entry point and game-specific components
- Systems are automatically injected via `WorldBootstrap.onSystemCreated`

#### 3. Reflection System
- `ReflectionManager`: Provides type discovery and caching
- Types must be marked with `[ReflectionInject]` attribute to be discoverable
- Supports querying by:
  - Derived types
  - Interface implementations
  - Attributes
  - Identifier strings (via `[Identifier]` attribute)
- All queries are cached for performance

#### 4. Content System
- Content root: `Assets/GameAssets/`
- Content folders must be prefixed with `#` (e.g., `#Entities`)
- Content types must:
  - Have `[ContentSchema("schema_name")]` attribute
  - Inherit from `AContent`
  - Have an `id` field (string)
- Content files are JSON and loaded asynchronously on startup
- Access via `IContentManager.Get<T>(string id)` or `GetAll<T>()`

#### 5. View System (UI/GameObject Binding)
- **Blackboard**: Key-value store for runtime data, supports change notifications
- **AViewComponent**: Base class for UI components that bind to Blackboard
- **BlackboardViewParameter<T>**: Type-safe parameter binding with automatic subscription
- **LabelViewComponent<T>**: Generic text display component using TextMeshPro
- Lifecycle:
  1. `Reset()`: Auto-finds Blackboard reference
  2. `Awake()`: Initializes parameters and calls `Subscribe()`
  3. `Subscribe()`: Derived classes bind to Blackboard events
  4. `OnDestroy()`: Auto-disposes parameter subscriptions

### Code Organization

```
Assets/Scripts/Game/
├── Core/                           # Core systems and utilities
│   ├── Addressables/              # Asset loading via Unity Addressables
│   ├── Content/                   # JSON-based content system
│   │   ├── Attributes/           # [ContentSchema], [Identifier]
│   │   ├── Converters/           # Custom JSON converters
│   │   └── Properties/           # Content property system
│   ├── Initialization/           # Bootstrap and async loading
│   ├── Logging/                  # Custom logging (GameLogger)
│   ├── Pooling/                  # Object pooling
│   ├── Reflection/               # Type discovery and caching
│   ├── UI/                       # View components and Blackboard
│   │   ├── ViewModules/          # Reusable UI components
│   │   └── Editor/               # Custom Unity editor tools
│   └── VContainer/               # DI lifetime scopes
├── Features/                      # Game features
│   ├── Entities/                 # Entity content definitions
│   ├── LoadingScreen/            # Loading screen component
│   ├── Movement/                 # Movement system and components
│   └── View/                     # View system for rendering entities
├── Settings/                      # Project settings
└── VContainer/                    # Game lifetime scope
```

### ECS Systems
- Systems inherit from `SystemBase` (UnsafeEcs)
- Use `[UpdateInGroup(typeof(...))]` to control update order
- Systems are auto-discovered and VContainer-injected
- Common groups:
  - `AllWorldInitializationSystemGroup`: Initialization
  - Custom groups can be defined

### Custom Attributes

#### Type Registration
- `[ReflectionInject]`: Marks types for reflection system discovery
- `[AutoRegister]`: Auto-registers with VContainer as singleton
- `[Identifier("name")]`: Associates string identifier with type

#### Content
- `[ContentSchema("schema")]`: Maps content type to folder name (e.g., `#Entities`)
- `[ContentConverter]`: Marks custom JSON converters

## Development Patterns

### Adding a New Content Type
1. Create class inheriting from `AContent`
2. Add `[ContentSchema("typename")]` attribute
3. Add `[ReflectionInject]` attribute
4. Include `id` field (string)
5. Create folder `Assets/GameAssets/#typename/`
6. Add JSON files with content data

### Adding a New System
1. Inherit from `SystemBase`
2. Add `[UpdateInGroup(typeof(...))]` attribute
3. Inject dependencies with `[Inject]` attribute
4. Override `OnAwake()` for initialization
5. Override `OnUpdate()` for per-frame logic
6. Use `CreateQuery()` to query entities

### Adding a New View Component
1. Inherit from `AViewComponent`
2. Declare `BlackboardViewParameter<T>` fields
3. Override `Subscribe()` to bind to parameter changes
4. Optional: Override `Reset()` to auto-configure in editor
5. Component auto-initializes and disposes subscriptions

### Custom JSON Converters
1. Inherit from `ContentConverter<T>`
2. Add `[ContentConverter]` attribute
3. Add `[ReflectionInject]` attribute
4. Implement serialization/deserialization logic
5. Converter is auto-registered with JsonConverterRegistry

## Important Notes

- The project uses **unsafe code** extensively for ECS performance
- Use **UniTask** for async operations, not standard Task
- Use **ZString** for zero-allocation string operations
- All ECS component operations should be **Burst-compatible** where possible
- Content files are loaded once at startup and cached
- Blackboard values use `SerializeReference` for polymorphism
- View components automatically clean up subscriptions
