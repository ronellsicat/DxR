# Development

## Folder Structure
    
    DxR/
      Prefabs/    - contains prefabs that users can instantiate in their scene.
      Resources/  - contains graphics objects (e.g., prefabs, models, textures) and scripts used by toolkit.
        Axis/   
        Marks/  - contains objects and scripts for core marks.
      Scripts/    - contains core scripts used by toolkit.
  
    DxRExamples/  - contains examples of scenes with DxR components.

    External/     - contains external dependencies such as Mixed Reality Toolkit.

    StreamingAssets/ 
      DxRData/    - contains data files.
      DxRMarks/   - contains user created marks.
      DxRSpecs/   - contains JSON specification files.
  
## For developers

Whenever a stable version is reached, a unitypackage (DxR.unitypackage) file is generated and placed in the root directory of the GitHub repository. This makes it easy to download and import into any existing Unity project. This is also the reason why some folder names have DxR prepended to them - to make it easier to distinguish from folders in existing projects.

## For users

Users will typically just have to use prefabs found in DxR/Prefabs and create a specification in StreamingAssets/DxRSpecs to create basic visualizations (optionally put data files in StreamingAssets/DxRData). To create their own marks and channels, they will have to add their marks objects in StreamingAssets/DxRMarks.

Users can also build from the example scenes in DxRExamples folder.
  
