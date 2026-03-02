# Changelog

All notable changes to Clarion-GitPane will be documented in this file.

## [1.0.6] - 2026-03-02

### Security
- **Command injection prevention** — All user-supplied values passed to git/gh commands are now escaped via `EscapeGitArg()`: branch names, commit messages, stash messages, file paths, remote names, remote URLs, commit hashes, GitHub repo name and description
- **`PushChanges`** — Branch name was previously unquoted in the push command; now quoted and escaped
- **`safe.directory` path** — Directory path passed to `git config --global --add safe.directory` is now escaped
- **GitHub CLI repo creation** — `repoName` is now quoted and escaped; `description` escaping improved
- **Browser URL validation** — Remote URL opened in browser is now validated to be `http://` or `https://` only, preventing `file://`, `javascript:` or other dangerous protocols executing via `Process.Start`
- **GitKraken launch** — Changed `UseShellExecute=true` to `false` so the repo path is not interpreted by the shell

### Fixed
- **Deadlock in `ExecuteGitHubCLICommand`** — stdout and stderr are now read concurrently (same fix as `ExecuteGitCommand` in 1.0.4); sequential reads could deadlock when either buffer filled
- **`configDebounceTimer` not disposed** — `Dispose()` was cleaning up `debounceTimer` but not `configDebounceTimer`, causing a resource leak on pane close
- **Debounce timer race condition** — `FileSystemWatcher` callbacks fire on ThreadPool threads; concurrent events could race through the null check and create duplicate timers. Fixed with `Interlocked.Exchange` to atomically swap timers

### Changed
- **Branch name validation** — Replaced weak check (blocked spaces and `..` only) with strict allowlist regex `^[a-zA-Z0-9/_\-\.]+$` plus git-specific rules; prevents special characters in user-created branch names
- **`EscapeGitArg`** — Promoted from `private` to `internal static` so it can be used across partial classes

## [1.0.5] - 2026-02-17

### Added
- **Template System** - Complete .gitignore and .gitattributes template management
  - Create custom templates from scratch with name, description, and content editor
  - Import existing .gitignore/.gitattributes files and save as templates
  - Edit and delete templates through dedicated management UI
  - Set default templates for quick repository initialization
  - Character and line count display in template editor
- **Repository Initialization** - Enhanced init dialog with template selection
  - Independent selection of .gitignore and .gitattributes templates
  - Option to skip either file type during initialization
  - Preview template descriptions before selection
- **Apply Templates** - Apply templates to existing repositories
  - Apply .gitignore and/or .gitattributes templates independently
  - Three modes per file: Skip (no change), Replace (backup existing), Merge (append with header)
  - Automatic backup creation (.gitignore.backup / .gitattributes.backup)
  - Merge operations include template name and timestamp in comment header
  - Detects existing files and shows appropriate warnings
- **Default Templates** - Ships with two ready-to-use templates
  - Standard Clarion .gitignore (dll, exe, lib, obj, map, sln.cache)
  - Standard Clarion .gitattributes (linguist-language tags, binary file markers)

### Changed
- **Menu Organization** - Added "Manage .gitignore/.gitattributes Templates..." to Help menu
- **Repository Menu** - Added "Apply .gitignore/.gitattributes Template..." menu item
- **Initialize Button** - Moved to top-level menu for better discoverability (was buried in File menu)
- **Context-Aware Menus** - Only show relevant menus based on repository state
- **Menu Naming** - More explicit menu item names for better clarity

### Fixed
- **Git History Dialog** - Fixed line ending display issues (LF to CRLF conversion for Windows TextBox)
- **Dialog Layout** - Fixed DPI scaling issues in template manager using proper Panel docking

### Technical
- Template storage: JSON format in `[ClarionRoot]\accessory\addins\GitPane\templates\templates.json`
- Independent gitignore and gitattributes collections for mix-and-match flexibility
- GUID-based template identification for reliable management
- Pretty-printed JSON for human readability and version control

## [1.0.4] - 2026-02-16

### Added
- **Async Operations** - Stage All, Unstage All, and Discard All now run in background threads to prevent IDE freezing with large repositories
- **Status Indicators** - Shows "Staging all files...", "Unstaging all files...", "Discarding all changes..." messages during operations
- **Authentication Help** - Comprehensive error messages when Git authentication fails, explaining why it works in other tools but not in GitPane

### Changed
- **MenuStrip Removed** - Replaced with ToolStrip-based menu to fix layout issues caused by SharpDevelop host reparenting
- **Error Messages** - Authentication errors now explain that other Git tools may have stored credentials, and GitPane uses Git directly
- **Push Error Handling** - Changed PushChanges() to return GitCommandResult instead of bool for better error reporting
- **Button Behavior** - Stage/Unstage/Discard All buttons now disable during operation to prevent double-clicks

### Fixed
- **Layout Issues** - Resolved MenuStrip overlap with file lists by converting to ToolStrip dropdowns
- **UI Freezing** - Eliminated IDE hang when staging/unstaging large numbers of files
- **Authentication Feedback** - Users now get clear guidance on configuring Git credentials (gh auth login, SSH, PAT, Credential Manager)

### Technical
- Verified compatibility: Clarion 10, 11.1, and 12 all use .NET Framework 4.0 (ICSharpCode.Core v4.0.30319)
- All operations use .NET 4.0 compatible ThreadPool.QueueUserWorkItem for async work
- Proper UI thread marshaling with Control.Invoke for background thread updates

## [1.0.3] - 2026-02-15

### Fixed
- Assembly loading issues in Clarion IDE
- Made ICSharpCode references version-agnostic (SpecificVersion=false)
- Corrected .addin file assembly path to use subdirectory structure

## [1.0.2] - 2026-02-14

### Added
- Initial branch management features
- Git history viewer dialog
- Remote repository operations

## [1.0.1] - 2026-02-13

### Added
- File staging and unstaging
- Commit operations with message editor
- Basic push/pull functionality

## [1.0.0] - 2026-02-12

### Added
- Initial release
- Repository initialization
- File monitoring and auto-refresh
- Stage/unstage individual files
- Basic commit, push, pull operations
- Branch dropdown selector
