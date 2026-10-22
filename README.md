# Project Hedra
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


## CI and publishing

Builds use the .NET SDK pinned in `global.json` and target .NET 10 LTS. Install
that SDK before building locally. The C# language version remains 10. Initialize
only the public dependencies required by the build:

```sh
git submodule update --init --depth 1 -- hedra-core project-hedra-assets
```

`hedra-tools` is optional and is not fetched by CI. Pull requests, including forks,
can run the checks without the old `HEDRA_CI` secret. CI uses read-only repository
permissions and does not expose Steam credentials to build jobs.

On Linux or Windows (Git Bash), run the appropriate `build-deps-*.sh`, then:

```sh
dotnet restore "Project Hedra.sln" --locked-mode
dotnet build "Project Hedra.sln" -c Release --no-restore
dotnet test HedraTests/HedraTests.csproj -c Release --no-build --no-restore
```

The lockfiles include the supported runtime identifiers. Restore the solution
before publishing, then use `./scripts/publish.sh linux-x64 Release` (or `win-x64`
and `win-x86`). Publishing reuses the archives produced by the tested build in
`.ci/assets/<configuration>` and stages the Steam depots under `.ci/publish`.
This avoids repacking assets for each architecture. The asset compiler is a
project dependency; the tests reuse its output instead of packing it again.
To update dependencies intentionally, run `dotnet restore "Project Hedra.sln"
--force-evaluate` and commit the lockfile changes.

The shared validation workflow runs the full test suite on Ubuntu 24.04 and
Windows 2022. macOS 15 Intel builds the solution and native library and runs the
portable allocator, block, math, and heightmap tests. This checks the committed
code without requiring the separate macOS game/physics port. It does not claim
full macOS gameplay or Apple Silicon coverage. TRX results and MSBuild binary logs
are retained for seven days, including on failures.

Native CMake outputs are cached per runner image, architecture, native revision,
and build-script inputs. Builds remain incremental and run without `sudo`.
Linux builds x64 once; Windows builds real Win32 and x64 variants. A headless
Python check calls all nine noise exports with positive and negative seeds,
all six perturbation modes, and SIMD-aligned and tail-sized sample grids. When a
PR or push changes the native submodule commit, CI also builds its previous
revision and compares outputs bit for bit. This baseline is supplied to managed
noise/mesh equivalence fixtures when those fixtures are present. The legacy
native library can return NaNs for normalization modes; the check preserves those
bytes in comparisons and requires finite results in non-normalizing modes.

The **Native CPU benchmark** workflow is manually dispatched and has no timing
threshold in required checks. It reports warmed median noise timings, including
Python copying and validation; it does not measure world generation or FPS.

The **Publish Hedra** workflow is also manually dispatched. It validates the
selected Debug/Release configuration, runs the shared checks on the selected
commit, publishes Windows x64/x86 and Linux x64 packages, then deploys those
artifacts to Steam's `test` branch. A failed check prevents deployment. Deployments
are serialized, and Linux packages use a tar archive to preserve executable
permissions through GitHub artifacts. The deploy job needs `STEAM_USERNAME`,
`STEAM_PASSWORD`, and `STEAM_CONFIG_VDF`; the pinned Steam action no longer uses
SSFN inputs. Dependabot checks action pins weekly. Repository branch protection
should require the validation checks before merging.

.NET 10 no longer implements `BinaryFormatter`. Heightmap format 1.3 continues to
round-trip; older serialized heightmaps now produce an explicit re-export error.
They must be converted with a legacy writer before use. Existing NuGet dependency
advisories remain visible during restore; this CI change does not upgrade the
rendering, scripting, or image-processing dependencies.
