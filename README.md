# DxR
This toolkit makes it easy to create <b>D</b>ata-driven graphics in <b>xR</b>, i.e., MR/VR/AR. The following instructions are based on the Microsoft Mixed Reality ecosystem with Unity3D version 2017.2.0f3.

## Set-up

First, import the DxR package (DxR.unitypackage found in the top-level directory) into your Unity3D project. Then follow Microsoft's initial set-up instructions [here](https://developer.microsoft.com/en-us/windows/mixed-reality/unity_development_overview). Then follow the steps below depending on your target xR system. 

### VR Debugging (Mixed Reality Headsets)
1. Make sure your Target Device (under File -> Build Settings) is set to "Any Device".
2. Connect your VR headset to your computer. Open your Mixed Reality Portal.
3. Add a DxRView object into your scene using the steps below (Creating a DxR Visualization).
4. Press play - your Unity editor should run your application but should also show up on your headset in VR.

### MR Debugging (Hololens)
1. Make sure your Target Device (under File -> Build Settings) is set to "HoloLens".
2. Put on your HoloLens and run the Holographic Remoting application (install if not yet installed). 
3. In Unity, go to Window -> Holographic Emulation. In the window that pops up, set Emulation Mode to "Remote to Device". Set Remote Machine to your HoloLens IP address (this should show up when you run the Holographic Remoting app). After typing in the IP address, with your cursor still in the Remote Machine form, make sure you press Enter key to submit the IP.
4. Press "Connect" button. If successful, the window should indicate so. If not, check your connection, e.g., make sure that your computer is on the same network as the HoloLens.
5. Add a DxRView object into your scene using the steps below (Creating a DxR Visualization).
6. Press play - your Unity editor should run your application but should also show up on your HoloLens.

### Developer notes
1. Once you've done the set-up for either VR or MR development as instructed above, you can easily switch between the two during debug mode by simply changing the Target Device option from "Any Device" to "HoloLens" in order to target VR headsets or HoloLens, respectively.

## Creating a DxR Visualization
1. Add a DxRView object into your scene by dragging and dropping the DxRView prefab (found in Assets/DxR/) into your Unity Hierarchy.
2. Select your instance to set parameters as needed, particularly the filename for the specification, e.g., vis_spec.json.
3. Create a specification file, e.g., vis_spec.json, inside Assets/StreamingAssets folder.
4. Modify the contents of specification file to create your visualization as desired. The grammar for specifying visualizations is described here [TODO]. 
5. Your specified visualization will get created automatically at run time.
