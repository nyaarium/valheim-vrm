## Update 1.6.0

**BepInEx:** 5.4.23.3

**Valheim:** 0.221.4 (n-35)

### 🎉 *Everyone should try out `UseMToonShader=false`*

Rewrote the entire texture loading system to skip multiple buffer copies and cache reused textures. You may see load improvements by x100 or more. (fixes #26)

The results are so impressive that I'm making it final. `AttemptTextureFix` is no longer a weird *"attempt"*. It's a finished feature:

- `AttemptTextureFix` flag has been **removed**
- `UseMToonShader=false` is the new setting to use the in-game Valheim shader
- `UseMToonShader=true` for traditional unlit VRM MToon shader

In my avatar tests with `UseMToonShader=false`:

- Version 1.5.1 material load time: **71.84 seconds**
- Version 1.6.0 material load time: **0.02 seconds**

### Logs in old 1.5.1:
```
🖌️ Processing 15 materials for "Nyaa" VRM  |  UseMToonShader False  |  AttemptTextureFix True
🖌️ Converted "Eve Sweater Atlas (Instance)" in 6.65 seconds
    "Eve Sweater Atlas.color"  |  8192x8192  |  ARGB32
    "Eve Sweater Atlas.normal"  |  4096x4096  |  ARGB32
🖌️ Converted "Eve Sweater Atlas Refract (Instance)" in 6.50 seconds
    "Eve Sweater Atlas.color"  |  8192x8192  |  ARGB32
    "Eve Sweater Atlas.normal"  |  4096x4096  |  ARGB32
🖌️ Converted "Eve Sweater Atlas Metal Silver (Instance)" in 6.47 seconds
    "Eve Sweater Atlas.color"  |  8192x8192  |  ARGB32
    "Eve Sweater Atlas.normal"  |  4096x4096  |  ARGB32
🖌️ Converted "Palette Metal Gold (Instance)" in 0.09 seconds
    "Palette 16x16"  |  16x16  |  ARGB32
🖌️ Converted "Gem Red (Instance)" in 0.01 seconds
    "Palette 16x16"  |  16x16  |  ARGB32
🖌️ Converted "Palette Metal Silver (Instance)" in 0.01 seconds
    "Palette 16x16"  |  16x16  |  ARGB32
🖌️ Converted "Gem Clear (Instance)" in 0.00 seconds
    "Palette 16x16"  |  16x16  |  ARGB32
🖌️ Converted "Eve Sweater Atlas Metal Gold (Instance)" in 6.65 seconds
    "Eve Sweater Atlas.color"  |  8192x8192  |  ARGB32
    "Eve Sweater Atlas.normal"  |  4096x4096  |  ARGB32
🖌️ Converted "Eve Sweater Atlas Metal Red (Instance)" in 6.48 seconds
    "Eve Sweater Atlas.color"  |  8192x8192  |  ARGB32
    "Eve Sweater Atlas.normal"  |  4096x4096  |  ARGB32
🖌️ Converted "Eve Sweater Atlas Outlined (Instance)" in 6.38 seconds
    "Eve Sweater Atlas.color"  |  8192x8192  |  ARGB32
    "Eve Sweater Atlas.normal"  |  4096x4096  |  ARGB32
🖌️ Converted "Eve Sweater Atlas Hosiery (Instance)" in 6.66 seconds
    "Eve Sweater Atlas.color"  |  8192x8192  |  ARGB32
    "Eve Sweater Atlas.normal"  |  4096x4096  |  ARGB32
🖌️ Converted "Eve Sweater Atlas Expressions (Instance)" in 6.50 seconds
    "Eve Sweater Atlas.color"  |  8192x8192  |  ARGB32
    "Eve Sweater Atlas.normal"  |  4096x4096  |  ARGB32
🖌️ Converted "Eve Sweater Atlas (Instance)" in 6.43 seconds
    "Eve Sweater Atlas.color"  |  8192x8192  |  ARGB32
    "Eve Sweater Atlas.normal"  |  4096x4096  |  ARGB32
🖌️ Converted "Eve Sweater Atlas Eyes (Instance)" in 6.45 seconds
    "Eve Sweater Atlas.color"  |  8192x8192  |  ARGB32
    "Eve Sweater Atlas.normal"  |  4096x4096  |  ARGB32
🖌️ Converted "Eve Sweater Atlas Outlined (Instance)" in 6.39 seconds
    "Eve Sweater Atlas.color"  |  8192x8192  |  ARGB32
    "Eve Sweater Atlas.normal"  |  4096x4096  |  ARGB32
🖌️ Finished processing 15 materials for VRM 'Nyaa' in 71.84 seconds
```

### Logs in new 1.6.0:
```
[Info   : Unity Log] [VrmTextureCache] 💽 Computed VRM hash for 'Nyaa': 18ed5641e5888190d219a9b8bc709167ef3304a635133b6de1fe892c48f98be0
[Info   : Unity Log] [ValheimVRM Async] loading vrm: 33047024 bytes
[Info   : Unity Log] [VrmTextureCache] 💽 Cache MISS - (8192x8192, ARGB32, srgb, 18035262 bytes)
[Info   : Unity Log] [VrmTextureCache] 💽 Cache MISS - (4096x4096, ARGB32, linear, 2300689 bytes)
[Info   : Unity Log] [VrmTextureCache] 💽 Cache MISS - (256x256, ARGB32, srgb, 54254 bytes)
[Info   : Unity Log] [VrmTextureCache] 💽 Cache MISS - (16x16, ARGB32, srgb, 667 bytes)
[Info   : Unity Log] [VrmTextureCache] 💽 Cache MISS - (256x256, ARGB32, srgb, 126690 bytes)
[Info   : Unity Log] [VrmTextureCache] 💽 Cache MISS - (256x256, ARGB32, srgb, 108585 bytes)
[Info   : Unity Log] [VrmTextureCache] 💽 Cache MISS - (512x512, ARGB32, srgb, 511452 bytes)
[Info   : Unity Log] [ValheimVRM] VRM read successful
[Info   : Unity Log] [VrmTextureCache] 💽 RegisterVrm started for 'Nyaa' with hash: 18ed5641e5888190d219a9b8bc709167ef3304a635133b6de1fe892c48f98be0
[Info   : Unity Log] [VrmTextureCache] 💽 RegisterVrm: Created state for Nyaa
[Info   : Unity Log] [ValheimVRM] 🖌️ Processing 15 materials for "Nyaa" VRM  |  UseMToonShader False
[Info   : Unity Log] [ValheimVRM] 🖌️ Converted "Eve Sweater Atlas (Instance)"
[Info   : Unity Log] [ValheimVRM]     "Eve Sweater Atlas.color"  |  8192x8192  |  ARGB32
[Info   : Unity Log] [ValheimVRM]     "Eve Sweater Atlas.normal"  |  4096x4096  |  ARGB32
[Info   : Unity Log] [ValheimVRM] 🖌️ Converted "Eve Sweater Atlas Refract (Instance)"
[Info   : Unity Log] [ValheimVRM]     "Eve Sweater Atlas.color"  |  8192x8192  |  ARGB32
[Info   : Unity Log] [ValheimVRM]     "Eve Sweater Atlas.normal"  |  4096x4096  |  ARGB32
[Info   : Unity Log] [ValheimVRM] 🖌️ Converted "Eve Sweater Atlas Metal Silver (Instance)"
[Info   : Unity Log] [ValheimVRM]     "Eve Sweater Atlas.color"  |  8192x8192  |  ARGB32
[Info   : Unity Log] [ValheimVRM]     "Eve Sweater Atlas.normal"  |  4096x4096  |  ARGB32
[Info   : Unity Log] [ValheimVRM] 🖌️ Converted "Palette Metal Gold (Instance)"
[Info   : Unity Log] [ValheimVRM]     "Palette 16x16"  |  16x16  |  ARGB32
[Info   : Unity Log] [ValheimVRM] 🖌️ Converted "Gem Red (Instance)"
[Info   : Unity Log] [ValheimVRM]     "Palette 16x16"  |  16x16  |  ARGB32
[Info   : Unity Log] [ValheimVRM] 🖌️ Converted "Palette Metal Silver (Instance)"
[Info   : Unity Log] [ValheimVRM]     "Palette 16x16"  |  16x16  |  ARGB32
[Info   : Unity Log] [ValheimVRM] 🖌️ Converted "Gem Clear (Instance)"
[Info   : Unity Log] [ValheimVRM]     "Palette 16x16"  |  16x16  |  ARGB32
[Info   : Unity Log] [ValheimVRM] 🖌️ Converted "Eve Sweater Atlas Metal Gold (Instance)"
[Info   : Unity Log] [ValheimVRM]     "Eve Sweater Atlas.color"  |  8192x8192  |  ARGB32
[Info   : Unity Log] [ValheimVRM]     "Eve Sweater Atlas.normal"  |  4096x4096  |  ARGB32
[Info   : Unity Log] [ValheimVRM] 🖌️ Converted "Eve Sweater Atlas Metal Red (Instance)"
[Info   : Unity Log] [ValheimVRM]     "Eve Sweater Atlas.color"  |  8192x8192  |  ARGB32
[Info   : Unity Log] [ValheimVRM]     "Eve Sweater Atlas.normal"  |  4096x4096  |  ARGB32
[Info   : Unity Log] [ValheimVRM] 🖌️ Converted "Eve Sweater Atlas Outlined (Instance)"
[Info   : Unity Log] [ValheimVRM]     "Eve Sweater Atlas.color"  |  8192x8192  |  ARGB32
[Info   : Unity Log] [ValheimVRM]     "Eve Sweater Atlas.normal"  |  4096x4096  |  ARGB32
[Info   : Unity Log] [ValheimVRM] 🖌️ Converted "Eve Sweater Atlas Hosiery (Instance)"
[Info   : Unity Log] [ValheimVRM]     "Eve Sweater Atlas.color"  |  8192x8192  |  ARGB32
[Info   : Unity Log] [ValheimVRM]     "Eve Sweater Atlas.normal"  |  4096x4096  |  ARGB32
[Info   : Unity Log] [ValheimVRM] 🖌️ Converted "Eve Sweater Atlas Expressions (Instance)"
[Info   : Unity Log] [ValheimVRM]     "Eve Sweater Atlas.color"  |  8192x8192  |  ARGB32
[Info   : Unity Log] [ValheimVRM]     "Eve Sweater Atlas.normal"  |  4096x4096  |  ARGB32
[Info   : Unity Log] [ValheimVRM] 🖌️ Converted "Eve Sweater Atlas (Instance)"
[Info   : Unity Log] [ValheimVRM]     "Eve Sweater Atlas.color"  |  8192x8192  |  ARGB32
[Info   : Unity Log] [ValheimVRM]     "Eve Sweater Atlas.normal"  |  4096x4096  |  ARGB32
[Info   : Unity Log] [ValheimVRM] 🖌️ Converted "Eve Sweater Atlas Eyes (Instance)"
[Info   : Unity Log] [ValheimVRM]     "Eve Sweater Atlas.color"  |  8192x8192  |  ARGB32
[Info   : Unity Log] [ValheimVRM]     "Eve Sweater Atlas.normal"  |  4096x4096  |  ARGB32
[Info   : Unity Log] [ValheimVRM] 🖌️ Converted "Eve Sweater Atlas Outlined (Instance)"
[Info   : Unity Log] [ValheimVRM]     "Eve Sweater Atlas.color"  |  8192x8192  |  ARGB32
[Info   : Unity Log] [ValheimVRM]     "Eve Sweater Atlas.normal"  |  4096x4096  |  ARGB32
[Info   : Unity Log] [ValheimVRM] 🖌️ Finished processing 15 materials for VRM 'Nyaa' in 0.02 seconds
```

## Update 1.5.1

**BepInEx:** 5.4.23.3

**Valheim:** 0.221.4 (n-35)

Fixed the custom texture loader to handle unusual textures *(like indexed color)*.

## Update 1.5.0

**BepInEx:** 5.4.23.3

**Valheim:** 0.221.4 (n-35)

When loading VRMs, progress will now be displayed in the top-left message HUD, telling you the amount of time taken to load the VRM.

Disabled the custom texture loader for indexed color textures. It broke with the latest Valheim update. *(re-enabled in 1.5.1)*

~~Match the .NET version to what Valheim uses, 4.8 (i think?).~~ *(nvm, the build error was something else. Reverting this in 1.5.1)*

## Update 1.4.2

This patch fixes a black-screen freeze when you die. (fixes #20)

Bug fixes by PR #19 revealed a pre-exising race condition when the ragdoll is created (in `Patch_Humanoid_OnRagdollCreated.Postfix`). It was a rare freeze before when it was syncronous. But now that it's asycronous, it always happens.

## Update 1.4.1

Fixed `AttemptTextureFix` feature that converted VRM shader to in-game shader (fixes #18):

*Side note:* I will eventually rename this option to be clear what this option even does.

## Update 1.4.0

*Major Project Restructure:** Much cleanup to the csproj file. And added support for Linux development environments and GitHub runners.

Added null-check in `Patch_Player_Awake.Postfix` to only add `VrmController` if not already present. (fixes #1)

Replaced unsafe `Substring` prefix checks with null-safe `StartsWith` in `Patch_VisEquipment_UpdateLodgroup.Postfix`. (fixes #2)

Implemented Harmony patch to skip `VRM.VRMBlendShapeProxy.OnDestroy` entirely, avoiding Editor assembly reference during disconnect. (fixes #3)

Added post-yield null-guards for `player`/`vrmModel` and re-fetched the `Animator` before camera-height step in `VRM.SetToPlayer`. (fixes #4)

Made `VRMShaders.Initialize()` idempotent with early-return after first call, then call `assetBundle.Unload(false)` to release bundle reference. (fixes #5)

Implemented ragdoll pose mirroring for VRM visibility during physics-driven ragdoll. (fixes #6)
- Parent VRM to ragdoll on `Humanoid.OnRagdollCreated` and keep VRM renderers enabled
- Hide vanilla ragdoll renderers to avoid double visuals
- In `VRMAnimationSync`, copy human bone positions/rotations from ragdoll animator to VRM every LateUpdate when in ragdoll mode (with existing model Y-offset)
