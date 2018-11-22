# Visualization for MIT Volpe project

[unity, c#, json]

---

![](DOCS/a.gif)

![](DOCS/b.gif)

---

-- Note: Demo is active but dev. on this repo seized --

This is a 3d visualization of CityScope Volpe [MIT East Campus] development. This part displays different analyses/heatmaps, including agents visualization of Proximity-based Amenities Usage in Kendall Sq. Cambridge.

This project uses Unity3d, a JSON parser and visualizer for cityIO server.

#### Running

This repo will only work in concert with cityIO API Ver.1.

- get Unity3d [v2017 onwards]
- clone repo
- Setup cityIO specs under `CityIO` component in the Unity editor
  - Make sure cityIO endpoints are following `cityIO` api specifications
  - rename server endpoint to yours
  - look for GET/POST console response
- Click `play`
