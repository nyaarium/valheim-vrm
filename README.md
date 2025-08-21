# ValheimVRM

## Game updated? Found a bug?

**[File an issue on the Issues tab](https://github.com/nyaarium/valheim-vrm/issues?q=is%3Aissue)** with **what the problem is**, and paste in the **error logged in the console**.

I'll _probably_ continue maintenance even when I'm not actively playing the game.

---

## Yet Another Fork

> **[yoship1639](https://github.com/yoship1639/ValheimVRM)** ► **[aMidnightNova](https://github.com/aMidnightNova/ValheimVRM)** ► **nyaarium**

Big thanks to the original authors and maintainers for their hard work. It's always been fun running around in custom avatars.

This fork diverges heavily from the original, to support Linux environments and CI/CD. As such, there won't be upstream PRs to ancestor repos ...Unless they really like Linux and GitHub workflows \:)

---

### Notes

If anyone's model has **MToon shaders**, you need to **disable SSAO**. This is true even if you do not have MToon stuff enabled in the settings. You can set `AttemptTextureFix` to true, it will convert shaders to standard at game runtime.

### How to Install

[Download the latest release](https://github.com/nyaarium/valheim-vrm/releases) and extract it directly into the Valheim install directory.

### First Time Install

- make sure to copy / rename all files that end with .example into the correct corresponding file. E.G. \
  global_settings.txt.example -> global_settings.txt.

### File Install Locations

The VRM and settings files should be in the ValheimVRM folder inside the Valheim game dir.

### Settings File

The name of the character in the game needs to correspond to a VRM and settings file like so.

**Character**: Midnight Nova \
**Settings File**: settings_Midnight Nova.txt \
**VRM**: Midnight Nova.vrm

### Default Settings and avatar for people you do not have custom stuff for

**Settings File**: settings\_**\_Default.txt \
**VRM**: \_**Default.vrm

**NOTE:** settings\_**_Default.txt has 4 underscores, and _**Default.vrm has 3.

### Usefull Info

- If you have a shader compile error you probably need to use the old shader bundle. \
  the newer current bundle should work, but JIC ive included the old one still\
  Its in General settings. UseShaderBundle=<old,current>. Note that this will affect all models.

---

## 🛑 Stop Here 🛑

Only continue down if you wish to fork from this repo and develop. The following info will help you get started.

### 🛠️ Technical Stuff for maintaining this repo

- The landscape of the project has changed a bit, so I'll have to come back and update these sections later.
- You might need to build an Asset Bundle of shaders to stay inline with UniVrm. This is probably a non issue
  unless Valheim Updates Unity. - see next point.
- Current UniVrm version is 121, for Unity 2022. UniVrm was 111 previous to Valheim Patch 0.217.46. 111 is the last version to support Unity 2020.
- Most Recent AssetBundle of shaders is UniVrm.shaders. This has shaders that are required since version 67 - 70(I dont know exactly when).
- You will need to install UniVrm into a blank project (create the shader asset bundle there too)
  once that's done(install from git the assetBundle Browser), you will need to build the Unity Project. Find the (build folder)\_Data and set that as a system Path. I called my project "UniVrm v121" so the data folder would be UniVrm v121_Data - **VALHEIM_UNITY_LIBS**
- inside your UniVrm Project you will need to install [UnityAsyncImageLoader](https://github.com/aMidnightNova/UnityAsyncImageLoader)
- Set your Valheim Folder as a system path. **VALHEIM_INSTALL**
- If for whatever reason you are targeting 111 still, Make sure in Unity you have Mono and .NET 4.x selected.
