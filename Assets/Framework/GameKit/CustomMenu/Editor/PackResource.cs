using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class PackResource
{
    [MenuItem("PackResource/Build Asset Resource", false, 101)]
    static void BuildAssetResource()
    {

        string streamPath = Application.streamingAssetsPath + "/" + EditorSettings.projectGenerationRootNamespace.ToLower();
        if (Directory.Exists(streamPath))
        {
            var files = Directory.GetFiles(streamPath, "*.*", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < files.Length; ++i)
            {
                if (File.Exists(files[i]))
                {
                    File.Delete(files[i]);
                }
            }
        }
        else
        {
            Directory.CreateDirectory(streamPath);
        }

        AssetDatabase.Refresh();
        BuildPipeline.BuildAssetBundles(streamPath, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
        AssetDatabase.Refresh();
    }

    public static void CopyDirectory(string sourceDirName, string destDirName, bool copySubDirs)
    {
        DirectoryInfo dir = new DirectoryInfo(sourceDirName);

        if (!dir.Exists)
        {
            return;
        }

        DirectoryInfo[] dirs = dir.GetDirectories();
        if (!Directory.Exists(destDirName))
        {
            Directory.CreateDirectory(destDirName);
        }

        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files)
        {
            if (file.Name.EndsWith(".meta", System.StringComparison.Ordinal)) continue;

            if (file.FullName.EndsWith(".manifest", System.StringComparison.Ordinal))
            {
                string folder = Directory.GetParent(file.FullName).Name;
                if (!file.FullName.EndsWith(folder + ".manifest", System.StringComparison.Ordinal))
                {
                    continue;
                }
            }

            string temppath = Path.Combine(destDirName, file.Name);
            file.CopyTo(temppath, true);
        }

        if (copySubDirs)
        {
            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(destDirName, subdir.Name);
                CopyDirectory(subdir.FullName, temppath, copySubDirs);
            }
        }
    }
}
