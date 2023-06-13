# Project Hedra

[Project Hedra](https://store.steampowered.com/app/1009960/) is an ambitious open-world, low-poly RPG. Choose your class, explore endless procedurally generated worlds, unlock different skills and complete procedurally generated quests.


![](https://cdn.akamai.steamstatic.com/steam/apps/1009960/ss_4d11007f15cd0b1b5fd10e5c3464281b020d58ad.jpg?t=1675777899)

See more screenshots at ([Screenshots](#Screenshots))


# Timeline/Context

I started this project when I was in highschool (around 15 yo) in order to learn how OpenGL worked. 

The scope creep quickly arrived and after a few years it turned into a fullfledged 3D game and game engine. The codebase has close to 1 million lines of code and the game has first class support for Linux and Windows.

# Features

The million lines of code are not in vain. The game and engine are both full features :)

## Game Features
* Beatiful procedurally generated worlds
* Quests
* 100+ items
* Skills and skill tree system with N+ skills
* 4 different classes with 2 specializations each ()
* Procedurally placed structures generated dynamically
* Fishing, crafting, etc
* Companions and pets
* Beatiful soundtrack
* More

## Engine Features
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
* More

# Interesting bits

[Block.cs](https://github.com/maxilevi/project-hedra/blob/master/Hedra/Engine/Generation/Block.cs). Core component of the world.

See more https://maxilevi.com/blog/procedural-meshing-hedra

[Fishing.py](https://github.com/maxilevi/project-hedra/blob/master/Hedra/Scripts/Fishing.py). Written in . Interacts with the C# API.

[NativeArray.cs](https://github.com/maxilevi/project-hedra/blob/master/Framework/NativeArray.cs) Implementation of an array primitive using a custom allocator. The same project provides heap based allocators as well as stac based ones using `stackalloc`. The idea was to minimize the automatic heap allocations (and thus avoiding the GC) during the game frames.

# Interesting facts

* The world is fully procedurally generated, there is no world save file. Only the player contents are saved and the world seed. The rest is created at runtime.

# Screenshots

![image](https://cdn.akamai.steamstatic.com/steam/apps/1009960/ss_b36484e33d3c9f18555a3f55d60149d3913ad73a.jpg?t=1675777899)
![](https://cdn.akamai.steamstatic.com/steam/apps/1009960/ss_e5a785f7eed4f5e1c652b8f7368f12bbf42f151e.jpg?t=1675777899)
![image](https://cdn.akamai.steamstatic.com/steam/apps/1009960/ss_b8abea9f49243d1d4f5145e2627019c76b6ad520.jpg?t=1675777899)
![image](https://cdn.akamai.steamstatic.com/steam/apps/1009960/ss_0e5de5ddf3c8b23d3efc64a1912dd42b2fdea31c.jpg?t=1675777899)

 TODO add more

# Code Structure

## Overall Architecture

The game is a mod to a base engine. The idea of architecturing this way was to force myself to maintain the mod API up to date, therefore encouraging access to make mods.

### Hedra

#### Hedra.Engine

The  

### Framework



# How to build

TODO

