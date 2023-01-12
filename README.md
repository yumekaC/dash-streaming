# How to use Point cloud streaming

## Requirement

### In server side

code for G-PCC: https://github.com/MPEGGroup/mpeg-pcc-tmc13

code for PSNR of point cloud objects: https://github.com/mauriceqch/geo_dist

### In client side
Unity version: 2019.4.17f1 (you can use 2021 ver.)

code for rendering: https://github.com/keijiro/Pcx 

## Usage

### In server side

Create Apache server on sever machine.

Copy the published IP address (*) of the server.

Make dash representation: Details

### In client side

In new Unity scene, create Resources folder in Assets folder and create new Material named MyDefault(.mat) in Resources folder.

Change to Shader: Point Cloud/Point in Inspector panel of MyDefault.mat.

Create Scripts folder in Assets folder. In Assets folder, create new two scripts named XXXX(.cs) and MyClass(.cs), and then replace code with the github code XXXX.cs and [MyClass.cs](https://github.com/yumekaC/dash-streaming/blob/main/source_code/Client/MyClass.cs). In XXXX.cs, change IP address to the copied IP address (*).

Create new GameObject in Hierarchy and add code of XXXX.cs by pushing Add Component button in Inspector panel.

Play the scene.

#### I. Rendering 1 point cloud without streaming
[Rendering1.cs](https://github.com/yumekaC/dash-streaming/blob/main/source_code/Client/Rendering1.cs)

#### II. Streaming Multi point cloud sequences or objects without rendering
[CompSegAdap.cs](https://github.com/yumekaC/dash-streaming/blob/main/source_code/Client/CompSegAdap.cs)

#### III. Emulating Multi point cloud sequences or objects without rendering using quality-driven recipe and control
[Emulating.cs](https://github.com/yumekaC/dash-streaming/blob/main/source_code/Client/Emulation.cs)
and [ChangeDistance.cs](https://github.com/yumekaC/dash-streaming/blob/main/source_code/Client/ChangeDistance.cs)

Moreover, store [distance.csv](https://github.com/yumekaC/dash-streaming/blob/main/source_code/Client/distance.csv) in Assets/StreamingAssets/
