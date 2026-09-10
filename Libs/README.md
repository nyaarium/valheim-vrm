# Runtime dependencies

The UniVRM libraries in `Managed/` are built together from
[UniVRM v0.131.2](https://github.com/vrm-c/UniVRM/releases/tag/v0.131.2).
Both the `UniVRM-0.131.2.unitypackage` (VRM 0.x) and
`VRM-0.131.2.unitypackage` (VRM 1.0) are required.

Do not mix the older `VRMShaders.*` assemblies with this release. Their importer
signatures use `VRMShaders.SubAssetKey` and `VRMShaders.ITextureDeserializer`;
0.131.2 uses the corresponding types in `UniGLTF`. Such a mixture can compile
but throws `MissingMethodException` when a VRM 1.0 model is imported.

To rebuild these libraries, import both packages into an empty Unity 2022.3.22f1
project. Build a Windows x64 player with the Mono scripting backend, managed
stripping disabled, and engine-code stripping disabled. Copy these assemblies
from the player's `*_Data/Managed` directory:

- `VRM.dll`, `VRM10.dll`, `VrmLib.dll`
- `MToon.dll`, `VRM10.MToon10.dll`
- `UniGLTF.dll`, `UniGLTF.Utils.dll`, `UniGLTF.UniUnlit.dll`
- `UniHumanoid.dll`, `FastSpringBone10.dll`, `SpringBoneJobs.dll`

The build used Burst 1.8.12 and Mathematics 1.2.6 in the Unity export project.
The mod itself references the copies supplied by the installed game. Neither
package is included in the release ZIP, because overwriting Valheim's own
Unity packages can break other game code after an update.

`AsyncImageLoader.Runtime.dll`, native `FreeImage.dll`, and the existing shader
bundle are unchanged. The UniVRM, UniGLTF, and MToon MIT notices are preserved in the `Managed/*-LICENSE.txt` files and included in release archives.

To check the importer references without launching Unity or the game:

```powershell
powershell -NoProfile -File tools/Test-RuntimeDependencies.ps1 -ValheimPath 'C:\Games\Valheim'
```

This reads assembly metadata through BepInEx's bundled Mono.Cecil. It also accepts
`-LibrariesPath` to check another set of runtime libraries.
