## Required dependencies
Use the Plugin directory (`{armsym_root}/Assets/Plugins/`) as the root for all the dependencies. If any dependency creates a problem, don't hesitate to [open an issue](https://github.com/SamuelBG13/ArmSym/issues/new).

1. First install `MathNet.Numerics`. Use a version compatible with .NET 3.5., for instance ~3.20 (see https://answers.unity.com/questions/462042/unity-and-mathnet.html). You can also get it using a [C# package manager](https://numerics.mathdotnet.com/Packages.html) or access the [MathNet binary archive](https://archive.mathdotnet.com/Math.NET%20Numerics/Zip/). You will also need the system threading dll, distributed with `MathNet.Numerics`.

2. SteamVR version [1.2.3](https://github.com/ValveSoftware/steamvr_unity_plugin/tree/1.2.3).
One way to install it is to copy the SteamVR assets into `{armsym_root}/Assets/Plugins/SteamVR`.

3. Install VRTK, version [~3.2](https://github.com/ExtendRealityLtd/VRTK/tree/3.2.0/Assets/VRTK).
Likewise, one way to install it is to copy the VRTK assets into `{armsym_root}/Assets/Plugins/VRTK`.


## Other dependencies
- We recommend adding the [LSL4Unity](https://github.com/labstreaminglayer/LSL4Unity) package to the plugin directory (`{armsym_root}/Assets/Plugins/`), enabling biosignal integration. If you don't intent to use it, you can modify the source code to remove the imports. 
