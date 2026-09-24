# Valheim 1.0 compatibility

Tested on Windows x64 with Valheim 1.0.7 (Steam build 25185596, network version
39), Unity 6000.0.75f1, and BepInEx 5.4.23.3.

## Reproduced on main

Main at `98cf3742211e203a87f5b8bbb9b53696b52e6569` compiles against the current
game, but importing a VRM 1.0 avatar fails before it can attach to the player:

```text
MissingMethodException: Method not found: void UniGLTF.ImporterContext..ctor(
    UniGLTF.GltfData,
    IReadOnlyDictionary<VRMShaders.SubAssetKey, UnityEngine.Object>,
    VRMShaders.ITextureDeserializer, UniGLTF.IMaterialDescriptorGenerator)
```

The runtime-dependency check finds 29 unresolved UniGLTF method references in
that main branch's `VRM10.dll`. Rebuilding only `ValheimVRM.dll` does not fix
references inside the bundled runtime libraries.

## Changes

- Build VRM 0.x and VRM 1.0 libraries from the same UniVRM 0.131.2 source and
  update the project's reference and release-file lists. Use the game's Burst
  and Mathematics libraries instead of overwriting them with older copies.
- Initialize VRM 1.0 visual clones while their transforms are still in the bind
  pose. Unity does not clone the importer's pose dictionary; an empty importer
  component must not supply the pose used by constraints.
- Retarget animation before UniVRM's constraint and spring update. For ragdolls,
  retain the avatar's bone lengths and map rotations using the live character's
  bone-axis offsets, applying parent bones first.
- Select the default cache entry even when it is already loaded, so the default
  avatar attaches again after respawning.
- Include the contents of the Windows release staging directory in the ZIP,
  along with license notices, so it extracts directly into the game directory.

## Verification

```powershell
$env:VALHEIM_INSTALL_PATH = 'C:\Games\Valheim'
powershell -NoProfile -File tools/Test-RuntimeDependencies.ps1 -ValheimPath $env:VALHEIM_INSTALL_PATH
dotnet msbuild ValheimVRM.csproj -restore -t:Compile -p:Configuration=Release
```

The metadata check resolves 335 UniGLTF method references across the matched
VRM 0.x and VRM 1.0 libraries. Pointing `-LibrariesPath` at the unmodified main
branch's libraries reproduces the failure above.

In an isolated game copy, eight VRM 1.0 avatars passed character-preview loading,
humanoid validation, shader support, and finite skinned-mesh checks. The world
test passed spawning, movement, sword and shield attachment, attacking, sitting,
death, ragdoll retargeting, and respawn with the cached default avatar. Bone
lengths remained unchanged during ragdoll simulation. Day/night, point-light,
and shadow rendering checks also passed.

The official UniVRM `AliciaSolid_vrm-0.51.vrm` fixture also passed a separate
VRM 0.x character-preview test, including import, humanoid validation, finite
skinned meshes, and conversion to the game's `Custom/Player` shader.

The Release build completed with no warnings or errors. Release contents are
checked separately from the source build: the archive must contain the plugin,
shader bundle, and complete UniVRM runtime set, without an extra staging folder
or copies of the game's Unity libraries.

To repeat the world checks, use a separate save directory and a VRM you are
licensed to use. Set it as `___Default.vrm`, create a character with no matching
VRM file, and exercise the actions above, including death and respawn in the
same session. The test avatars and game files are not included in this repository.

Linux, multiplayer avatar sharing, and combinations with other mods have not
been tested in this change. Existing material settings and shader-conversion
behavior are unchanged.
