# Setup

This document provides instructions for setting up DxR in Unity for xR applications. If you are familiar with xR development and already have an xR application up and running, you only have to import [DxR.unitypackage]() [TODO] into your project and start following the [authoring instructions](AUTHORING.md). Otherwise, follow the instructions below. 

## Requirements

Note that DxR is based on the Microsoft Mixed Reality ecosystem with: 

1. Unity3D Editor [version 2017.2.0p1-MRTP4](http://beta.unity3d.com/download/b1565bfe4a0c/UnityDownloadAssistant.exe) 
2. Mixed Reality Toolkit [v1.2017.1.2](https://github.com/Microsoft/MixedRealityToolkit-Unity/releases/tag/v1.2017.2.0)

## Getting Started

1. Create/Open your a project in Unity. 
2. Import the DxR package [DxR.unitypackage]() [TODO] into your Unity project.
3. You can build your scene from scratch, following instructions [here]() [TODO], or you can also use a DxR template scene [DxRExamples/template.unity]() [TODO] with a basic set-up and an example visualization.
4. Depending on your target xR environment, follow the debugging instructions below.
Microsoft's initial set-up instructions [here](https://developer.microsoft.com/en-us/windows/mixed-reality/unity_development_overview). Then follow the steps below depending on your target xR system. 

## Debugging DxR

The following instructions are based on the [Mixed Reality Toolkit getting strated guide](https://github.com/Microsoft/MixedRealityToolkit-Unity/blob/master/GettingStarted.md) and [Microsoft's Unity development overview](https://developer.microsoft.com/en-us/windows/mixed-reality/unity_development_overview).

### VR Debugging (Immersive Headsets or IHMD)
1. Make sure your Target Device (under File -> Build Settings) is set to "Any Device".
2. Connect your IHMD to your computer. Open your Mixed Reality Portal.
3. Add a DxRView object into your scene using the steps below (Creating a DxR Visualization).
4. Press play - your Unity editor should run your application but should also show up on your IHMD in VR.

### MR Debugging (Hololens)
1. Make sure your Target Device (under File -> Build Settings) is set to "HoloLens".
2. Put on your HoloLens and run the Holographic Remoting application (install if not yet installed). 
3. In Unity, go to Window -> Holographic Emulation. In the window that pops up, set Emulation Mode to "Remote to Device". Set Remote Machine to your HoloLens IP address (this should show up when you run the Holographic Remoting app). After typing in the IP address, with your cursor still in the Remote Machine form, make sure you press Enter key to submit the IP.
4. Press "Connect" button. If successful, the window should indicate so. If not, check your connection, e.g., make sure that your computer is on the same network as the HoloLens.
5. Add a DxRView object into your scene using the steps below (Creating a DxR Visualization).
6. Press play - your Unity editor should run your application but should also show up on your HoloLens.

### Developer notes
1. Once you've done the set-up for either VR or MR development as instructed above, you can easily switch between the two during debug mode by simply changing the Target Device option from "Any Device" to "HoloLens" in order to target IHMDs or HoloLens, respectively.

[More developer notes can be found here.](Development.md)

## Creating a DxR Visualization

Instructions for creating or authoring a DxR visualization can be found [here](AUTHORING.md).

## Device Deployment

These are some guidelines for deploying your application to device. More information can be found [here](https://developer.microsoft.com/en-us/windows/mixed-reality/using_visual_studio).

<!--
### VR Deployment (Immersive Headsets or IHMD)
1. Build the solution (File -> Build Settings -> Build). 
2. Open the generated/updated MS Visual Studio solution (using VS 2017). 
3. In Visual Studio, set the build settings to x86 and Local Machine. 
4. Deploy.
-->

### MR Deployment (Hololens)
1. Build the solution (File -> Build Settings -> Build). 
2. Open the generated/updated MS Visual Studio solution (using VS 2017). 
3. In Visual Studio, set the build settings to x86 and Remote Machine. Go to Debug -> Test Properties -> Configuration Properties -> Debugging and set "Machine Name" to your HoloLens IP address. Also make sure "Authentication Type" is set to Universal.
4. Deploy.
