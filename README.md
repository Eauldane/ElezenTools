# ElezenTools

ElezenTools is designed as a beginner-friendly, opinionated and stable library for 
FFXIV mod development.

The project exists to smooth over the parts of plugin development that tend to be awkward for 
newcomers, or those who want to use modding as their first foray into development. 

Instead of asking users to work directly with pointers, Lumina rows, or ClientStructs 
internals, ElezenTools provides a more stable, higher-level interface that is easier to 
read, easier to learn, and easier to keep using as the surrounding APIs change.

## Project goals

ElezenTools has two main goals:

- make common Dalamud plugin tasks easier for first-time developers
- provide a stable compatibility layer so plugins can depend on ElezenTools 
rather than directly on implementation details that may shift between API versions

In practice, that means that developers can work with ElezenTools provided objects that wrap
lower-level APIs, reducing the need to touch unsafe code or pointer-heavy functions. We also aim 
to allow you to avoid direct Lumina and ClientStructs usage for common tasks, giving plugin authors
a cleaner surface area that can remain stable even when the underlying implementation needs to 
change.

In short; ElezenTools aims to provide functionality that lets you simply mark your plugin as compatible
with the latest API version with minimal changes, as well as make the task of making a plugin in the
first place seem less daunting to those who haven't used Dalamud, or perhaps even coded anything before. 

## Who this is for

ElezenTools is primarily aimed at:

- developers starting their first Dalamud plugin
- developers who want a simpler, more opinionated API for common plugin tasks
- developers who would rather depend on a stable wrapper than track every underlying
API change themselves

If you are already comfortable working directly with Dalamud, Lumina, and ClientStructs, 
you may not need ElezenTools for everything, or perhaps even anything. The library is designed to 
be a practical abstraction, not a replacement for understanding the underlying ecosystem.

Those who start off using ElezenTools are more than welcome to remove helpers as they become more
comfortable with the underlying structure, and our code is designed to be simple enough to understand
as a newbie if you're interested in an implementation detail that you should be able to figure it out.

## Installation

The expected way to use ElezenTools is as a NuGet package.

If using the CLI: 

```powershell
dotnet add package ElezenTools
```

If using an IDE with a package browser, simply search by name.

After adding the package, reference the namespace you need in your plugin.

## Quick start

The first step is initialising ElezenTools from your plugin.

If you started from Dalamud's sample plugin, you can remove the sample's `[PluginService]` 
lines and let ElezenTools handle service access through its own `Service` class.

```csharp
using Dalamud.Plugin;
using ElezenTools;

public sealed class Plugin : IDalamudPlugin
{
    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        ElezenInit.Init(pluginInterface, this);
    }

    public void Dispose()
    {
        ElezenInit.Dispose();
    }
}
```

Once initialised, you have the option to call our helper functions to return easy-to-use
objects. The following example demonstrates how to look up world data for the current player:

```csharp
using ElezenTools.Data;
using ElezenTools.Services;

public void PrintCurrentWorld()
{
    var player = Service.PlayerState;
    if (player == null)
    {
        return;
    }

    var world = ElezenData.Worlds.GetById(player.CurrentWorld.RowId);
    if (world == null)
    {
        return;
    }

    Service.ChatGui.Print($"{player.CharacterName} is currently on {world.Name} in {world.DataCenterName} ({world.RegionName}).");
}
```
A WorldData object is returned with commonly used attributes, so that you don't need to query 
Lumina directly for them. Text values will be automatically
converted to the client's current language unless directly overridden.


ElezenTools can also expose the Dalamud services it manages and provide its own
higher-level helpers on top of them. For example, services such as `IFramework`, `IChatGui`,
`IDataManager`, and `IDalamudPluginInterface` are exposed through `ElezenTools.Services.Service`.

The following example prints a message to the chatbox:

```csharp
using ElezenTools.Services;

public void SayHello()
{
    Service.ChatGui.Print("Hello from ElezenTools.");
}
```

ElezenTools also includes useful ImGui helpers and SeString builders.

To build a coloured `SeString` from a hex code and show it in the chatbox, you might do the following:

```csharp
using ElezenTools.Services;
using ElezenTools.UI;

public void PrintStyledMessage()
{
    var colours = new ElezenStrings.Colour(
        foreground: Colour.HexToColour("#8CC4FF"),
        glow: Colour.HexToColour("#23374D"));

    var message = ElezenStrings.BuildColouredString("Hello from ElezenTools.", colours);
    Service.ChatGui.Print(message);
}
```
Note: Colour is a helper struct provided by ElezenTools. Note the use of British English.

## Why initialise with `ElezenInit`?

`ElezenInit.Init(...)` sets up ElezenTools against your plugin's `IDalamudPluginInterface` and 
gives the library access to the Dalamud services it wraps.

This gives you a single setup step at plugin start-up, after which you can use ElezenTools 
instead of wiring up service access yourself throughout the project.

### Async Plugins

As of 15.0.0, ElezenTools supports async plugins using the `IAsyncDalamudPlugin` interface.

```csharp
using Dalamud.Plugin;
using ElezenTools;

public sealed class Plugin : IAsyncDalamudPlugin
{
    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        ElezenInit.Init(pluginInterface, this);
    }

    public Task LoadAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;

    public ValueTask DisposeAsync()
    {
        ElezenInit.Dispose();
        return ValueTask.CompletedTask;
    }
}

```

You can also use the lifecycle-agnostic `Assembly` instantiation method if you might switch to async plugins later:

`ElezenInit.Init(pluginInterface, GetType().Assembly);`

## Updates and Versioning

Versions are numbered in three parts, roughly equating to SemVer:

Example: 15.0.1

15 is the Dalamud API version, and is bumped when Dalamud releases a major API version. The 0 is reserved for 
post-major update bumps - for example, 7.51s release will bring 15.1.0 The final number is the patch release 
for ElezenTools - this increments whenever a change is published.

As a general rule, if your project works fine with the current version of ElezenTools, just update
whenever you feel like it - these versions will add new features or optimisations that may or may not be useful, so
there's no need to rush a release.

If a version comes out that adds a feature you need or a bugfix in an area you're using it, update to it.

When Dalamud releases a major API version, update as soon as it's available - we can't guarantee compatibility otherwise.

ElezenTools aims for, at minimum, a full expansion worth of compatibility. In other words, any changes made in the
middle of FFXIV 8.0 will be supported until at least FFXIV 10.0's release. ElezenTools will handle the Dalamud wrangling.

## Current direction

ElezenTools is intended to grow into a shared toolbox for common plugin tasks, especially the 
kinds of tasks that otherwise lead projects to reimplement the same patterns 
slightly differently whenever there's an update.

That includes:

- beginner-friendly wrappers around common Dalamud interactions
- stable interfaces over parts of the ecosystem that are more likely to shift over time
- helpers that keep plugin code clearer and less fragile

Additional usage examples and feature-specific documentation can be added over time 
as the library surface grows.

## FAQ

### New Users

*Q: What problem does ElezenTools solve that Dalamud doesn't?*

A: ElezenTools is just a wrapper around what Dalamud already gives you, but designed to be easier
to use. In the olden days, game modding was a lot of people's first introduction to development - 
but Dalamud is pretty intimidating to use as a newbie developer, which is where ElezenTools aims to 
step in. The ultimate goal is to get more people making plugins with a significantly lowered barrier
of entry. 

*Q: Do I need to understand pointers, unsafe code, Lumina, or ClientStructs to use ElezenTools?*

A: No. While you'll need to learn these eventually if you want to use raw Dalamud, ElezenTools 
abstracts this away from you so you can focus on your idea. With that in mind, there's a lot of
areas we don't cover yet - if we're missing something, open a Feature Request in the issues tab
and I'll get it implemented ASAP! 

*Q: Can I still use raw Dalamud APIs alongside ElezenTools?*

A: Yes! I do this in my plugins where it makes sense to. ElezenTools is designed to be easy enough
to remove once you're more comfortable with something that you're not locked into using it forever if
using Dalamud raw ends up better for you.

*Q: What happens if a lookup returns `null`?*

A: Usually this means what you were looking for wasn't found. For example, if using `GetByName`, one of two
things likely happened: there's a typo, or client language is interfering. If you know the source text is in a
different language, you can override the lookup language first, then use the returned territory row ID for a
language-independent lookup:

```csharp
using Dalamud.Game;
using ElezenTools.Data;
using ElezenTools.Services;

public void PrintOldGridania()
{
    const string oldGridaniaJapanese = "グリダニア：旧市街";

    // This will return null on an English client, because the default lookup
    // uses the client's current language.
    var location = ElezenData.Locations.GetByName(oldGridaniaJapanese);
    if (location != null)
    {
        Service.ChatGui.Print(location.Value.Name);
        return;
    }

    // Override the language so the Japanese name can be resolved correctly.
    var japaneseLocation = ElezenData.Locations.GetByName(oldGridaniaJapanese, ClientLanguage.Japanese);
    if (japaneseLocation == null)
    {
        Service.ChatGui.Print("Could not find Old Gridania.");
        return;
    }

    // TerritoryId is stable regardless of client language, so you can use it
    // to fetch the same location back in the player's current language.
    var localisedLocation = ElezenData.Locations.GetByTerritoryId(japaneseLocation.Value.TerritoryId);
    if (localisedLocation == null)
    {
        Service.ChatGui.Print("Resolved the Japanese row, but could not localise it.");
        return;
    }

    Service.ChatGui.Print($"Found: {localisedLocation.Value.Name} ({localisedLocation.Value.AreaName})");
}
```

*Q: Can I use ElezenTools even I'm still learning how to code/ how to use C#?*

A: Yes! However, note that ElezenTools is opinionated - we ignore some of the "proper" conventions
of C# in favour of getting things done. Ignoring that, you should still be able to get a solid
foundation to build off later.

### Existing Plugin Devs

*Q: Why would I use ElezenTools if I already know Dalamud and ClientStructs?*

A: If you're already comfortable, there's no need to switch. The main reason would be if you like
having stable object interfaces to play with, or if you're the type who wants to write as little code as
possible and just change a version number occasionally.

*Q: How does ElezenTools help with compatibility between API bumps?*

A: The aim is that the interface with our helper objects is stable. When Dalamud updates, we update
the library to handle the changed surfaces and wrangle anything that needs wrangling into the same data
format that we already had, so for plugins consuming the library, nothing has changed.

*Q: Is ElezenTools intended to replace Dalamud, or sit on top of it?*

A: Sit on top of it. ElezenTools is really just a helper library to simplify things.

*Q: Can I adopt ElezenTools gradually in an existing plugin?*

A: Yes - this is how I'm doing it myself in Snowcloak and my other plugins. As things change in Dalamud
or I actively work on a component, I replace things as I go - this is a perfectly viable way of doing things.
The only caveat to bear in mind is ElezenTools' helper objects - if you cross class/function boundaries, 
you're handing stuff one of our things. However, anything that uses Lumina contains an ID reference so you can pull
Lumina rows if needed.

*Q: Does ElezenTools expose the underlying Dalamud services if I still need them?*

A: Yes - `ElezenTools.Services` contains a `Service` class that can be used to call things as needed - for example `Service.ChatGui`
is an instance of IChatGui. 

*Q: How opinonated is ElezenTools, and where does it intentionally differ from raw Dalamud usage?*

A: Fairly opinionated. ElezenTools is geared towards getting stuff done rather than looking clever while you do it.
Usage is aimed at abstracting away the need to do anything with pointers, Lumina, or ClientStructs, so it's fairly different
from how Dalamud currently wants you to do things.

*Q: If ElezenTools changes internally, how stable is the public interface meant to be?*

A: The public interface is intended to not change. The internals will handle any CS or Dalamud wrangling necessary 
to ensure that public facing APIs provide the same data for a minimum of two years. For example, if Dalamud changes something
for game patch 8.1, ElezenTools won't consider changing the way the data was represented in 7.5 until at least game patch 9.0.

*Q: How should I handle features ElezenTools doesn't wrap yet?*

A: You can either implement it the traditional way, make a feature request and wait for me to implement it, or make a PR
implementing it for everyone else! 

*Q: Is ElezenTools aimed more at convenience, stability, or both?*

A: Primarily convenience, with stability very closely behind. The ultimate goal is for people who aren't subbed to 
the game currently to be able to just update their plugin's JSON to the latest Dalamud API version and rebuild.

### General FAQ

*Q: Is ElezenTools suitable for production plugins?*

A: Yes. ElezenTools has been used in Snowcloak for a couple of months now, and I dogfood it in every
other plugin I'm working on.

*Q: Can plugins made using ElezenTools be added to the official Dalamud repository?*

A: I don't see why not. Some of the PAC don't like plugins that use libraries, but there's nothing inherent
to ElezenTools that'd disqualify you.

*Q: Does ElezenTools affect performance in any meaningful way?*

A: Not that I've been able to measure - it'll cache any heavy lookups to prevent having to repeat them.

