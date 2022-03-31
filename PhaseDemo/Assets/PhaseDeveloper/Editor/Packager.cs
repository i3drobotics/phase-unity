/*!
 * @authors Ben Knight (bknight@i3drobotics.com)
 * @date 2021-05-26
 * @copyright Copyright (c) I3D Robotics Ltd, 2021
 * 
 * @file I3DRDemoBuild
 * @brief Build pipeline for command line building of project
 */

using UnityEditor;
using UnityEngine;
using System.IO;

namespace I3DR.PhaseUnity
{
    class Packager
    {
        [MenuItem("PhaseUnity/Create Package")]
        public static void ExportPackage()
        {
            // get package version from Phase and use it to name the package e.g. phaseunity_v.x.x.x.unitypackage
            string phaseUnity_version = Application.version;
            string exportPackageFolder = "../deployment/package";
            Directory.CreateDirectory(exportPackageFolder);
            string exportPackagePath = exportPackageFolder + "/phaseunity_v"+ phaseUnity_version + ".unitypackage";
            ExportPackageOptions exportFlags = ExportPackageOptions.IncludeDependencies | ExportPackageOptions.Recurse;
            AssetDatabase.ExportPackage("Assets/Phase", exportPackagePath, exportFlags);
        }
    }
}