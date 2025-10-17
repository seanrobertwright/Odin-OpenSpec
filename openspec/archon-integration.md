# Archon MCP Integration Configuration

## Project Details
- **Project ID**: `b05d3b75-10a8-4320-8035-f87dba10f74c`
- **Project Name**: Odin - OpenSpec
- **Created**: 2025-10-17

## Integration Guidelines

### For AI Assistants
When working on this project, always follow these patterns:

#### Before Implementation Research
1. **Check Archon Knowledge Base**: Use `mcp_archon_rag_search_knowledge_base()` to research relevant patterns, examples, and documentation
2. **Search Code Examples**: Use `mcp_archon_rag_search_code_examples()` for implementation patterns
3. **Review Project Context**: Use `mcp_archon_find_projects()` and `mcp_archon_get_project_features()` to understand current state

#### Task Management Workflow
1. **Check Current Tasks**: Use `mcp_archon_find_tasks()` to see active tasks
2. **Update Task Status**: When starting work, use `mcp_archon_manage_task()` to set status to "doing"
3. **Complete Tasks**: When finished, update both:
   - Local OpenSpec task files (check boxes to `[x]`)
   - Archon task status to "review" or "done"

#### Dual Tracking Pattern
- **Local Files**: Maintain OpenSpec task completion in `tasks.md` files
- **Archon Server**: Keep parallel task tracking for project oversight
- **Consistency**: Ensure both systems reflect the same completion state

### Task Status Mapping
- OpenSpec `[ ]` → Archon `todo`
- OpenSpec `[x]` → Archon `done`
- Working on task → Archon `doing`
- Ready for review → Archon `review`

### Knowledge Base Usage
- Search before implementing new patterns
- Research WinUI 3 and .NET 8 examples
- Look for MVVM and touch interface patterns
- Check for similar application architectures

## Current Tasks in Archon
1. Project Structure and Dependencies (ID: 384b84fc-9180-4f34-82ef-09be91cdc1bd)
2. Basic Data Layer and SQLite Setup (ID: d6937217-2102-4173-9520-a2159c4c6605)
3. Core Service Interfaces (ID: 86c86b63-394a-4a14-ab6e-47d4b05e7aa1)
4. Theme System Foundation (ID: 06db8958-9036-43a4-9767-3c5e8211d426)

Additional tasks will be created as implementation progresses through the OpenSpec phases.