[//]: <> (README.md)

# UntoldByte - GAINS  [![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/H2H4RPRCN)

GAINS - Generative AI Novelty Software is a collection of (currently only two :) ) Generative AI Tools that allow you to generate Icons, Sprites and Textures by controling by example what you want to generate.  Don't just generate some art but exactly how you want it. Texture untextured (but UV unwrapped) 3D objects by projecting textures.

[![Preview in action](https://external-preview.redd.it/texturing-with-untoldbyte-gains-in-unity-v0-N2MwdXBhd2VyYTRjMY22CQHxFZPvL2BhMgOw8SukWeWRIV2qYOjSMd442Jfj.png?width=640&crop=smart&format=pjpg&auto=webp&s=0b39db773c25e2a50883227a9ab175b269a0761b "Click the image to see UntoldByte - GAINS Entity Painter in action (reddit post with video)")](https://www.reddit.com/r/StableDiffusion/comments/18amoq6/texturing_with_untoldbyte_gains_in_unity/)

*Click the image above to see UntoldByte - GAINS Entity Painter in action (reddit post with video)*

---
**Render pipeline compatibility**

|Unity Version|Built-In|URP|HDRP|
|-|-|-|-|
|2019.4.39f1|:white_check_mark: Compatible|:white_check_mark: Compatible|:white_check_mark: Compatible|
|2020.3.42f1|:white_check_mark: Compatible|:white_check_mark: Compatible|:white_check_mark: Compatible|
|2021.3.27f1|:white_check_mark: Compatible|:white_check_mark: Compatible|:white_check_mark: Compatible|
|2022.3.3f1|:white_check_mark: Compatible|:white_check_mark: Compatible|:white_check_mark: Compatible|
|2023.1.1f1|:white_check_mark: Compatible|:white_check_mark: Compatible|:white_check_mark: Compatible|
---
**_Generate_**  everything  **_locally_**  - by accessing Stable Diffusion Web UI through api, without a fear of future availability or support - own the process.


Currently  **_two tools_**  are available:

-   **_Symbol Creator_**  is a tool that enables you to create images and icons by controlling the Stable Diffusion output with your sketches, by controlling with in Unity Editor captured color and depth images or by letting AI decide how the image should look.
-   **_Entity Painter_**  is a tool that allows you to leverage controled image generation to generate textures for 3D objects by projecting, baking and then exporting texture (currently only diffuse textures).

  

**_Additional_**  AI  **_features_**  include  **_upscaling_** and  **_removing background_**  from images (cutting out the main object and making the rest of image transparent).

  
---
**Dependencies**:

 + SharpZipLib (com.unity.sharp-zip-lib:1.3.4-preview),
 + Newtonsoft Json (com.unity.nuget.newtonsoft-json:3.2.1), 
 + Stable Diffusion Web UI (v1.6.0 a bit earlier versions may work too) with:

	- ControlNet (tested with v1.1.410 a bit earlier versions may work too),
	- Rembg.
---

**Requirements**:

- Access to working Stable Diffusion web UI instance with ControlNet with suitable PC/Graphics card (see  [Stable Diffusion web UI installation](https://github.com/AUTOMATIC1111/stable-diffusion-webui#installation-and-running))

---

_* Graphics APIs OpenGLES3, Metal, Switch, XboxOne, XboxOne D3D12, GameCoreXboxOne, GameCoreXboxSeries, PlayStation 4, PlayStation 5 may work but have not been tested_

_* Part of this asset (Entity Painter) does not work correctly when project is set up to use graphics API that does not support texture arrays (e.g., OpenGLES2)_

_* This asset may work but has not been tested in play mode_

_* This asset may work on Linux (Ubuntu) but has not been extensively tested_

_* This asset may work on MacOS but has not been tested_

---
**Tested with:**

-   Unity 2019.4, 2020.3, 2021.3, 2022.3, 2023.1
-   Built-In, URP, HDRP
-   DirectX, Vulkan, OpenGL
-   Forward, Forward+, Deffered
-   Mono, IL2CPP
-   .net standard, .net framework
-   Windows
