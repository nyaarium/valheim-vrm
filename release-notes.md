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
