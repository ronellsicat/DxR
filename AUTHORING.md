# Authoring

This document provides instructions on how to create data-driven graphics using DxR on different "authoring layers" (user and author are used interchangeably):

1. Minimum high-level specs - author just needs to provide bare minimum specs, e.g., data, mark, channel, field, data-type. DxR infers and generates the best options for the full high-level specs to create the visualization.
2. Full high-level specs - author can specify some or all detailed specs of the visualization.
3. Custom marks and channels - author can create own custom marks and channels and use either minimum or full high-level specs to create custom visualizations. This authoring layer requires some minor low-level programming.

## Minimum high-level specs: 

Users will typically just have to use prefabs found in DxR/Prefabs and create a specification in StreamingAssets/DxRSpecs to create basic visualizations (optionally put data files in StreamingAssets/DxRData).

### Steps:

1. Add a DxRView object into your scene by dragging and dropping the DxRView prefab (found in Assets/DxR/) into your Unity Hierarchy.
2. Select your instance to set parameters as needed, particularly the filename for the specification, e.g., vis_spec.json.
3. Create a specification file, e.g., vis_spec.json, inside Assets/StreamingAssets folder.
4. Modify the contents of specification file to create your visualization as desired. The grammar for specifying visualizations is described here [TODO]. 
5. Your specified visualization will get created automatically at run time.

## Full high-level specs:

[TODO]

## Custom marks and channels:

[TODO]

### Sharing 

To allow other users to use your custom marks and channels, simply create a package (Assets -> Export Package) with ONLY your mark directory checked, e.g., DxR/Resources/Marks/mycustommark. Once another user imports your package, your custom mark will show up in their DxR/Resources/Marks folder.
  
