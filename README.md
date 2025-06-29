Attention! Version 1.6 underwent various namespace changes; many existing XML and C# mods relying on it will break. Here's a summary of all namespace changes: https://github.com/Vanilla-Expanded/VanillaExpandedFramework/wiki/16Changes

Welcome to the Vanilla Expanded Framework!

![](https://raw.githubusercontent.com/Vanilla-Expanded/VanillaExpandedFramework/main/About/Preview.png)

The Vanilla Expanded Framework is a code library that adds some shared behaviours for several of the Vanilla Expanded series of mods. We have tried to document everything and to make it as modular as possible, so you can use it as a dependency in your own mods and access all these extra behaviours.

# How do I use the framework?

First of all, you'll need to add the framework as a dependency. You do this by adding this to your mod's about.xml file:

```xml
<modDependencies>
  <li>
    <packageId>OskarPotocki.VanillaFactionsExpanded.Core</packageId>
    <displayName>Vanilla Expanded Framework</displayName>
    <steamWorkshopUrl>https://steamcommunity.com/workshop/filedetails/?id=2023507013</steamWorkshopUrl>
    <downloadUrl>https://github.com/Vanilla-Expanded/VanillaExpandedFramework</downloadUrl>
  </li>
</modDependencies>
```

It is also recommended that you add in Steam this framework mod to your list of Required Items.

Then, you'll need to link the classes you want to use from your mod's XML code. Each class has a different way to use them, so we have divided the wiki into different pages explaining how to use most of them.

# Then what?
   
It is recommended that you read [our Wiki](https://github.com/Vanilla-Expanded/VanillaExpandedFramework/wiki) to know how to use all these libraries. Or you can YOLO it.
