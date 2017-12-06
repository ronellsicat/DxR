# Development

This document describes the development structure of DxR which could help developers add features.

## Overview [TODO]

## Folder Structure

Once the DxR.unitypackage is imported in a project, the following folder structure will be added.
    
    DxR/
      Prefabs/          - contains prefabs that users can instantiate in their scene.
      Resources/        - contains graphics objects (e.g., prefabs, models, textures) and scripts used by toolkit.
        Axis/           - contains objects and scripts for creating axes.
        ColorSchemes/   - contains JSON specs of color schemes.
        Legend/         - contains objects and scripts for creating legends.
        Marks/          - contains objects and scripts for all marks.
        Materials/      - contains materials used by DxR.
      Scripts/          - contains core scripts used by toolkit.
  
    DxRExamples/        - contains examples of scenes with DxR components.

    External/           - contains external dependencies.
    
    HoloToolkit/        - Microsoft's Mixed Reality Toolkit; provided for convenience.

    StreamingAssets/ 
      DxRData/          - contains data files.
      DxRSpecs/         - contains JSON specification files.
  
## DxR.unitypackage

Whenever a stable version is reached, a unitypackage (DxR.unitypackage) file is generated and placed in the root directory of the GitHub repository. This makes it easy to download and import into any existing Unity project. This is also the reason why some folder names have DxR prepended to them - to make it easier to distinguish from folders in existing projects.
