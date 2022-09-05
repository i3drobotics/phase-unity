# Phase Unity
Unity package for using I3DR's Phase library in Unity. This uses the [Phase CSharp](https://github.com/i3drobotics/phase-csharp) wrapper library for using Phase in C#. This repository includes a full demo project that includes the unity package. This is also used to test and build the package.

**NOTE: This package is still in development with very little functionality. Updates coming soon.**

## Dependencies
### Unity
This project aims to be tested on the latest LTS version of Unity. At time of writing the Unity version used by this project is v2021.3.9f1.
### Phase CSharp
Phase CSharp library v0.1.0 is required. A Unity Editor menu item is provided to automically download and setup the required library. After launching the project in the Unity Editor, navigate the menu toolbar at the top and select `Phase Unity -> Download Plugins`.

## Build
A Unity Editor menu item is provided for building the Phase Unity demo. After launching the project in the Unity Editor, navigate to the menu toolbar at the top and select `Phase Unity -> Build Demo`.  
A Unity Editor menu item is also provied for creating a Phase Unity package. Navigate to the menu toolbar at the top and select `Phase Unity -> Create Package`.

## Test
Unit testing is provided for the package that can be run from the [Unity Test Framework](https://docs.unity3d.com/Packages/com.unity.test-framework@1.1/manual/index.html). Download this from the package manager to run tests.  
Run tests by selecting `Windows->General->Test Runner` from the Unity menu. Then run the provided Play and Edit tests.  
Alternatively a Unity Editor manu item is provided which can by selecting `Phase Unity -> Tests -> Run Play Mode Tests` and `Phase Unity -> Tests -> Run Edit Mode Tests`.
