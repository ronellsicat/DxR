# Tutorial

## Overview

[TODO]

DxR uses JSON specs [temporarily documented here](https://docs.google.com/spreadsheets/d/1MykCFZxE1f-NnCWADAPwEab72OFV3FdGYndidCshSpI/edit?usp=sharing) to describe a visualization. It is based on a similar syntax inpired by the well-designed [Vega-Lite](https://vega.github.io/vega-lite/) - a high-level grammar of interactive graphics for SVG or Canvas -based data visualizations.

This document provides instructions on how to create data-driven graphics using DxR on different "authoring layers" (user and author are used interchangeably):

1. Minimum high-level specs - author just needs to provide bare minimum specs, e.g., data, mark, channel, field, data-type. DxR infers and generates the best options for the full high-level specs to create the visualization.
2. Full high-level specs - author can specify some or all detailed specs of the visualization.
3. Custom marks and channels - author can create own custom marks and channels and use either minimum or full high-level specs to create custom visualizations. This authoring layer requires some minor low-level programming.

## Minimum high-level specs: 

Users will typically just have to use prefabs found in DxR/Prefabs and create a specification in StreamingAssets/DxRSpecs to create basic visualizations (optionally put data files in StreamingAssets/DxRData).

### Steps:

1. Add a DxRSceneObject into your scene by dragging and dropping the DxRSceneObject prefab (found in Assets/DxR/Prefabs/) into your Unity Hierarchy.
2. Select the object in your hierarchy to show parameters in the inspector windos. Set parameters as needed - particularly the filename for the specification, e.g., DxRData/vis_spec.json. Note that this filename is relative to the Assets/StreamingAssets/ folder.
3. Create a specification file, e.g., DxRData/vis_spec.json, inside Assets/StreamingAssets/ folder.
4. Modify the contents of specification file to create your visualization as desired. The grammar for specifying visualizations is described here [TODO]. You can also look at [Vega-Lite](https://vega.github.io/vega-lite/)'s grammar for reference.
5. Your specified visualization will get created automatically when you play the scene.

## Full high-level specs:

[TODO]

## Custom marks and channels:

[TODO]

### Sharing 

[TODO]

To allow other users to use your custom marks and channels, simply create a package (Assets -> Export Package) with ONLY your mark directory checked, e.g., DxR/Resources/Marks/mycustommark. Once another user imports your package, your custom mark will show up in their DxR/Resources/Marks folder.
  
