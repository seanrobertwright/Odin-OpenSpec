# Implement Base Framework

**Change ID**: `implement-base-framework`  
**Status**: Draft  
**Created**: 2025-10-17  

## Summary

Establish the foundational application framework for Odin - OpenSpec as a fullscreen, touch-optimized personal productivity application. This framework will provide the base structure for modular components including application shell, navigation, theming, user management, and data service abstractions.

## Background

The current application is a minimal WinUI 3 application with a basic MainWindow. To support the vision of a comprehensive personal productivity application for calendar management, task tracking, and integrated services, we need a robust framework that can:

- Support fullscreen, landscape-oriented touch interfaces
- Provide modular navigation between different application areas
- Enable comprehensive theming and user profile management  
- Abstract data services for Google and Microsoft 365 integration
- Maintain state persistence across sessions

## Goals

### Primary Goals
- Create a responsive application shell optimized for touch interaction
- Implement collapsible side navigation with persistent state
- Establish comprehensive light/dark theming system
- Build user profile management with easy switching
- Design data service abstraction layer for external integrations

### Secondary Goals  
- Ensure mouse/keyboard accessibility alongside touch
- Provide on-screen keyboard integration
- Support modal dialog overlays
- Enable modular content area with tab support

## Non-Goals
- Implementation of specific modules (calendar, tasks, etc.) - these will be separate changes
- External service integrations (Google/Microsoft APIs) - framework provides abstractions only
- Advanced theming beyond light/dark color schemes
- Multi-window or split-screen layouts

## Approach

The framework will be built using WinUI 3 MVVM patterns with the following architectural components:

1. **Application Shell**: Main window structure with header, navigation, and content areas
2. **Navigation System**: Side panel with expandable/collapsible module selection  
3. **Theme Management**: Centralized theming with light/dark modes
4. **User Management**: Profile switching and user state persistence
5. **Data Services**: Abstract interfaces for external service integration

Each component will be designed as loosely coupled services that can be easily tested and extended.

## Impact Assessment

### Benefits
- Provides solid foundation for all future module development
- Enables consistent user experience across all application areas
- Simplifies integration of new modules through standard interfaces
- Supports responsive design for various screen sizes and input methods

### Risks
- Framework complexity may slow initial development
- Over-abstraction could make simple features unnecessarily complex
- Touch optimization may impact traditional desktop usability

### Migration Strategy
The current minimal MainWindow will be replaced with the new framework. No existing functionality will be lost since the current application has minimal features.

## Dependencies

- WinUI 3 and Windows App SDK 1.8
- SQLite for state persistence (already planned)
- **Archon MCP Server**: Project and knowledge management integration (Project ID: b05d3b75-10a8-4320-8035-f87dba10f74c)
- No external service dependencies for framework itself

## Validation Criteria

- [ ] Application launches in fullscreen landscape mode
- [ ] Side navigation expands/collapses and persists state
- [ ] Theme switching works between light/dark modes  
- [ ] User profiles can be created and switched
- [ ] Touch interaction works for all UI elements
- [ ] Mouse/keyboard input remains functional
- [ ] Modal dialogs display correctly over main content
- [ ] Framework supports loading placeholder modules
- [ ] **Archon Integration**: Tasks are tracked in both OpenSpec files and Archon MCP server
- [ ] **Knowledge Management**: Research workflows use Archon RAG functionality