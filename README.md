# Project Hedra

[Project Hedra](https://store.steampowered.com/app/1009960/) is an ambitious open-world, low-poly RPG. Choose your class, explore endless procedurally generated worlds, unlock different skills and complete procedurally generated quests.

# Why open source it?

# Timeline/Context

I started this project when I was in highschool (around 15 yo) in order to learn how open gl worked. The scope creep quickly arrived and after a few years it turned into a fullfledged 3D game and game engine. The codebase has close to 1 million lines of code and the game has first class support for Linux and Windows.

# Features

## Game Features
* Beatiful procedurally generated worlds
* Quests
* 100+ items
* Skill system
* 4 different classes
* Procedurally placed structures generated dynamically
* Fishing, crafting, etc

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

# Interesting bits

[Block.cs](https://github.com/maxilevi/project-hedra/blob/master/Hedra/Engine/Generation/Block.cs). Core component of the world.

See more https://maxilevi.com/blog/procedural-meshing-hedra

[Fishing.py](https://github.com/maxilevi/project-hedra/blob/master/Hedra/Scripts/Fishing.py). Written in . Interacts with the C# API.

# Code Structure

## Overall Architecture

The game is a mod to a base engine. The idea of architecturing this way was to force myself to maintain the mod API up to date, therefore encouraging access to make mods.

### Engine

The  



# How to build

TODO

