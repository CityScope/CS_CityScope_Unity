## Overview

This is a barebone template project for MIT CityScope development in Unity environment. It provides all necessary components for designing a CS table that can scan, compute, share and project urban related simulations. This project is designed to so that each of the core components could run separately, without others dependency.    

# Setup

## Unity

Download [Unity](https://unity3d.com/), clone, run
Branches: master for Template, heatmap for Andorra

## Displays & Environment

### Displays
Scene, Display 1 (UI) & Display 2 (3D visualization) should all be active; Display 1 will be the most important for the initial runthrough but might be inactive when play is pressed.

### Environment

Check that all textures are there (e.g. the image when there’s no webcam) and that there are no errors on running script. RenderTextures might cause issues here:

**Webcam texture:** The texture is passed from the keystoned quad holding the live webcam stream to another quad that is used for the scanning via a RenderTexture that the Camera looking at the keystoned quad holds.
**Projection-mapped texture: **This texture is coming from a RenderTexture assigned by a Camera looking at the 3D grid.

# Running the Template with Static Image

The first run-through with the image already provided is just to help orient the users with the help of the UI in Display 1:

## Views
We can start with the Camera view showing the image of the grid, then the Scanners view showing the scanner objects with their color assignment, then the Projection view with the output (table) image.
![](https://github.com/RELNO/CSharpScope_TemplateProject/blob/master/docs/Scanning_01.png?raw=true)
## Keystoning
![](https://github.com/RELNO/CSharpScope_TemplateProject/blob/master/docs/KeystoneUI.png?raw=true)
Landing on the Projection view, we can toggle the keystone mode to calibrate the projection quad. We can then test saving & loading the configuration we have, as well as switch to the camera view & keystone that object.

## Scanners 

We can also click on Calibrate Scanners here to modify their size/ placement etc.
![](https://github.com/RELNO/CSharpScope_TemplateProject/blob/master/docs/CameraKeystone_01.png?raw=true)
![](https://github.com/RELNO/CSharpScope_TemplateProject/blob/master/docs/CameraKeystone_02.png?raw=true)

## Using webcam input

Under Webcam Settings, we can choose Use Webcam, which will display extra settings for frame rate and pausing the camera. We can toggle between views in this mode, seeing what happens to scanners, 3D object, etc. 

## Color calibration

We can then click on Calibrate Colors under Color Settings--this is the part where we might need to exit the UI view (Display 1) and start looking at the object in the Scene. To begin, though, we can look at the Scanners or Colors views & see how the colors are sorted.
![](https://github.com/RELNO/CSharpScope_TemplateProject/blob/master/docs/Scanning_02.png?raw=true)

# Color Calibration & Objects in the Scene
![](https://github.com/RELNO/CSharpScope_TemplateProject/blob/master/docs/Color3d_lines.png?raw=true)
![](https://github.com/RELNO/CSharpScope_TemplateProject/blob/master/docs/Color3D_03.png?raw=true)
![](https://github.com/RELNO/CSharpScope_TemplateProject/blob/master/docs/Color3d_01.png?raw=true)

## Finding objects in the Scene

After seeing all the components, we can probably look into where the objects are in the scene & how the functionalities are implemented. The Template project has the following sub-categories:

GridDecoder holds all the scanner objects in two sub-groups: (1) the keystoned surface and (2) the scanners that read this texture. Color calibration, UI items (e.g. sliders, dock), and any grid calibration happens here. To begin calibration, we can click on the ScannersParent object inside the decoder, and then on 3D color space to see the 3D color plot.

[cityIO](cityio.media.mit.edu) holds the objects that interpret the scanned data & create the 3D grid. Controls data source (scanners vs via server) & data sending.
PrjMapping holds a camera and the keystoned surface for the projection-mapped table image, similar to the scanned webcam surface.
UICanvas holds all the UI in Display 1.

Helpers holds extra items such as a scene refresh that is currently unused but could be helpful for auto-restarting the app daily; a view manager that is controlled by the UI; and an event manager that helps the different parts communicate.

We can click on each of these to see what kinds of objects they have.

## Colors in 3D

Then, we can look at the 3D color space object & start calibrating colors by moving the spheres--this might be a little complicated at first, but we can try.

# Example: Andorra


Setup: checking out the heatmap branch & making sure nothing’s missing.
We can check how cityIO can work without the other components & how data sending works.

# Implementation Details


More in-depth look at scanning, adding UI elements (physical UI), maybe cityIO, and discussing what still needs an update (e.g. heatmaps).

# Questions & Suggestions

