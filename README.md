# Project Hedra

All code at the [`pre-ai`](https://github.com/maxilevi/project-hedra/tree/pre-ai) tag and in its earlier history was written before AI-assisted development.

![](https://github.com/maxilevi/project-hedra/actions/workflows/main.yml/badge.svg)
![](https://github.com/maxilevi/project-hedra/actions/workflows/deploy.yml/badge.svg)

[Project Hedra](https://store.steampowered.com/app/1009960/) is an ambitious open-world, low-poly RPG. Choose your class, explore endless procedurally generated worlds, unlock different skills and complete procedurally generated quests.

![](https://cdn.akamai.steamstatic.com/steam/apps/1009960/ss_4d11007f15cd0b1b5fd10e5c3464281b020d58ad.jpg?t=1675777899)

More screenshots at ([Screenshots](#Screenshots))

## Timeline/Context

I started this project when I was in highschool (around 15 yo) in order to learn how OpenGL worked. 

The scope creep quickly arrived and after a few years it turned into a fullfledged 3D game and game engine. The codebase has around 100k LOC and the game has first class support for Linux and Windows.

## Features

The million lines of code are not in vain. The game and engine are both full of features :)

### Game Features
* Beatiful procedurally generated worlds
* Quests and bosses
* 100+ items with rarities system
* Skills and skill tree system with N+ skills
* 4 different classes with 2 specializations each ()
* Procedurally placed structures generated dynamically
* Fishing, crafting, etc
* Trading and NPCs
* Companions and pets
* Beatiful soundtrack
* Navigation, maps
* Different dimensions
* More stuff I don't recall :)

### Engine Features
* Fully fledged renderer using modern OpenGL
* Skinned mesh rendering 
* Support for loading and playing generic animations
* Collada (.dae) parser
* Static mesh rendering
* Screen space reflections
* Screen space ambient occlusion
* Bloom, FXAA & MSAA
* Modding support
* CI for automatically deploying to a Steam beta branch
* Localization support
* Sound support using OpenAL
* Python scripting support
* Module system for loading data oriented objects
* Steam integration
* Fully fledged UI & controls system (dropdowns, buttons, etc.) written from raw OpenGL
* Dynamic shader system for reloading during runtime
* Frustum culling, occlusion culling,
* Mesh simplification for runtime level of detail (LOD)
* Skybox support
* Shadows
* 3D trails renderering support
* Particle system using object instancing in opengl
* bulletphysics integration
* Chunk system for infinite worlds support
* .ply parsing support
* Dynamic meshing support using MarchingCubes
* Custom implementation of basic C# primitives and allocators (like Lists) for supporting stack allocated lists and minimizing the automatic runtime allocations (and thus the GC).
* 1000+ plus test cases
* Framework for mocking game components and doing integration testing on games
* Image and asset compression for faster loading and smaller size
* Entity component system 
* More stuff I don't recall :)

## Interesting bits

* [WorldRenderer.cs](https://github.com/maxilevi/project-hedra/blob/master/Hedra/Engine/Rendering/WorldRenderer.cs). Renders the entire world in just 3 (actually it depends on how partitioned is the data buffer, but it was designed to be in 3) draw calls (water, terrain, instanced) using glMultiDrawElements.

* [Block.cs](https://github.com/maxilevi/project-hedra/blob/master/Hedra/Engine/Generation/Block.cs). Core component of the world. Read more at https://maxilevi.com/blog/procedural-meshing-hedra

* [Fishing.py](https://github.com/maxilevi/project-hedra/blob/master/Hedra/Scripts/Fishing.py). Written in Python as a mod script using IronPython, directly interacts with the C# API.

* [NativeArray.cs](https://github.com/maxilevi/project-hedra/blob/master/Framework/NativeArray.cs) Implementation of an array primitive using a custom allocator. The same project provides heap based allocators as well as stac based ones using `stackalloc`. The idea was to minimize the automatic heap allocations (and thus avoiding the GC) during the game frames.

## Interesting facts

* The world is fully procedurally generated, there is no world save file. Only the player contents are saved and the world seed. The rest is created at runtime.
* The world is voxel based but an isosurface extraction algorithm called [Marching Cubes](https://en.wikipedia.org/wiki/Marching_cubes) is applied to extract an implicit continous surface from the discrete voxels.
* The name "Hedra" comes from tetrahedra which is the shape used in the original iso surface extraction algorithm I implemented [Marching Tetrahedra](https://en.wikipedia.org/wiki/Marching_tetrahedra)
* The game only supports loading .PLY and .DAE models, both which are in plaintext and the models have no textures. Therefore if you put this into perspective, this means I wrote a bunch of text that reads text and creates interactive images from this. I think this is pretty cool.

## Screenshots

![image](https://cdn.akamai.steamstatic.com/steam/apps/1009960/ss_b36484e33d3c9f18555a3f55d60149d3913ad73a.jpg?t=1675777899)
![image](https://cdn.akamai.steamstatic.com/steam/apps/1009960/ss_e5a785f7eed4f5e1c652b8f7368f12bbf42f151e.jpg?t=1675777899)
![image](https://cdn.akamai.steamstatic.com/steam/apps/1009960/ss_b8abea9f49243d1d4f5145e2627019c76b6ad520.jpg?t=1675777899)
![image](https://cdn.akamai.steamstatic.com/steam/apps/1009960/ss_0e5de5ddf3c8b23d3efc64a1912dd42b2fdea31c.jpg?t=1675777899)

 TODO add more

## Code Structure

### Overall Architecture

The game is a mod to a base engine. The idea of architecturing this way was to force myself to maintain the mod API up to date, therefore encouraging access to make mods.

#### Hedra

Basically the entire game. This includes AI logic, components, world generation, module systems, object placement and lots of other stuff.

##### Hedra.Engine

The core parts of the engine, this defines all the abstractions to the world. It handles rendering, sound, different base systems as well as the general windowing, game loop and platform features. The split was not done at the start so some stuff might still be outside.

#### Framework

Core primitives used in the engine. This includes heap and stack allocators, custom implementations of lists and arrays as well as wrappers to pointers to native and some other useful primitives

#### HedraTests

Test suite that

#### Asset Builder

Command line tool to compress the game assets into convenient binary files. Combines images, models, sound files, shaders into a single binary file with a structure designed for fast data retrieval.

#### Server

Once upong a time Hedra used to support multiplayer. It worked by doing [UDP Hole Punching](https://en.wikipedia.org/wiki/UDP_hole_punching) and having clients communicate P2P, this server was in charge of that. You might also see some networking remnants in the Engine code.

# How to build

The build process is pretty straightforward.
1. Checkout all the submodules
2. Run the build depedencies scripts
3. Compile the C# solution

You can check the GitHub actions workflows for some examples on Linux and Windows.



## macOS

On macOS, install the Xcode command-line tools, CMake, and a .NET SDK (6 or newer),
and initialize the repository submodules. Then run:

```sh
./scripts/build-macos.sh
open ".macos-build/Project Hedra.app"
```

This builds an optimized, self-contained app with the .NET 6 runtime and native
world-generation and Bullet physics libraries. The build downloads pinned Bullet
sources into `.macos-build`; subsequent builds reuse them. The macOS CMake wrapper
supports Apple Silicon (`arm64`) and Intel (`HEDRA_ARCH=x86_64`); runtime validation
has been performed on Apple Silicon. An Intel runtime has not been tested.

Use the Release app for playing and performance measurements. Debug builds enable
expensive OpenGL validation. To build only the native dependencies, run
`./build-deps-macos.sh`.

Character saves, settings, logs, and crash reports are stored in the user's .NET
application-data directory (`~/.config/Project Hedra` on the tested runtime).
Existing characters in the app's `Characters` directory are copied there on first
launch. Rebuilding the app does not replace these saves. Steam integration remains
optional: this repository does not include a macOS Steamworks native library, so
the macOS app runs offline without Steam achievements or networking.

To record actual render-loop FPS and average CPU frame costs every five seconds:

```sh
HEDRA_PROFILE=1 ".macos-build/Project Hedra.app/Contents/MacOS/Hedra"
```

The measurements are written to `~/.config/Project Hedra/log.txt`. Allow the initial
world load to settle before comparing steady-state gameplay. FPS varies with the
scene, resolution, quality settings, and hardware.

To measure application startup separately from world generation:

```sh
HEDRA_PROFILE_STARTUP=1 ".macos-build/Project Hedra.app/Contents/MacOS/Hedra"
```

`STARTUP_PHASE` records archive extraction, asset setup, model caches, rendering,
player/UI creation, and scripts in milliseconds. Extraction phases are included
in the `assets` phase; do not add nested phases together. `STARTUP_READY` measures
elapsed time from entry into `Main` until initialization finishes, before the
normal game loop resumes. It excludes character selection and playable-world
generation. Startup profiling is disabled by default.

The macOS packaging script also bakes fixed structure connectivity and navigation
sample points into `generation.bin`. Structure transforms, biome colors, terrain,
and placement remain procedural. The runtime checks source fingerprints and uses
the original algorithms if the file is missing or the source model has changed.
To rebuild this cache in an existing bundle:

```sh
".macos-build/Project Hedra.app/Contents/MacOS/Hedra" --bake-generation \
  "$PWD/.macos-build/Project Hedra.app/Contents/MacOS/generation.bin"
```

To measure generation using an existing character without changing its save:

```sh
HEDRA_PROFILE=1 HEDRA_GENERATION_BENCHMARK="$HOME/.config/Project Hedra/Characters/MacTest.db" \
  ".macos-build/Project Hedra.app/Contents/MacOS/Hedra"
```

`GEN_RESULT` reports the time from character loading until the target disk has
finished generation, collision preparation, and GPU uploads at its requested LOD,
and all scheduled structure builds have completed. Application and asset startup
are excluded. Optional `HEDRA_VERIFY_TERRAIN=1`, `HEDRA_VERIFY_MESH_CELLS=1`,
`HEDRA_VERIFY_WAYPOINTS=1`, and `HEDRA_VERIFY_BAKED_ASSETS=1` compare optimized
results with their original algorithms; these checks add substantial work and
should be disabled for timing runs.

CPU meshing can also be tested and timed without starting the game or creating a
graphics context. The headless fixtures use synthetic flat terrain, hills, caves,
water, and mixed materials with real chunk meshing, welding, border detection,
simplification, smoothing, and flattening at LODs 1, 2, 4, and 8:

```sh
HEDRA_MESH_BENCHMARK=1 HEDRA_PROFILE_GENERATION=1 DOTNET_TieredCompilation=0 \
  DOTNET_ROLL_FORWARD=Major dotnet test HedraTests/HedraTests.csproj -c Release \
  -p:Platform=AnyCPU --filter 'FullyQualifiedName~TerrainMeshingBenchmark'
```

The benchmark reports warmed median CPU times and managed allocations, including
the actual .NET runtime used. It measures the complete CPU terrain mesh path;
GPU uploads and world/asset generation are excluded. Dense traversal with separate
welding and optimized traversal with fused indexed emission are timed in the same
process (`MESH_MODE` identifies these modes). Correctness checks compare
all mesh attributes and collision meshes against dense traversal, welding against
the original managed algorithm, and smoothing against the original weighting:

```sh
DOTNET_ROLL_FORWARD=Major dotnet test HedraTests/HedraTests.csproj -c Release \
  -p:Platform=AnyCPU --filter 'FullyQualifiedName~TerrainSurfaceMaskTest|FullyQualifiedName~NativeVertexWeldTest|FullyQualifiedName~NativeFlattenEquivalenceTest|FullyQualifiedName~IndexedMarchingCubesTest|FullyQualifiedName~SmoothingEquivalenceTest|FullyQualifiedName~NativeTransformEquivalenceTest|FullyQualifiedName~ChunkMeshBorderDetectorTest'
```

When changing `hedra-core`, preserve a copy of the previous native library before
rebuilding. Set `HEDRA_MESH_BASELINE_NATIVE` to its absolute path and run
`NativeSimplifierEquivalenceTest` to compare simplified indices against the old
library, including empty, duplicate, and out-of-range border-lock entries,
vertex counts near powers of two, and planar meshes with seam duplicates or
unlocked borders. The planar shortcut requires an exact axis-aligned plane with
no border/seam penalties; other meshes use the full quadric-error calculation.
Water cache checks also include water-filled caves, the top of a chunk, and
missing neighbors. `HEDRA_VERIFY_MESH_CELLS=1` checks cached water cells against
raw block sampling as well as checking terrain samples.

The terrain block generator also has a headless benchmark:

```sh
HEDRA_BLOCK_BENCHMARK=1 HEDRA_PROFILE_GENERATION=1 HEDRA_VERIFY_TERRAIN=0 \
  DOTNET_TieredCompilation=0 DOTNET_ROLL_FORWARD=Major \
  dotnet test HedraTests/HedraTests.csproj -c Release -p:Platform=AnyCPU \
  --filter 'FullyQualifiedName~HeadlessBlockGenerationTest.MeasureRealBlockGeneration'
```

`BLOCK_BENCH` reports warmed median times, managed allocations, and the runtime
for generating all 262,144 blocks in each of three normal-biome chunks. It runs
the real native biome noise, density interpolation, material and water passes,
and block packing. The surrounding world is a fixture with no structures or
landforms and a constant small-frequency world-noise input. Environment placement,
meshing, assets, GPU work, and the game loop are excluded. These measurements do
not establish a whole-world loading speedup or gameplay FPS.

Correctness tests replay the original generation passes and compare every packed
block, water coordinates, and water/river flags. They cover normal and mushroom
biomes, plus cases with nonzero world noise, plateaus, and groundworks:

```sh
DOTNET_ROLL_FORWARD=Major dotnet test HedraTests/HedraTests.csproj -c Release \
  -p:Platform=AnyCPU \
  --filter 'FullyQualifiedName~HeadlessBlockGenerationTest.RealBlockGenerationMatchesReference|FullyQualifiedName~TerrainBlockPackingTest'
```

Noise generation has a separate headless benchmark and bitwise regression check.
Before rebuilding `hedra-core`, copy its current library to a separate path. Set
`HEDRA_NOISE_BASELINE_NATIVE` to that absolute path to compare the optimized code
against the previous native implementation:

```sh
HEDRA_NOISE_BASELINE_NATIVE=/absolute/path/to/previous/libhedracore.dylib \
  HEDRA_NOISE_BENCHMARK=1 DOTNET_TieredCompilation=0 DOTNET_ROLL_FORWARD=Major \
  dotnet test HedraTests/HedraTests.csproj -c Release -p:Platform=AnyCPU \
  --filter 'FullyQualifiedName~NoiseGenerationTest'
```

The checks cover all nine exposed noise types, all perturbation modes, negative
coordinates and seeds, short rows, aligned rows, and partial SIMD tails. Without
a baseline library, they compare the managed results against a scalar copy from
the current native implementation. `NOISE_BENCH` reports alternating, warmed
median timings for native generation plus managed allocation and copying. These
measurements exclude terrain processing, meshing, and the game loop.
