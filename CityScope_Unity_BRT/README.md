# BRT Street Simulation in Boston (Barr Foundation+DUSP+CS)

BRT Street Scale simulation using Unity3d

---

- Clone, start with Unity
- Use 1-4 to select keystone croner for either camera calibration or projection mapping [toggle on/off within GameObject]

<a href="http://www.youtube.com/watch?feature=player_embedded&v=lFXMshEGBSk
" target="_blank"><img src="http://img.youtube.com/vi/lFXMshEGBSk/3.jpg" 
alt="YouTube" width="400" height="100%" border="10" /></a>

# BRT Table

Scanning and Keystoning: both take place inside their respective parent objects, `ScannerScheme` and `ProjectorsScheme`.

## Scanning

Scanning's most important functionalities are in `Scanners.cs`, attached to the `CameraKeystoned` GameObject.

The grid's dimensions are defined by the two public variables

```
public int _numOfScannersX;
public int _numOfScannersY;
```

The Scanner object receives the modified texture from a keystoned quad `GameObject` in `assignRenderTexture ();`. This keystoned quad has a `RenderTexture` assigned to it via another, orthogonal camera looking at the camera's stream, the `KeystoneCamera` `GameObject`. This camera has its Target Texture set to the keystoned quad `GameObject`.

In order for scanning to work, the `KeystoneCamera` has to look down at the webcam's stream, with the scanning grid's parent object vertically above the camera and the keystoned quad.

The grid is attached to the `ScanGridParent GameObject`, and it gets population upon play.

## Keystoning

Keystoning takes place in two separate instances of `keyStone.cs`, one attached to `ScannerScheme` and another one to `ProjectorsScheme`.

#### Scanners

`keyStone.cs` essentially remaps the texture received from the webcam to the `CameraKeystoned` object's quad. This remapping also happens in the shader attached to the material associated with this `GameObject` called `KeystonedTexture`. Due to the shader's affine fix, the texture has to be grabbed from the screen, hence the extra pass through the `RenderTexture`.

#### Projectors

Similarly, `keyStone.cs` is included in the two projector `GameObjects` under `ProjectorsScheme`. `AffineFixQuadMonitor` 1 and 2 include a `keyStone.cs` copy called `AffineUVFix.cs`.
