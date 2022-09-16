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
using System.IO;
using Ionic.Zip;

namespace I3DR.PhaseUnity
{
    class PluginDownloader
    {
        static string phaseVersion = "0.1.1-1";
        static UnityWebRequestAsyncOperation request;

        [MenuItem("PhaseUnity/Download Plugins")]
        public static void DownloadPlugins()
        {
            string zip_file = "phase-csharp-v" + phaseVersion + "-windows-x86_64.zip";
            string url = "https://github.com/i3drobotics/phase-csharp/releases/download/v" + phaseVersion + "/" + zip_file;
            string plugin_folder = Application.dataPath + "/Phase/Plugins/x64";
            string zip_filepath = plugin_folder + "/" + zip_file;
            string zip_out_folder = plugin_folder + "/phase-csharp-zip";

            UnityWebRequest www = UnityWebRequest.Get(url);
            www.downloadHandler = new DownloadHandlerFile(zip_filepath);

            request = www.SendWebRequest();

            while (!request.isDone)
            {
                EditorUtility.DisplayProgressBar("Phase Plugins", "Downloading...", request.progress);
            }

            EditorUtility.ClearProgressBar();

            if (www.responseCode != 200)
                return;

            // extract zip file
            using (ZipFile zip = ZipFile.Read(zip_filepath))
            {
                foreach (ZipEntry e in zip)
                {
                    EditorUtility.DisplayProgressBar("Phase Plugins", "Extracting " + zip_file + "...", 0.9f);
                    e.Extract(zip_out_folder);
                }
            }

            EditorUtility.ClearProgressBar();

            // clean folder
            foreach (string file in Directory.GetFiles(plugin_folder, "*", SearchOption.TopDirectoryOnly))
            {
                EditorUtility.DisplayProgressBar("Phase Plugins", "Updating files ...", 0.9f);
                File.Delete(file);      
            }

            // copy plugin files to plugin folder
            foreach (string file in Directory.GetFiles(zip_out_folder, "*", SearchOption.TopDirectoryOnly))
            {
                EditorUtility.DisplayProgressBar("Phase Plugins", "Updating files ...", 0.9f);
                File.Copy(file, plugin_folder + "/" + Path.GetFileName(file));
            }

            // delete zip folder
            Directory.Delete(zip_out_folder, true);
            // delete zip file
            File.Delete(zip_filepath);

            EditorUtility.ClearProgressBar();

            bool restart = EditorUtility.DisplayDialog("Phase Plugins", "Phase plugins updated. Unity editor must be restarted for plugins to be reloaded. Would you like to restart now?", "Restart", "Cancel");
            if (restart){
                EditorApplication.OpenProject(Directory.GetCurrentDirectory());
            }
        }
    }


}