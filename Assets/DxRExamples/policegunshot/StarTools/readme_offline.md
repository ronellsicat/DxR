# Star Creation Tools for Unity #
 
[Unity Asset Store Page](https://www.assetstore.unity3d.com/en/#!/content/80595)

[Video Demo](https://www.youtube.com/watch?v=fIn6SE-O1SM)

[Current FAQ](https://gist.github.com/Jacob-Lane/c735897fc8a1760d7d5944a682f2ce86)

## How to use ##

Simply drag or instantiate a copy of the star prefab into your scene. Creating a new star from scratch is not recommended. When selecting a prefab, you can select between Manual Color or Scientific Color, and Thick or Thin Coronas. Thin coronas are generally recommended, as thick coronas have a significantly higher performance impact.

You can click on the star to open it's inspector, or change most settings programatically from the 'Star' script. Settings include:

**Enable manual color selection -** Select to enter a color, or unselect to specify a temperature or B-V value to automatically generate a color.

**Paused -** When selected, freezes the texture, rotation rate, and particle system. Can be set with Star.Pause(), Star.Unpause(), or Star.TogglePause().

**Time scale -** How quickly the star's texture and rotation changes in appearance.

**Resolution -** How 'Zoomed in' the star's texture is. You should change this when adjusting the star's scale.

**Contrast -** How varied the final texture will be from the base star color.

**Rotation rates -** How quickly the star will spin in each direction.
 
## FAQ ##

### What range of temperature/B-V values can be represented when using accurate colors? ###

Stars can range from 1195 to 113017 Kelvin, or -0.4 to 2.0 B-V. Anything beyond that is not accurate.

### Why aren't a star's size or its light's brightness properties set automatically by the tool? ###

This was an intentional design descision. Because of the wide varieties of use cases in scales and lighting, these are left up to the user. The prefabs come with built-in point lights and automatically set their colors, but not distance or brightness. Also note that star textures are always fully lit, so extremely low-brightness stars such as brown dwarves may not appear correctly.

### Why can't I rotate stars using Unity's built-in rotation system? Doesn't this hurt performance? ###

The tool's shaders require complete control over the rotation so that the corona appears to blend seamlessly with the rest of the star's texture. You can still modify the rates at which a star spins.

### I'm having display problems working with large coordinates. Is this because of your tool? ###

No, Unity can struggle with large coordinates and scales, which you might see when trying to create space-sized games or simulations. You can read more about this problem [here](http://davenewson.com/posts/2013/unity-coordinates-and-scales.html). 

## Contact info ##

Questions? Comments? Suggestions?

Shoot me a quick email at <jacob.lane.p@gmail.com>. I try to respond to emails within a day when possible.

## Update History ##

### 1.0.3 ###

Released October 12th, 2017

-Fixed a bug in the corona shader on mobile (Thanks Leo!)

-Improved performance by switching from spheres to hemispheres

### 1.0.2 ###

Released April 10th, 2017

-Added Pause/Unpause functionality

-Time scale can now be adjusted in real time, and affects rotation rates.

### 1.0.1 minor update ###

Released April 7th, 2017

-Added this README/FAQ

### 1.0.1 ###

Released April 3, 2017

-Improved camera controls

-Fixed color lookup tables to make low temperature stars darker

-Allowed overwrite of color/temperature/B-V lookup tables

### 1.0.0 ###

Released February 2, 2017

-Initial release