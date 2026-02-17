# Clarion-GitPane

A visual Git integration add-in for the Clarion IDE, providing simple and intuitive Git operations directly within your development environment.

## Overview

Clarion-GitPane adds a dockable pane to the Clarion IDE that enables essential Git operations without leaving your IDE. It automatically syncs with your solution directory and provides a clean, professional interface for version control.

## Features

### Core Git Operations
- **Initialize Repository** - Create new Git repositories with template selection
- **Commit** - Stage files and commit changes with messages
- **Push/Pull/Fetch** - Sync with remote repositories
- **Stage/Unstage** - Granular file staging control
- **Discard Changes** - Revert unwanted modifications

### Template System
- **Template Management** - Create, edit, and organize .gitignore and .gitattributes templates
- **Import Templates** - Import existing .gitignore/.gitattributes files from any repository
- **Template Editor** - Full-featured editor with character/line count and description
- **Default Templates** - Set preferred templates for quick initialization
- **Apply to Existing Repos** - Apply templates with Skip/Replace/Merge options
- **Template Storage** - Templates persist in JSON format for easy version control

### Branch Management
- **Quick Branch Switching** - Dropdown selector for fast branch changes
- **Create Branches** - Create new branches from current HEAD
- **Delete Branches** - Remove local branches (with force option)
- **Merge Branches** - Merge branches with conflict detection

### Remote Management
- **Add/Remove Remotes** - Manage repository remotes
- **View on Remote** - Open repository in browser (GitHub/GitLab)
- **External Tool Integration** - Launch GitHub Desktop or GitKraken

### UI Features
- **Git History Viewer** - Browse commit history with details
- **File Watcher** - Auto-refresh on external file changes
- **Context-Aware Menus** - UI adapts to repository state
- **Professional ToolStrips** - Clean, modern interface
- **Uncommitted Changes Handling** - Prompts to stash/commit before operations

## Requirements

- **Clarion IDE** (SharpDevelop 2.1 fork)
- **.NET Framework 4.0** or higher
- **Git** installed on your system (available in PATH)
- **Windows** operating system

## Authentication with GitHub and Other Git Hosts

GitPane uses Git directly and cannot prompt for credentials interactively. If you see authentication errors when pushing, pulling, or fetching, this is likely because Git needs credentials to be configured.

### Why Authentication May Fail

Other Git tools (GitHub Desktop, Visual Studio, etc.) may work fine because they have credentials stored and supply them automatically. GitPane cannot prompt for credentials, so Git must have access to them through one of these methods:

### Recommended Solutions

1. **GitHub CLI (Easiest for GitHub users)**
   ```bash
   gh auth login
   ```
   This configures credentials once and Git will use them automatically.

2. **SSH Keys (Recommended for all Git hosts)**
   - Generate an SSH key: `ssh-keygen -t ed25519`
   - Add the public key to your GitHub/GitLab/Bitbucket account
   - Clone/configure repositories with SSH URLs (`git@github.com:user/repo.git`)
   - Or convert existing HTTPS repos to SSH:
     ```bash
     git remote set-url origin git@github.com:user/repo.git
     ```

3. **Personal Access Token (PAT)**
   - Generate a token in your Git host settings (GitHub: Settings → Developer settings → Personal access tokens)
   - Use the token as your password when Git prompts (first time only if using credential storage)

4. **Git Credential Manager**
   - Download from: https://github.com/git-ecosystem/git-credential-manager
   - Stores credentials securely and supplies them automatically

### Testing Your Configuration

After configuring authentication, test with:
```bash
git fetch
```

If successful, GitPane will work without authentication errors.

## Building

### Prerequisites
- .NET SDK supporting .NET Framework 4.0 compilation
- C# 7.3 compatible compiler

### Build Steps

```bash
# Clone the repository
git clone https://github.com/msarson/Clarion-GitPane.git
cd Clarion-GitPane

# Build the project
dotnet build GitPane.csproj --configuration Release
```

The compiled DLL will be located at: `bin\Release\net40\GitPane.dll`

## Installation

1. **Build the project** (see above) or download a release
2. **Copy the DLL** to your Clarion IDE add-ins directory:
   ```
   [ClarionRoot]\accessory\addins\GitPane\GitPane.dll
   ```
   Note: The subdirectory structure is required by the Clarion IDE

3. **Copy the .addin file** to the same location:
   ```
   [ClarionRoot]\accessory\addins\GitPane\GitPane.addin
   ```

4. **Restart the Clarion IDE**

5. **Open the pane** via the View menu or IDE pane manager

## Usage

### Basic Workflow

1. **Open a Solution** - GitPane automatically detects the solution directory
2. **Initialize or Open Repository** - Create a new repo or work with an existing one
3. **Make Changes** - Edit files in your IDE as normal
4. **Stage Files** - Select files in the Unstaged list and click "Stage Selected"
5. **Commit** - Enter a commit message and click "Commit"
6. **Push** - Click "Push" to send commits to remote

### Switching Branches

- Click the **branch dropdown** in the toolbar
- Select a branch from the list to switch
- Use **"More branch options..."** for advanced features (create, delete, merge)

### Menu Features

- **Initialize Repository** → When no repo exists (top-level button)
- **File** → Open in external tools, close pane
- **Repository** → Refresh, Fetch, Pull, Push, Apply Templates, View on Remote
- **Branch** → Create, Delete, Merge branches
- **View** → Toggle UI elements, reset layout
- **Help** → Manage Templates, About and diagnostics

### Template System Usage

#### Managing Templates

1. **Open Template Manager** - Help → Manage .gitignore/.gitattributes Templates...
2. **Create New Template** - Click "New" button on either tab
3. **Import Template** - Click "Import" to load an existing .gitignore or .gitattributes file
4. **Edit Template** - Select a template and click "Edit" (or double-click)
5. **Set Default** - Select a template and click "Set Default" for automatic selection during init
6. **Delete Template** - Select a template and click "Delete"

#### Initializing with Templates

1. **Click "Initialize Repository"** button when no repo exists
2. **Select Templates** - Choose from dropdowns for .gitignore and .gitattributes (or select "None")
3. **Click "Initialize"** - Repository is created with selected template files

#### Applying Templates to Existing Repository

1. **Repository → Apply .gitignore/.gitattributes Template...**
2. **Select Templates** - Choose templates to apply (shows "(None)" option to skip)
3. **Choose Action** for files that already exist:
   - **Skip** - Don't modify existing file
   - **Replace** - Replace file (backup created as .gitignore.backup)
   - **Merge** - Append template content with timestamp header
4. **Click "Apply"** - Templates are applied with chosen actions

## Project Structure

```
GitPane/
├── GitPanePad.cs                      # Main pad logic
├── GitPanePad.Designer.cs             # UI initialization
├── GitPanePad.BranchManagement.cs     # Branch operations
├── GitPanePad.CommitOperations.cs     # Commit/stage logic
├── GitPanePad.FileSystemWatchers.cs   # File monitoring
├── GitPanePad.RemoteManagement.cs     # Remote handling
├── GitPanePad.RepositoryDialogs.cs    # Dialogs and prompts
├── GitRepository.cs                   # Git command wrapper
├── GitTemplate.cs                     # Template data model
├── TemplateManager.cs                 # Template CRUD and persistence
├── TemplateEditorDialog.cs            # Template editor UI
├── TemplateManagerDialog.cs           # Template management UI
├── InitializeRepositoryDialog.cs      # Enhanced init dialog
├── ApplyTemplateDialog.cs             # Apply template dialog
├── BranchSelectorDialog.cs            # Branch selection UI
├── GitHistoryDialog.cs                # History viewer
└── GitPane.addin                      # Add-in manifest
```

## Technology Stack

- **Language:** C# 7.3
- **Framework:** .NET Framework 4.0
- **UI:** Windows Forms
- **IDE Integration:** SharpDevelop 2.1 API (`AbstractPadContent`)
- **Git Operations:** Command-line Git wrapper

## Development Notes

### Clarion IDE Environment
- Based on SharpDevelop 2.1 (circa 2007)
- Uses `AbstractPadContent` base class for pads
- Limited to .NET 4.0 and C# 7.3 features
- Windows Forms only (no WPF)

### Deployment
The Clarion IDE requires add-ins to be in a subdirectory under `accessory\addins\`. The add-in manifest (`.addin` file) specifies the assembly name and entry point.

### Code Organization
The project uses partial classes to separate concerns:
- **Designer** - UI construction
- **BranchManagement** - Branch operations
- **CommitOperations** - Staging and commits
- **FileSystemWatchers** - Auto-refresh logic
- **RemoteManagement** - Remote handling
- **RepositoryDialogs** - User prompts

## Contributing

Contributions are welcome! Please:
1. Fork the repository
2. Create a feature branch
3. Make your changes with clear commit messages
4. Test thoroughly in the Clarion IDE
5. Submit a pull request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

You are free to use, modify, and distribute this software for any purpose, commercial or non-commercial.

## Author

Built with assistance from GitHub Copilot CLI.

## Support

For issues, questions, or suggestions, please open an issue on GitHub:
https://github.com/msarson/Clarion-GitPane/issues

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for detailed release notes and version history.
