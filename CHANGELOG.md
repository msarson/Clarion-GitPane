# Changelog

All notable changes to Clarion-GitPane will be documented in this file.

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
