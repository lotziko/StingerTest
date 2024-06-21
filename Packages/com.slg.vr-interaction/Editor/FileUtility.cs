using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UnityEditor.VRInteraction
{
    // Based on ProBuilder File Utility
    public static class FileUtility
    {
        static string _vrInteractionFolderPath = "Packages/com.slg.vr-interaction/";
        static string _vrInteractionDataPath = "Assets/VRInteraction Data/";

        // The order is important - always search for the package manager installed version first
        static readonly string[] _possibleInstallDirectories = new string[]
        {
            "Packages/com.slg.vr-interaction/",
            "UnityPackageManager/com.slg.vr-interaction/",
            "Assets/",
        };

        internal static string GetVRInteractionInstallDirectory()
        {
            if (ValidateVRInteractionRoot(_vrInteractionFolderPath))
                return _vrInteractionFolderPath;

            foreach (var install in _possibleInstallDirectories)
            {
                _vrInteractionFolderPath = install;

                if (ValidateVRInteractionRoot(_vrInteractionFolderPath))
                    return _vrInteractionFolderPath;
            }

            // It's not in any of the usual haunts, start digging through Assets until we find it (likely an A$ install)
            _vrInteractionFolderPath = FindAssetStoreVRInteractionInstall();

            if (Directory.Exists(_vrInteractionFolderPath))
                return _vrInteractionFolderPath;

            // Things are dire. VRInteraction was nowhere to be found in the Assets directory, which means either the user
            // has renamed the folder, or something very spooky is going on.
            // Either way, just create a new VRInteraction folder in Assets and return that so at the very least
            // local preferences will still work.
            Debug.LogWarning("Creating a new VRInteraction directory... was the VRInteraction folder renamed?\nIcons & preferences may not work in this state.");
            _vrInteractionFolderPath = "Assets/VRInteraction";
            Directory.CreateDirectory(_vrInteractionFolderPath);

            return _vrInteractionFolderPath;
        }

        internal static string FindAssetStoreVRInteractionInstall()
        {
            string dir = null;

            string[] matches = Directory.GetDirectories("Assets", "VRInteraction", SearchOption.AllDirectories);

            foreach (var match in matches)
            {
                dir = match.Replace("\\", "/") + "/";
                if (dir.Contains("VRInteraction") && ValidateVRInteractionRoot(dir))
                    break;
            }

            return dir;
        }

        internal static bool ValidateVRInteractionRoot(string dir)
        {
            return !string.IsNullOrEmpty(dir) &&
                Directory.Exists(dir + "/Editor") &&
                Directory.Exists(dir + "/Runtime");
        }

        internal static string GetLocalDataDirectory(bool initializeIfMissing = false)
        {
            if (Directory.Exists(_vrInteractionDataPath))
                return _vrInteractionDataPath;

            string root = GetVRInteractionInstallDirectory();

            if (root.StartsWith("Assets"))
            {
                // Installed from Asset Store or manual package import
                _vrInteractionDataPath = root + "Data/";
            }
            else
            {
                // Scan project for VRInteraction Data folder
                // none found? create one at root
                string[] matches = Directory.GetDirectories("Assets", "VRInteraction Data", SearchOption.AllDirectories);
                _vrInteractionDataPath = matches.Length > 0 ? matches[0] : "Assets/VRInteraction Data/";
            }

            if (!Directory.Exists(_vrInteractionDataPath) && initializeIfMissing)
                Directory.CreateDirectory(_vrInteractionDataPath);

            return _vrInteractionDataPath;
        }

        internal static T FindAssetOfType<T>() where T : Object
        {
            foreach (var i in AssetDatabase.FindAssets("t:" + typeof(T).ToString()))
            {
                T o = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(i));
                if (o != null)
                    return o;
            }

            return null;
        }

        internal static T LoadRequired<T>(string path) where T : ScriptableObject
        {
            T asset = Load<T>(path);

            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();

                UnityEditor.EditorUtility.SetDirty(asset);

                string folder = Path.GetDirectoryName(path);

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                AssetDatabase.CreateAsset(asset, path);
            }

            return asset;
        }

        static T Load<T>(string path) where T : Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }
    }
}
