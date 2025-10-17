# Project Context

## Purpose
Odin - OpenSpec is a Windows desktop application for managing and working with OpenSpec specifications and documentation. The application provides a modern WinUI 3 interface for creating, editing, and managing specification documents.  I want an application that my wife and I can use to plan daily / weekly / monthly activities. It will use our personal calendars and tasks (Google) as well as our work calendars and tasks (Microsoft 365). We want to assign and track routine and non-routine tasks, maintain grocery lists. We also want to be able to view the weather for our current location as well as any other location(s) we specify, for example if we are planning a trip next week, what is the weather going to be there? We currently do not have any cohesive means of doing this. The purpose is to keep us on track and not get overwhelmed.
Everything this application uses should be free / opensource and fully contained

## Tech Stack
- **.NET 8** - Target framework for Windows applications
- **WinUI 3** - Modern Windows UI framework for native Windows apps
- **C#** - Primary programming language with nullable reference types enabled
- **XAML** - Declarative markup for UI definition
- **Windows App SDK 1.8** - Latest Windows application development platform
- **MSIX Packaging** - Modern Windows app packaging and deployment
- **State Management** - Use SQLite to manage state and store local data
- **AI** - Use a local Ollama server for AI integration

## Project Conventions

### Code Style
- **C# Naming Conventions**: PascalCase for public members, camelCase for private fields
- **Nullable Reference Types**: Enabled project-wide for better null safety
- **XAML Formatting**: Standard indentation with proper namespace declarations
- **Namespace Pattern**: Root namespace follows project name pattern (Odin___OpenSpec)

### Architecture Patterns
- **MVVM Pattern**: Follow Model-View-ViewModel pattern typical for WinUI applications
- **Single Window Application**: Currently structured as a single-window desktop app
- **Mica Backdrop**: Uses modern Windows 11 Mica material for visual hierarchy

### Testing Strategy
- Unit tests should target business logic and view models
- UI tests should focus on critical user workflows
- Consider using MSTest or xUnit for testing framework

### Git Workflow
- Feature branches for new development
- Conventional commit messages for change tracking
- Integration with OpenSpec change proposal system
- **Archon Integration**: All tasks tracked in Archon MCP server alongside local task files

### AI Assistant Workflow
- **Research Phase**: Always use Archon RAG functionality to search knowledge base before implementing features
- **Task Management**: Update both local OpenSpec task files and Archon MCP server when completing tasks
- **Knowledge Management**: Leverage Archon's project and document management for specification context
- **Dual Tracking**: Maintain task completion status in both systems for comprehensive project oversight

## Domain Context
This application is part of the OpenSpec ecosystem, which focuses on:
- **Specification Management**: Creating and maintaining technical specifications
- **Change Proposals**: Managing specification changes through structured proposals
- **Documentation Workflow**: Streamlined processes for technical documentation

## Important Constraints
- **Windows 10/11 Only**: Minimum Windows 10 version 17763 (October 2018 Update)
- **Multi-Architecture Support**: Must support x86, x64, and ARM64 platforms
- **Modern Windows Features**: Leverages Windows 11 design language and features
- **Full Trust Requirements**: Application runs with full trust permissions

## External Dependencies
- **Microsoft Windows SDK**: Build tools for Windows development
- **Windows App SDK**: Core Windows application runtime
- **SQLite**: Local data storage and state management
- **Ollama AI**: Local AI server for intelligent features
- **OpenSpec Framework**: Specification-driven development methodology
- **Archon MCP Server**: Project management, task tracking, and knowledge base integration (Project ID: b05d3b75-10a8-4320-8035-f87dba10f74c)
