# Design: Base Framework Implementation

**Change ID**: `implement-base-framework`

## Architecture Overview

The base framework follows a modular, service-oriented architecture using WinUI 3 MVVM patterns. The design prioritizes touch-first interaction while maintaining mouse/keyboard accessibility.

```
┌─────────────────────────────────────────────────────────────┐
│                    Application Shell                         │
├─────────────────────────────────────────────────────────────┤
│  Header: User Profile | Content Title | Theme | Settings    │
├───────────────┬─────────────────────────────────────────────┤
│   Navigation  │              Content Area                   │
│   Sidebar     │                                             │
│   - Home      │         ┌─────────────────────────┐         │
│   - Calendar  │         │     Module Content      │         │
│   - Tasks     │         │                         │         │  
│   - Grocery   │         │   (Tabs if needed)      │         │
│   - Weather   │         │                         │         │
│   - Music     │         └─────────────────────────┘         │
│   - Settings  │                                             │
└───────────────┴─────────────────────────────────────────────┘
```

## Core Services

### 1. Shell Service (`IShellService`)
Manages the overall application layout and window state.

**Responsibilities:**
- Window initialization and fullscreen management
- Layout orchestration between header, navigation, and content
- Touch/input mode detection and adaptation

**Key Components:**
- `ShellView` (UserControl) - Main layout container
- `ShellViewModel` - Coordinates shell behavior
- `InputModeManager` - Detects and adapts to touch vs mouse input

### 2. Navigation Service (`INavigationService`)  
Handles module selection and content area management.

**Responsibilities:**
- Side navigation state management (expand/collapse)
- Module registration and routing
- Content area view transitions
- Navigation state persistence

**Key Components:**
- `NavigationView` (UserControl) - Side panel with module buttons
- `NavigationViewModel` - Navigation state and commands
- `IModule` interface - Contract for pluggable modules
- `ModuleRegistry` - Manages available modules

### 3. Theme Service (`IThemeService`)
Provides centralized theming and visual state management.

**Responsibilities:**
- Light/dark theme switching
- Theme persistence across sessions  
- Dynamic resource updating
- Touch-optimized sizing adjustments

**Key Components:**
- `ThemeManager` - Core theming logic
- Theme resource dictionaries (Light.xaml, Dark.xaml)
- `TouchSizeConverter` - Adaptive sizing for touch interfaces

### 4. User Service (`IUserService`)
Manages user profiles and authentication state.

**Responsibilities:**
- User profile creation and management
- Profile switching and persistence
- User-specific settings and preferences
- Profile photo and display name handling

**Key Components:**
- `UserProfile` model - User data structure  
- `ProfileManager` - Profile CRUD operations
- `UserSwitchView` (UserControl) - Profile selection UI

### 5. Data Service Abstractions
Abstract interfaces for external service integration.

**Responsibilities:**
- Define contracts for calendar, task, and external data sources
- Provide local caching and offline support patterns
- Abstract authentication and API client management

**Key Interfaces:**
- `ICalendarService` - Calendar data operations
- `ITaskService` - Task and todo management  
- `IWeatherService` - Weather data retrieval
- `IMusicService` - Music streaming integration
- `IDataSyncService` - Cross-service synchronization

## Touch Optimization Strategy

### Input Adaptation
- **Touch Targets**: Minimum 44x44 logical pixels for all interactive elements
- **Gesture Support**: Pan, pinch, and tap gestures where appropriate
- **On-Screen Keyboard**: Integration with Windows touch keyboard
- **Hover States**: Adapt hover effects for touch (show on tap, hide on next interaction)

### Layout Responsive Design  
- **Landscape Primary**: Optimized for 16:9 and 16:10 landscape orientations
- **Flexible Sizing**: Content adapts to different screen resolutions
- **Safe Areas**: Respect screen bezels and system UI areas
- **Dense Information**: Efficient use of screen real estate for productivity tasks

## State Management Architecture

### Local Storage (SQLite)
```sql
-- User profiles and preferences
Users (id, name, photo_path, is_active, created_date)
UserPreferences (user_id, key, value, data_type)

-- Navigation and UI state  
NavigationState (user_id, is_expanded, last_module, updated_date)
ThemeState (user_id, theme_name, custom_settings, updated_date)

-- Module-specific state tables (defined by individual modules)
ModuleState (user_id, module_name, state_data, updated_date)
```

### Memory State Management
- **Service Singletons**: Core services registered as singletons in DI container
- **ViewModels**: Per-view lifecycle with automatic cleanup
- **Event Aggregation**: Loosely coupled communication between services
- **Configuration**: Centralized app settings with change notification

## Error Handling and Resilience

### Framework Error Handling
- **Service Exceptions**: Wrapped in framework-specific exception types
- **UI Error Boundaries**: Error recovery at view model level
- **Logging Integration**: Structured logging for diagnostics
- **Graceful Degradation**: Core functionality remains available when services fail

### Data Resilience
- **Local Caching**: All external data cached locally for offline access
- **Sync Conflict Resolution**: Strategies for handling data conflicts
- **State Recovery**: Application state recovery on crash/restart
- **Validation**: Input validation at service boundaries

## Performance Considerations

### Startup Performance
- **Lazy Loading**: Services initialized on-demand where possible
- **Module Discovery**: Asynchronous module registration
- **UI Rendering**: Virtualization for large data sets in modules

### Runtime Performance  
- **Memory Management**: Proper disposal of resources and event subscriptions
- **Background Processing**: Long-running operations off UI thread
- **Touch Responsiveness**: Sub-100ms response time for touch interactions
- **Battery Efficiency**: Minimize background activity and CPU usage

## Technology Choices

### UI Framework Decisions
- **WinUI 3**: Native Windows performance with modern XAML features
- **MVVM Toolkit**: Microsoft.Toolkit.Mvvm for view model infrastructure
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **SQLite**: Microsoft.Data.Sqlite for local storage

### Architecture Patterns
- **Service Layer**: Clear separation between UI and business logic
- **Repository Pattern**: Data access abstraction
- **Command Pattern**: UI actions as discrete, testable commands
- **Observer Pattern**: Event-driven communication between services

This design provides a solid foundation that can evolve with the application's needs while maintaining clear separation of concerns and testability.