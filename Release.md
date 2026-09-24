## Update 1.7.0

**BepInEx:** 5.4.23.3

**Valheim:** 1.0.7 (n-39)

### Changes

- Fix VRM imports and avatar lifecycle on Valheim 1.0 by @Celeste-twinkle (#53)
  - VRM 1.0 avatars import again. UniVRM libraries updated to 0.131.2.
  - VRM 1.0 constraints and spring bones follow the game's animation.
  - Ragdolls keep the avatar's own bone lengths.
  - Default avatar returns after respawn.
  - The mod no longer ships `Unity.Burst.dll` or `Unity.Mathematics.dll`. It uses the game's copies.

Thank you @Celeste-twinkle!
