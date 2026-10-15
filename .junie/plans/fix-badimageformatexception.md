---
sessionId: session-261014-143428-tjtg
---

# Requirements

### Overview & Goals
The game throws a `System.BadImageFormatException` when trying to spawn `TornMap.exe` (and other subprograms). This is a regression caused by migrating the projects to .NET 9. 

In older .NET Framework versions, an `.exe` file was a managed .NET assembly containing all the code, which `Mono.Cecil` could parse and modify to inject clues. In modern .NET, an `.exe` is a native unmanaged launcher. 

The user has explicitly requested that subprograms (like `TornMap`) be small, self-contained single-file `.exe`s without adjacent `.dll` or `.json` files. 

Because `Mono.Cecil` cannot parse a single-file `.exe` bundle, we will remove `Mono.Cecil` usage. Instead, `TreasureManager` will write out the single-file `.exe` and append the clue payload (as binary data) to the end of the file. The subprograms will read their own `.exe` file, seek to the end, and parse this appended payload to extract the clues. This preserves the single-file requirement while successfully passing data to the spawned programs.

### Scope
**In Scope:**
- Updating `.csproj` files for `TornMap`, `WaterPuddle`, `SimpleVictory`, `Note`, and `WaterMeter` to publish as single-file self-contained executables.
- Modifying `TreasureManager.cs` to append clue data to the end of the written `.exe`.
- Updating the subprograms (e.g., `Map.cs`, `WaterPuddleProgram.cs`) to parse the appended payload from their own `.exe` file instead of using `GetManifestResourceStream`.

**Out of Scope:**
- Changing the game's core architecture.

# Technical Design

### Current Implementation
- Subprograms compile to standard .NET applications (which produce `.exe`, `.dll`, `.json`).
- `Resources.csproj` bundles the outputs from `..\Builds\` into `Treasures.dll`.
- `TreasureManager` uses `Mono.Cecil` to inject clues as embedded resources into the executable.
- Subprograms use `Assembly.GetExecutingAssembly().GetManifestResourceStream` to read clues.

### Proposed Changes
**1. Subprogram Build Configuration:**
- Add `<PublishSingleFile>true</PublishSingleFile>`, `<SelfContained>true</SelfContained>`, `<RuntimeIdentifier>win-x64</RuntimeIdentifier>` to subprogram `.csproj` files.
- Add an MSBuild `<Target>` or similar step to run `dotnet publish` and output the single-file `.exe` directly to `..\Builds\`, replacing the existing multi-file output.

**2. TreasureManager Payload Appending:**
- Remove `Mono.Cecil` references and logic in `TreasureManager.cs`.
- When writing the `.exe` to disk, stream the embedded executable out. 
- If `InternalResources` has items, serialize the dictionary (e.g., using `BinaryWriter` to write keys and string values), write it to the end of the file, followed by the payload length (4 bytes) and a magic signature (e.g., 8 bytes `CALIBAN\0`).

**3. Subprogram Payload Extraction:**
- Create a shared utility method in Caliban (or add directly to subprograms) that opens `Environment.ProcessPath` for reading, seeks to the end, checks for the magic signature, reads the length, and extracts the dictionary.
- Update subprograms (`Map.cs`, `WaterPuddleProgram.cs`, etc.) to use this payload extraction method instead of `GetManifestResourceStream`.