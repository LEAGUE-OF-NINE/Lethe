# Custom Encounter

This is a BepInEx mod for Limbus Company which allows loading custom identities, fights, skills and other stuff.

# Installation

1. Download BepInEx #577 from [here](https://builds.bepinex.dev/projects/bepinex_be)
2. Unzip the content in the Limbus Company folder where `Limbus Company.exe` lives
3. Put the dll of the mod in the `BepInEx/plugins` folder

# Development

1. Make sure you have installed BepInEx as per the instructions above, and that you have launched Limbus Company with
   BepInEx installed at least once
2. To setup a dev environment, copy the file `Directory.Build.example.props` in `src/` and rename the copy to
   `Directory.Build.props`
3. Modify the file as per the instructions inside
4. Whenenver you build, the development build of the BepInEx plugin should automatically be copied to your game folder
