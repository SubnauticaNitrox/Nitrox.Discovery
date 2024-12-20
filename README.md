# Nitrox.Discovery

[![NuGet](https://img.shields.io/nuget/v/Nitrox.Discovery.MSBuild?label=Nitrox.Discovery.MSBuild&logo=NuGet)](https://www.nuget.org/packages/Nitrox.Discovery.MSBuild)

Discovers the path to an installed game from a given game name. Supports the most popular game stores and on both Windows and Linux.

### Use with MSBuild (minimized setup)

```xml
<Target Name="FindGameAndIncludeReferences" BeforeTargets="ResolveAssemblyReferences" Condition="'$(_NitroxDiscovery_TaskAssembly)' != ''">
    <DiscoverGame GameName="Subnautica"> <!-- CHANGE THIS -->
        <Output TaskParameter="GamePath" PropertyName="GameDir" />
    </DiscoverGame>
    <Error Condition="'$(GameDir)' == ''" Text="Failed to find the game 'Subnautica' on your machine" />

    <!-- Load any references to game DLLs here -->
    <ItemGroup>
        <Reference Include="MyGameDll">
            <HintPath>$(GameDir)\bin\MyGameDll.dll</HintPath>
        </Reference>
    </ItemGroup>
</Target>
```

> [!IMPORTANT]
> `Condition="'$(_NitroxDiscovery_TaskAssembly)' != ''"` is needed so Visual Studio can still load Nuget packages without requiring `DiscoverGame` task to exist!

### Recommended setup

```xml
<Target Name="FindGameAndIncludeReferences" BeforeTargets="ResolveAssemblyReferences" Condition="'$(_NitroxDiscovery_TaskAssembly)' != ''">
    <PropertyGroup>
        <!-- Change this (optional: can be put in your Directory.Build.props file) -->
        <GameName>MY_GAME</GameName>
    </PropertyGroup>
    <DiscoverGame GameName="$(GameName)">
        <Output TaskParameter="GamePath" PropertyName="GameDir" />
    </DiscoverGame>
    <Error Condition="'$(GameDir)' == ''" Text="Failed to find the game '$(GameName)' on your machine" />
    <PropertyGroup>
        <GameDir>$(GameDir)\</GameDir>
    </PropertyGroup>
    <!-- Optional: do other checks on game (e.g. version check) -->
    <Message Importance="high" Text="Game found at: '$(GameDir)'" />

    <!-- Load any references to game DLLs here -->
    <ItemGroup>
        <Reference Include="MyGameDll">
            <HintPath>$(GameDir)bin\MyGameDll.dll</HintPath>
        </Reference>
    </ItemGroup>
</Target>
```

### You can append other references on a per project basis like this

```xml
<Target Name="MoreGameReferences" AfterTargets="FindGameAndIncludeReferences">
    <ItemGroup>
        <Reference Include="SomeOtherGameDll">
            <HintPath>$(GameDir)bin\SomeOtherGameDll.dll</HintPath>
        </Reference>
    </ItemGroup>
</Target>
```
