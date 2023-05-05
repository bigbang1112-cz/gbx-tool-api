# Gbx Tool API (powered by [GBX.NET](https://github.com/BigBang1112/gbx-net))

Gbx Tool API library integrates tools into Gbx Web Tools in both serverside and clientside ways.

That way it is possible to make conventions that allow easier Gbx tool development, while also not completely rely on the Gbx Web Tools project.

**It is important to note that Gbx Tool API can still introduce major breaking changes without notice due to its early development stage.**

Features of this library set are split in this way:

- GbxToolAPI
- GbxToolAPI.Client
- GbxToolAPI.Server
- GbxToolAPI.CLI
- GbxToolAPI.Lua *(coming soon)*

### GbxToolAPI

This is the base library, used by all the other sub-libraries above *(except GbxToolAPI.Server, temporarily)*. It has the tool skeleton logic, and other features suitable for most tool scenarios.

Reference this library in your Gbx tool **library** project, if it is supposed to run in both wbe browser and CLI.

### GbxToolAPI.Client

This has everything from GbxToolAPI + Razor web features on top.

If you want to enhance web UI of your tool that is also supposed to work on CLI, create a `[YourTool].Client` Razor library and reference `GbxToolAPI.Client` there (don't reference it in your main tool library project). If your tool is only supposed to work in web browser, directly reference `GbxToolAPI.Client` and don't bother about the base `GbxToolAPI` library.

### GbxToolAPI.Server

This allows your tool to communicate with databases and websocket connections. In the future, this will also allow serversided executions of the tools, like Discord bot or REST API endpoints for example.

## Usage

To build a Gbx tool utilizing Gbx Tool API:

1. Create a library project that will be the base of everything

2. Reference GbxToolAPI in that project

```
exact commands soon
```

3. Create a class called `[YourToolName]Tool.cs`:

```cs
using GBX.NET.Engines.Game;
using GbxToolAPI;

namespace [YourToolNamespace];

[ToolName("[Your Tool Name]")]
public class [YourToolName]Tool : ITool
{
    private readonly CGameCtnChallenge map;

    public [YourToolName]Tool(CGameCtnChallenge map)
    {
        this.map = map;
    }
}

```

4. This on its own cannot do anything, but you can add "extensions" to the tool via interfaces.

5. For example, add `IHasOutput<T>`, which will implement `Produces()` method returning `T` as the produced result. Currently I only recommend using `Node` or `NodeFile<T> where T : Node` classes in there.

```cs
using GBX.NET.Engines.Game;
using GbxToolAPI;

namespace [YourToolNamespace];

[ToolName("[Your Tool Name]")]
public class [YourToolName]Tool : ITool, IHasOutput<CGameCtnMediaClip>
{
    private readonly CGameCtnChallenge map;

    public [YourToolName]Tool(CGameCtnChallenge map)
    {
        this.map = map;
    }
    
    public CGameCtnMediaClip Produce()
    {
        return map.ClipIntro; // Some advanced conversion
    }
}

```

6. For example, add configuration of the tool via `IConfigurable<TConfig> where TConfig : Config`. You should create your own configuration class (standard name is `[YourToolName]Config`).

