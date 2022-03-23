/*!
 * @authors Ben Knight (bknight@i3drobotics.com)
 * @date 2021-05-26
 * @copyright Copyright (c) I3D Robotics Ltd, 2021
 * 
 * @file I3DRDemoBuild
 * @brief Build pipeline for command line building of project
 */

using UnityEditor;
using I3DR.Phase;

namespace I3DR
{
    class PhaseAutoPackage
    {
        public static void ExportPackage()
        {
            // get package version from Phase and use it to name the package e.g. Phase_x.x.x.unitypackage
            string Phase_version = PhaseVersion.getVersionString();
            string exportPackagePath = "../../../install/unity/package/phase_v"+Phase_version+".unitypackage";
            ExportPackageOptions exportFlags = ExportPackageOptions.IncludeDependencies | ExportPackageOptions.Recurse;
            AssetDatabase.ExportPackage("Assets/Phase", exportPackagePath, exportFlags);
        }
    }
}