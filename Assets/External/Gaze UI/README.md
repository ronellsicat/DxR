# Introduction

Gaze UI is a simple asset using raycasting to provide a Gaze Controller working with native UI.

Currently supporting :
* Button
* Toggle
* Dropdown
* Slider
* Scrollbar

# How to use

1. Drag and drop the "Eye Raycaster" prefab in your scene.
2. Set the desired Camera in the "Eye Raycaster"'s Canvas.
3. Configure the two parameters in the editor for EyeRaycaster.cs :
	* [Float] Loading Time - Amount of time the viewer must stare at his target before activation
	* [Event] On Load - Notify gaze progress (float parameter is normalized time)

# Contact

Feel free to contact me is you have any question :
youe.graillot@gmail.com

If you have suggestions :
https://gitlab.com/youe_unity/unityassets_gazeui/issues/new