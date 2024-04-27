# ValheimVRM

**This mod requires BepInEx to be installed.**

This fork is actively developed and maintained. If you need help, want to request a feature, or found a bug; Head on over to the [discord](https://discord.gg/q3wuVMCvXE).

### How to install
[Download](https://github.com/aMidnightNova/ValheimVRM/releases/latest) the latest release and extract it. There will be a folder called release, copy the folders inside (BepInEx,valheim_Data) into your valheim install directory.
The folders are setup to put the files where they need to go.

### First time install
- make sure to copy / rename all files that end with .example into the correct corresponding file. E.G. \
 global_settings.txt.example -> global_settings.txt.

### Settings File
The name of the character in the game needs to correspond to a VRM and settings file like so.

**Character**: Midnight Nova \
**Settings File**: Midnight Nova_settings.txt \
**VRM**: Midnight Nova.vrm


### Default Settings and avatar for people you do not have custom stuff for.

**Settings File**: settings____Default.txt \
**VRM**: ___Default.vrm

**NOTE:** settings____Default.txt has 4 underscores, and ___Default.vrm has 3.

### Usefull Info
- If you have a shader compile error you probably need to use the old shader bundle. \
  the newer current bundle should work, but JIC ive included the old one still\
  Its in General settings. UseShaderBundle=<old,current>. Note that this will affect all models.

### Technical Stuff for maintaining this repo
- You might need to build an Asset Bundle of shaders to stay inline with UniVrm. This is probably a non issue
  unless Valheim Updates Unity. - see next point.
- Current UniVrm version is 121, for Unity 2022. UniVrm was 111 previous to  Valheim Patch 0.217.46. 111 is the last version to support Unity 2020.
- Most Recent AssetBundle of shaders is UniVrm.shaders. This has shaders that are required since version 67 - 70(I dont know exactly when).
- You will need to install UniVrm into a blank project (create the shader asset bundle there too)
  once that's done(install from git the assetBundle Browser), you will need to build the Unity Project. Find the Managed folder and set that
  as a system Path. - **VALHEIM_UNITY_LIBS**
- Set your Valheim Folder as a system path. **VALHEIM_INSTALL**


- If for whatever reason you are targeting 111 still, Make sure in Unity you have Mono  and .NET 4.x selected.
