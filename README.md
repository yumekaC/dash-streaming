# How to use Point cloud streaming

## Requirement
Unity version: 2019.4.17f1 (you can use 2021 ver.)

code for rendering: https://github.com/keijiro/Pcx 

## Usage

### In server side
Create Apache server on sever machine.

Copy the published IP address (*) of the server. 

### In client side

In new Unity scene, create Resources folder in Assets folder and create new Material named MyDefault(.mat) in Resources folder.

Change to Shader: Point Cloud/Point in Inspector panel of MyDefault.mat.

Create Scripts folder in Assets folder. In Assets folder, create new two scripts named XXXX(.cs) and MyClass(.cs), and then replace code with the github code XXXX.cs and [MyClass.cs](https://github.com/yumekaC/dash-streaming/blob/main/source_code/Client/MyClass.cs). In XXXX.cs, change IP address to the copied IP address (*).

Create new GameObject in Hierarchy and add code of XXXX.cs by pushing Add Component button in Inspector panel.

Play the scene.

#### I. Rendering 1 point cloud without streaming
[Rendering1.cs](https://github.com/kanai1192/sensor_works/blob/main/chujo_code/HoloLens_code/Rendering1.cs)

#### II. Streaming 1 point cloud sequence with rendering
[Streaming1.cs](https://github.com/kanai1192/sensor_works/blob/main/chujo_code/HoloLens_code/Streaming1.cs)

#### III. Rendering Multi point cloud sequences or objects without streaming
[Demo6.cs](https://github.com/kanai1192/sensor_works/blob/main/chujo_code/HoloLens_code/Demo6.cs)

#### IV. Streaming Multi point cloud sequences or objects without rendering
[CompSegAdap.cs](https://github.com/kanai1192/sensor_works/blob/main/chujo_code/HoloLens_code/CompSegAdap.cs)
