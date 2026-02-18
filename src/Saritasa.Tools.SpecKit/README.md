# Saritasa.Tools.SpecKit

A production-ready .NET tool package for managing Spec Kit artifacts with MCP (Model Context Protocol) server functionality.

## Features

- 🚀 **Dotnet Tool** - Install globally or per-project
- 📦 **Artifact Management** - Automated deployment of spec kit files
- 🤖 **MCP Integration** - AI-friendly functions decorated with `AIFunction`
- 🛠️ **Production Ready** - Service-based architecture with proper logging
- 📝 **Template Management** - Access to spec, plan, and tasks templates

## Installation

### Global Installation
```bash
dotnet tool install -g Saritasa.Tools.SpecKit
```

### Local Installation
```bash
dotnet tool install Saritasa.Tools.SpecKit
```

## Usage

### Install Spec Kit Artifacts

Deploy spec kit artifacts to your project:

```bash
# Install to current directory
speckit install

# Install to specific directory
speckit install /path/to/your/project
```

**What gets installed:**
- **`.github/agents/`** - 8 agent files (force updated)
- **`.github/prompts/`** - 8 prompt files (force updated)
- **`.speckit/memory/`** - Constitution and project index (preserved if exist)
- **`.speckit/templates/`** - Business doc, tech doc, plan, spec, tasks templates (updated)
- **`.speckit/README.md`** - Spec Kit documentation (updated)

### MCP Server

The package provides a Model Context Protocol (MCP) server that exposes spec kit management tools to LLM clients:

```bash
speckit mcp
```

This starts an MCP server that exposes the following tools:
- **`setup_spec`** - Gets the spec template content for creating a new specification document
- **`setup_plan`** - Gets the plan template content for creating a new implementation plan
- **`setup_tasks`** - Gets the tasks template content for creating a new tasks list
- **`get_spec_files`** - Lists all specification files in the `.speckit` directory

The MCP server runs as a hosted service and communicates via stdio, following the MCP protocol specification.

## Architecture

### Services

**`GitService`** - Abstraction for Git operations:
- Get revision count
- Get short/long commit hash
- Async operations with cancellation support

**`ArtifactsService`** - Manages spec kit artifacts:
- Install artifacts to target directory
- Get template contents
- List spec files
- Async file operations

### Commands

**`InstallCommand`** - Deploys artifacts to target project

**`McpCommand`** - Runs the MCP server that exposes spec kit tools

### MCP Server

**`SpecKitMcpServer`** - Implements the Model Context Protocol server:
- Uses `AIFunctionFactory` to create MCP-compatible tools
- Exposes 4 tools for spec management
- Runs as a hosted service with proper logging

## Technical Details

- **Framework**: .NET 8.0
- **Command Line**: McMaster.Extensions.CommandLineUtils
- **AI Integration**: Microsoft.Extensions.AI.Abstractions
- **Logging**: Microsoft.Extensions.Logging with console output
- **Architecture**: Service-based with dependency injection

## What's Included

### Agents (.github/agents)
- Constitution Agent
- Analyze Agent
- Clarify Agent
- Document Agent
- Implement Agent
- Plan Agent
- Specify Agent
- Tasks Agent

### Prompts (.github/prompts)
Corresponding prompts for all agents listed above.

### Spec Kit Artifacts (.speckit)
- **Memory files**: Constitution, project index (preserved on install)
- **Templates**: Business doc, tech doc, plan, spec, tasks
- **README**: Spec Kit workflow documentation

## Development

This tool is part of the Saritasa.Tools suite. For development instructions, see the main repository README.

## License

BSD License - See LICENSE.txt in the root of the repository.
