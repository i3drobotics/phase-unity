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
using UnityEngine.Networking;
using System;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace I3DR.PhaseUnity
{
    class PluginUpdater
    {
        static string phaseVersion = "0.0.1";
        static UnityWebRequestAsyncOperation request;

        [MenuItem("PhaseUnity/Update Plugins")]
        public static void UpdatePlugins()
        {
            string url = "https://github.com/i3drobotics/phase-sharp/releases/download/v" + phaseVersion +
                    "/phase-sharp-v" + phaseVersion + "-windows-x86_64.zip";
            string zip_file = Application.dataPath + "/Phase/Plugins/x64/phase-sharp-v" + phaseVersion + "-windows-x86_64.zip";
            string out_folder = Application.dataPath + "/Phase/Plugins/x64";

            UnityWebRequest www = UnityWebRequest.Get(url);
            www.downloadHandler = new DownloadHandlerFile(zip_file);

            request = www.SendWebRequest();

            while (!request.isDone)
            {
                EditorUtility.DisplayProgressBar("Phase Plugins", "Downloading...", request.progress);
            }

            EditorUtility.ClearProgressBar();

            // TODO decompress zip file
        }
    }


}