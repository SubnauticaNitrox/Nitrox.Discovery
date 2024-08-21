# Nitrox.Discovery

[![NuGet](https://img.shields.io/nuget/v/Nitrox.Discovery.MSBuild?label=Nitrox.Discovery.MSBuild&logo=NuGet)](https://www.nuget.org/packages/Nitrox.Discovery.MSBuild)

Discovers the path to an installed game from a given game name.

### Use with msbuild
```xml
<Target Name="FindGameAndIncludeReferences" BeforeTargets="ResolveAssemblyReferences">
    <DiscoverGame GameName="MY_GAME" IntermediateOutputPath="$(IntermediateOutputPath)">
        <Output TaskParameter="GamePath" PropertyName="GameDir" />
    </DiscoverGame>
    <Error Condition="'$(GameDir)' == ''" Text="Failed to find the game 'MY_GAME' on your machine" />
    <PropertyGroup>
        <GameDir>$(GameDir)\</GameDir>
    </PropertyGroup>
    <Message Importance="high" Text="Game found at: '$(GameDir)' with version @(GameVersion)" />
    
    <!-- Load any references to game DLLs here -->
    <ItemGroup>
        <Reference Include="MyGameDll">
            <HintPath>$(GameDir)\bin\MyGameDll.dll</HintPath>
        </Reference>
    </ItemGroup>
</Target>
```
