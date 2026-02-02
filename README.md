# LibAurora
This is just a self-developed engine based on Raylib_cs. But I welcome more interested developers to use it, why not? (Anyway, I wrote the complete XML document)

## How can I used it?
First, make sure your project is using DotNet 10 or above.

Then, you can just clone this repository and reference it in your project.

You can also use nuget:
```shell
dotnet add package LibAurora --version 1.0.0
```

## What features dose it have? What should I implement by myself?
The following are list of features. Some of them might be still under development.

- [x] Independent Rendering Loop and Logic Loop
- [x] Multifunctional and High-performance Sprite Batch Drawing (Contains shader functionlity)
- [x] Mapping of Input and Action
- [x] Space Querying (SpatialGrid/QuadTree)
- [x] Object Pool
- [x] Event Bus

The folowing are of optional features. You can reference extra project of this sovlution.
- [x] ECS Management Based on [Arch](https://github.com/genaray/Arch)
- [x] GUI Service Based on [ImGui.NET](https://github.com/ImGuiNET/ImGui.NET)
- ~~Resource Management~~ (I dont think a basic lib needs this.)
- [ ] Simple Physics
- [ ] Audio Bus
- [ ] Tween
