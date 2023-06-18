# Project Hedra
![](https://github.com/maxilevi/project-hedra/actions/workflows/main.yml/badge.svg)
![](https://github.com/maxilevi/project-hedra/actions/workflows/deploy.yml/badge.svg)

(Submodule will be open sourced too soon!)

[Project Hedra](https://store.steampowered.com/app/1009960/) is an ambitious open-world, low-poly RPG. Choose your class, explore endless procedurally generated worlds, unlock different skills and complete procedurally generated quests.

![](https://cdn.akamai.steamstatic.com/steam/apps/1009960/ss_4d11007f15cd0b1b5fd10e5c3464281b020d58ad.jpg?t=1675777899)

More screenshots at ([Screenshots](#Screenshots))

## Timeline/Context

I started this project when I was in highschool (around 15 yo) in order to learn how OpenGL worked. 

The scope creep quickly arrived and after a few years it turned into a fullfledged 3D game and game engine. **The codebase has around 800k LOC** and the game has first class support for Linux and Windows.

## Features

The million lines of code are not in vain. The game and engine are both full features :)

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

WorldRenderer.cs

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

