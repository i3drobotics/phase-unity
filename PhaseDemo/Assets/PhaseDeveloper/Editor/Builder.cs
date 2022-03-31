/*!
 * @authors Ben Knight (bknight@i3drobotics.com)
 * @date 2021-05-26
 * @copyright Copyright (c) I3D Robotics Ltd, 2021
 * 
 * @file I3DRDemoBuild
 * @brief Build pipeline for command line building of project
 */

using UnityEngine;
using UnityEditor;

namespace I3DR.PhaseUnity
{
    class Builder
    {
        [MenuItem("PhaseUnity/Build Demo")]
        public static void Build()
        {
            var report = BuildPipeline.BuildPlayer(
                new[] { "Assets/Phase/Scenes/PhaseUnityDemoScene.unity" },
                "../deployment/PhaseDemo/PhaseUnityDemo.exe",
                BuildTarget.StandaloneWindows64,
                BuildOptions.None);
            Debug.Log(report);
        }
    }
}