# Exploding Elves

A Unity DOTS (ECS) simulation built to run with lots of entities on mobile. Four spawners each generate elves of one color (black, red, white and blue) that wander the scene at random. When two elves of the same color collide, they spawn another elf of that color. When elves of different colors collide, both explode. The player can change how often each spawner generates elves, and the simulation speed.

Developed by Chicken 🐔 in October 2025.

## Tech

- Unity 2022.3 (2022.3.62f2), URP
- Entities 1.4 with Entities Graphics, Burst-compiled systems and jobs
- Unity Physics 1.4 (replacing the built-in physics)
- Android and iOS player settings, with safe-area handling for the UI

## Running it

1. Open the project in Unity 2022.3.62f2.
2. Import the free [Low Poly Christmas Pack + Santa Claus](https://assetstore.unity.com/packages/3d/characters/low-poly-christmas-pack-santa-claus-181035) from the Unity Asset Store into `Assets/Markus Art/`. Its files are not in this repo because the Asset Store license doesn't allow redistributing them.
3. Open `Assets/Scenes/MainScene.unity` and press Play. `SingleColorScene` runs a single color.

## Breakdown

The core features are the MVP:

- **Spawners:** four of them, each handling its own spawn time. They ended up as separate prefabs with different materials rather than one prefab with a changed color.
- **Random movement:** elves start in a random direction and can change it with a certain probability, confined to an area.
- **Same-color collisions:** spawn one new elf. A cooldown after spawning and colliding stops the new elf from colliding with its "parents" right away.
- **Different-color collisions:** both elves explode.
- **Spawn interval UI:** simple but mobile friendly, with a min and max value.
- **Android and iOS setup:** minding the orientation and the safe areas of different devices.

Optional features considered: simulation speed, elf count by color, unit tests, explosion animation and particles, a memory usage report, moving spawners, more than 4 spawners, a bigger map, an options menu, editor tools, and replacing the "hybrid" animation approach with a better optimised one.

## Thinking about the solution

The simulation needs a great amount of objects running on mobile, so optimization was a critical requirement.

ECS is a great option for games with a lot of objects. Developing with ECS is very different from using pure MonoBehaviours, so I had to choose before writing any code. I wasn't very familiar with ECS, so I read its documentation, went through code samples and tested locally to check it was feasible and a good fit.

Minor functionality, such as the UI, was a mix of AI (Cursor) and code samples. I also used AI to pinpoint some exceptions and possible solutions for them.

## Systems

### Elf Animator System

The 3D assets come with animations, which are not very compatible with ECS. This system creates a GameObject that follows and updates each entity. This "hybrid approach" is not the most efficient, but it is fast and easy to implement. It could be improved with libraries that handle animation differently.

### Elf Count System

Counts the `ElfComponent`s by color and leaves the totals in an `ElfCountData` singleton entity. I tried implementing it with spawn and destroy events, but it didn't work as it should. It doesn't use much memory, and it could be improved further by updating every few frames instead of every frame.

### Collision System

One of the most important systems in the project, and the one that uses the most memory. Every collision raises an event that this system processes. The prefabs use Unity Physics "Physics Shape" and "Physics Body", which only raise collision events when elves collide with each other or with "killer" entities, so a lot of unnecessary processing is avoided. The system checks the colors of the elves involved and acts accordingly, keeping track of the entities it already handled so none is processed twice.

### Spawn System

Spawns elves from the spawner authorings, at the start one per color, each holding the color it spawns. The time between spawns lives in a `SpawnerSettings` singleton entity, which the UI also writes to.

### Pending Destroy Elves System

A cleanup system that deletes elf entities and runs after the Collision System.

## Known bugs

- On Android the 3D assets are not animated. It may be code stripping, or a missing graphics setting.
- An "outburst" of one color (many same-color elves crashing into each other) is hard to revert. A cap could help, such as no color going over 3x the average, or spawning more of the colors with fewer elves.
