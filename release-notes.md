## Update 1.4.1

- Fixed `AttemptTextureFix` feature that converted VRM shader to in-game shader (fixes #18):

  - Side note: I will eventually rename this option to be clear what this option even does.

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
