# Decompiled Source Code

⚠️ **This directory is excluded from version control** (see `.gitignore`)

## Contents

This directory contains decompiled .NET assemblies from the Clarion IDE installation, decompiled using ILSpy/dotPeek for research and integration purposes.

### Decompiled Assemblies:

- **SharpDevelop/** - `ICSharpCode.SharpDevelop.dll` - Core IDE framework
- **CommonSources/** - `CommonSources.dll` - Clarion-specific editor components including:
  - Embed editor (`CommonGenEditor.cs`)
  - PWEE system (`PweeLineManager.cs`, `CustomPweeLine.cs`)
  - Navigation commands
- **ClarionCore/** - `Clarion.Core.dll` - Core Clarion library
- **Clarion/** - `Clarion.exe` - Main IDE launcher

## Purpose

These decompiled sources were analyzed to understand:
- How the embed editor works
- Read-only region protection mechanisms
- Embed point data structures
- Save workflow and interface contracts
- Integration patterns for Monaco Editor replacement

## Legal Note

These are reverse-engineered binaries from a licensed Clarion installation for integration research purposes only. The original assemblies are NOT included in this repository.

## Documentation

Key findings documented in:
- `EMBED_EDITOR_FINDINGS.md`
- `EMBED_READONLY_PROTECTION.md`
- `EMBED_EDITOR_SAVE_WORKFLOW.md`
- `MONACO_EMBED_CAPABILITIES.md`
- `IN_PLACE_CODE_EDITING.md`
- `IDE_EXTENSION_GUIDE.md`

## Regenerating

To regenerate decompiled source:

```powershell
# Using ILSpy CLI (already installed)
ilspycmd "C:\Clarion\Clarion11.1\bin\ICSharpCode.SharpDevelop.dll" -p -o decompiled\SharpDevelop
ilspycmd "C:\Clarion\Clarion11.1\bin\Addins\BackendBindings\ClarionBinding\Common\CommonSources.dll" -p -o decompiled\CommonSources
```

## Git Status

✅ This directory is ignored by git (see `.gitignore` line 110)  
✅ No decompiled files will be committed to the repository  
✅ Safe to regenerate locally as needed
