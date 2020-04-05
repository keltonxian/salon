using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.IO;
using UnityEditor.iOS.Xcode;

[InitializeOnLoad]
public class BuildPostProcess : IActiveBuildTargetChanged, IPostprocessBuildWithReport
{

	public int callbackOrder { get { return 0; } }

	public void OnActiveBuildTargetChanged(BuildTarget previousTarget, BuildTarget newTarget)
	{
		Debug.Log("BuildPostProcess.OnActiveBuildTargetChanged Switched build target to " + newTarget);
        if (newTarget == BuildTarget.Android)
        {
            EditorUserBuildSettings.exportAsGoogleAndroidProject = true;
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        }
    }

	public void OnPostprocessBuild(BuildReport report)
	{
        BuildTarget target = report.summary.platform;
        string pathToBuiltProject = report.summary.outputPath;
        Debug.Log("BuildPostProcess.OnPostprocessBuild for target " + target + " at path " + pathToBuiltProject);
        if (target == BuildTarget.Android)
        {
            string buildOutPutPath = pathToBuiltProject + "/" + Application.productName + "/assets";
            if (!Directory.Exists(buildOutPutPath))
            {
                buildOutPutPath = pathToBuiltProject + "/" + Application.productName + "/src/main/assets";
            }
            string androidStudioProjectPath = Application.dataPath + "/../proj.android-studio/app/src/main/assets";
            if (Directory.Exists(buildOutPutPath) && Directory.Exists(androidStudioProjectPath))
            {
                DeleteFolder(androidStudioProjectPath);
                Directory.Move(buildOutPutPath, androidStudioProjectPath);
                Debug.Log("BuildPostProcess>>>>>>>>>>Replaced 'assets'");
            }
#if ZIP || zip
			string exportsFolder = Application.dataPath.Substring(0,Application.dataPath.Length-7)+"/Exports/"+Packager.GetFolderByBuildTarget(EditorUserBuildSettings.activeBuildTarget);
			if(Directory.Exists(exportsFolder)){

				if(Directory.Exists(androidStudioProjectPath+"/lua")) Directory.Delete(androidStudioProjectPath+"/lua",true);
				if(Directory.Exists(androidStudioProjectPath+"/"+LuaFramework.AppConst.appName)) Directory.Delete(androidStudioProjectPath+"/"+LuaFramework.AppConst.appName,true);
				if(Directory.Exists(androidStudioProjectPath+"/"+LuaFramework.AppConst.appName+"raw")) Directory.Delete(androidStudioProjectPath+"/"+LuaFramework.AppConst.appName+"raw",true);

				if(File.Exists(androidStudioProjectPath+"/lua.zip")) File.Delete(androidStudioProjectPath+"/lua.zip");
				if(File.Exists(androidStudioProjectPath+"/"+LuaFramework.AppConst.appName+".zip")) File.Delete(androidStudioProjectPath+"/"+LuaFramework.AppConst.appName+".zip");
				if(File.Exists(androidStudioProjectPath+"/"+LuaFramework.AppConst.appName+"raw.zip")) File.Delete(androidStudioProjectPath+"/"+LuaFramework.AppConst.appName+"raw.zip");
				if(File.Exists(androidStudioProjectPath+"/"+LuaFramework.AppConst.appName+".txt")) File.Delete(androidStudioProjectPath+"/"+LuaFramework.AppConst.appName+".txt");

				var dirs1 = new string[]{exportsFolder+"/lua"};
				var dirs2 = new string[]{exportsFolder+"/"+LuaFramework.AppConst.appName};
				var dirs3 = new string[]{exportsFolder+"/"+LuaFramework.AppConst.appName+"raw"};
				ZipUtility.Zip(dirs1,androidStudioProjectPath+"/lua.zip");
				ZipUtility.Zip(dirs2,androidStudioProjectPath+"/"+LuaFramework.AppConst.appName+".zip");
				ZipUtility.Zip(dirs3,androidStudioProjectPath+"/"+LuaFramework.AppConst.appName+"raw.zip");

				if(File.Exists(exportsFolder+"/"+LuaFramework.AppConst.appName+".txt")){
					File.Copy(exportsFolder+"/"+LuaFramework.AppConst.appName+".txt",androidStudioProjectPath+"/"+LuaFramework.AppConst.appName+".txt");
				}

				Debug.Log ("BuildPostProcess>>>>>>>>>>Create Zip");
			}
#endif
            string buildOutArmeabiPath = pathToBuiltProject + "/" + Application.productName + "/libs/armeabi-v7a";
            if (!Directory.Exists(buildOutArmeabiPath))
            {
                buildOutArmeabiPath = pathToBuiltProject + "/" + Application.productName + "/src/main/jniLibs/armeabi-v7a";
            }
            string androidStuidoArmeabiPath = Application.dataPath + "/../proj.android-studio/app/src/main/jniLibs/armeabi-v7a";
            if (Directory.Exists(buildOutArmeabiPath) && Directory.Exists(androidStuidoArmeabiPath))
            {
                DeleteFolder(androidStuidoArmeabiPath);
                Directory.Move(buildOutArmeabiPath, androidStuidoArmeabiPath);
                Debug.Log("BuildPostProcess>>>>>>>>>>Replaced 'armeabi-v7a'");
            }

            string buildOutX86Path = pathToBuiltProject + "/" + Application.productName + "/libs/x86";
            if (!Directory.Exists(buildOutX86Path))
            {
                buildOutX86Path = pathToBuiltProject + "/" + Application.productName + "/src/main/jniLibs/x86";
            }
            string androidStuidoX86Path = Application.dataPath + "/../proj.android-studio/app/src/main/jniLibs/x86";
            if (Directory.Exists(buildOutX86Path) && Directory.Exists(androidStuidoX86Path))
            {
                DeleteFolder(androidStuidoX86Path);
                Directory.Move(buildOutX86Path, androidStuidoX86Path);
                Debug.Log("BuildPostProcess>>>>>>>>>>Replaced 'x86'");
            }

            string buildOutJarPath = pathToBuiltProject + "/" + Application.productName + "/libs/unity-classes.jar";
            string androidStuidoJarPath = Application.dataPath + "/../proj.android-studio/app/libs/unity-classes.jar";
            if (File.Exists(buildOutJarPath) && File.Exists(androidStuidoJarPath))
            {
                File.Delete(androidStuidoJarPath);
                File.Move(buildOutJarPath, androidStuidoJarPath);
                Debug.Log("BuildPostProcess>>>>>>>>>>Replaced 'unity-classes.jar'");
            }


            //			string buildOutNugetPackage = pathToBuiltProject+"/"+Application.productName+"/libs/armeabi-v7a";
            //			if(!Directory.Exists(buildOutNugetPackage)){
            //				buildOutNugetPackage = pathToBuiltProject+"/"+Application.productName+"/src/main/jniLibs/armeabi-v7a";
            //			}
            //			string androidStuidoNugetPackagePath = Application.dataPath + "/../proj.android-studio/app/src/main/jniLibs/armeabi-v7a";
            //			if(Directory.Exists(buildOutNugetPackage) && Directory.Exists(androidStuidoNugetPackagePath)){
            //				DeleteFolder (androidStuidoNugetPackagePath);
            //				Directory.Move (buildOutNugetPackage, androidStuidoNugetPackagePath);
            //				Debug.Log ("BuildPostProcess>>>>>>>>>>Replaced 'package'");
            //			}

            //			string buildOutPutPathraw = Application.dataPath + "/../Exports/Android";
            //			if( Directory.Exists(buildOutPutPathraw) && Directory.Exists(androidStudioProjectPath)){
            //				CopyDirectory(buildOutPutPathraw,androidStudioProjectPath,true);
            //				Debug.Log ("BuildPostProcess>>>>>>>>>>Replaced 'Exports/Android'");
            //			}
        }
        else if (target == BuildTarget.iOS)
        {
            UpdateProject(target, pathToBuiltProject + "/Unity-iPhone.xcodeproj/project.pbxproj");

            string dir1 = pathToBuiltProject.Substring(0, pathToBuiltProject.LastIndexOf('/'));
            string dir2 = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/'));
            bool isSameDir = dir1.Equals(dir2);

            string buildOutPutPath1 = pathToBuiltProject + "/Data";
            string iOSProjectPath1 = Application.dataPath + "/../proj.ios/Data";
            if (!isSameDir && Directory.Exists(buildOutPutPath1))
            {
                if (Directory.Exists(iOSProjectPath1))
                {
                    DeleteFolder(iOSProjectPath1);
                }
                Directory.Move(buildOutPutPath1, iOSProjectPath1);
                Debug.Log("BuildPostProcess>>>>>>>>>>Replaced 'Data'");
            }
            string buildOutPutPath2 = pathToBuiltProject + "/Classes/Native";
            string iOSProjectPath2 = Application.dataPath + "/../proj.ios/Classes/Native";
            if (!isSameDir && Directory.Exists(buildOutPutPath2))
            {
                if (Directory.Exists(iOSProjectPath2))
                {
                    DeleteFolder(iOSProjectPath2);
                }
                Directory.Move(buildOutPutPath2, iOSProjectPath2);
                Debug.Log("BuildPostProcess>>>>>>>>>>Replaced 'Native'");
            }

            // e.g. in Assets, create a Plugins/iOS folder, put .h .mm inside
            string buildOutPutPath3 = pathToBuiltProject + "/Libraries/Framework";
            string iOSProjectPath3 = Application.dataPath + "/../proj.ios/Libraries/Framework";
            if (!isSameDir && Directory.Exists(buildOutPutPath3))
            {
                if (Directory.Exists(iOSProjectPath3))
                {
                    DeleteFolder(iOSProjectPath3);
                }
                Directory.Move(buildOutPutPath3, iOSProjectPath3);
                Debug.Log("BuildPostProcess>>>>>>>>>>Replaced 'Framework'");
            }

            string buildOutPutPath4 = pathToBuiltProject + "/Libraries/libil2cpp";
            string iOSProjectPath4 = Application.dataPath + "/../proj.ios/Libraries/libil2cpp";
            if (!isSameDir && Directory.Exists(buildOutPutPath4))
            {
                if (Directory.Exists(iOSProjectPath4))
                {
                    DeleteFolder(iOSProjectPath4);
                }
                Directory.Move(buildOutPutPath4, iOSProjectPath4);
                Debug.Log("BuildPostProcess>>>>>>>>>>Replaced 'libil2cpp'");
            }
        }
        Debug.Log("BuildPostProcess>>>>>>>>>>=========================build complete===============================");
    }

    private void DeleteFolder(string dir)
    {
        DirectoryInfo dirs = new DirectoryInfo(dir);
        DeleteFolder(dirs);
    }

    private void DeleteFolder(DirectoryInfo dirs)
    {
        if (dirs == null || (!dirs.Exists))
        {
            return;
        }

        DirectoryInfo[] subDir = dirs.GetDirectories();
        if (subDir != null)
        {
            for (int i = 0; i < subDir.Length; i++)
            {
                if (subDir[i] != null)
                {
                    DeleteFolder(subDir[i]);
                }
            }
            subDir = null;
        }

        FileInfo[] files = dirs.GetFiles();
        if (files != null)
        {
            for (int i = 0; i < files.Length; i++)
            {
                if (files[i] != null)
                {
                    files[i].Delete();
                    files[i] = null;
                }
            }
            files = null;
        }

        dirs.Delete();
    }

    private void CopyDirectory(string sourceDirName, string destDirName, bool copySubDirs)
    {
        // Get the subdirectories for the specified directory.
        DirectoryInfo dir = new DirectoryInfo(sourceDirName);

        if (!dir.Exists)
        {
            return;
        }

        DirectoryInfo[] dirs = dir.GetDirectories();
        // If the destination directory doesn't exist, create it.
        if (!Directory.Exists(destDirName))
        {
            Directory.CreateDirectory(destDirName);
        }

        // Get the files in the directory and copy them to the new location.
        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files)
        {
            string temppath = Path.Combine(destDirName, file.Name);
            file.CopyTo(temppath, true);
        }

        // If copying subdirectories, copy them and their contents to new location.
        if (copySubDirs)
        {
            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(destDirName, subdir.Name);
                CopyDirectory(subdir.FullName, temppath, copySubDirs);
            }
        }
    }

    private void UpdateProject(BuildTarget buildTarget, string projectPath)
    {
        PBXProject project = new PBXProject();
        project.ReadFromString(File.ReadAllText(projectPath));

        string targetId = project.GetUnityMainTargetGuid();

        // Required Frameworks
        project.AddFrameworkToProject(targetId, "CoreTelephony.framework", false);
        project.AddFrameworkToProject(targetId, "EventKit.framework", false);
        project.AddFrameworkToProject(targetId, "EventKitUI.framework", false);
        project.AddFrameworkToProject(targetId, "MediaPlayer.framework", false);
        project.AddFrameworkToProject(targetId, "MessageUI.framework", false);
        project.AddFrameworkToProject(targetId, "QuartzCore.framework", false);
        project.AddFrameworkToProject(targetId, "SystemConfiguration.framework", false);
        project.AddFrameworkToProject(targetId, "Security.framework", false);
        project.AddFrameworkToProject(targetId, "MobileCoreServices.framework", false);
        project.AddFrameworkToProject(targetId, "PassKit.framework", false);
        project.AddFrameworkToProject(targetId, "Social.framework", false);
        project.AddFrameworkToProject(targetId, "CoreData.framework", false);
        //project.AddFrameworkToProject(targetId, "AdSupport.framework", false);
        //project.AddFrameworkToProject(targetId, "StoreKit.framework", false);

        //if (project.ContainsFramework(targetId, "iAd.framework"))
        //{
        //    project.RemoveFrameworkFromProject(targetId, "iAd.framework");
        //}

        project.AddFileToBuild(targetId, project.AddFile("usr/lib/libz.1.2.5.dylib", "Frameworks/libz.1.2.5.dylib", PBXSourceTree.Sdk));
        project.AddFileToBuild(targetId, project.AddFile("usr/lib/libz.dylib", "Frameworks/libz.dylib", PBXSourceTree.Sdk));
        project.AddFileToBuild(targetId, project.AddFile("usr/lib/libsqlite3.dylib", "Frameworks/libsqlite3.dylib", PBXSourceTree.Sdk));
        project.AddFileToBuild(targetId, project.AddFile("usr/lib/libsqlite3.0.dylib", "Frameworks/libsqlite3.0.dylib", PBXSourceTree.Sdk));
        project.AddFileToBuild(targetId, project.AddFile("usr/lib/libxml2.dylib", "Frameworks/libxml2.dylib", PBXSourceTree.Sdk));

        // Optional Frameworks
        project.AddFrameworkToProject(targetId, "Webkit.framework", true);
        project.AddFrameworkToProject(targetId, "JavaScriptCore.framework", true);
        project.AddFrameworkToProject(targetId, "WatchConnectivity.framework", true);

        // For 3.0 MP classes
        project.AddBuildProperty(targetId, "OTHER_LDFLAGS", "-ObjC");
        project.AddBuildProperty(targetId, "OTHER_LDFLAGS", "-lxml2");
        project.SetBuildProperty(targetId, "ENABLE_BITCODE", "NO");

        File.WriteAllText(projectPath, project.WriteToString());
    }
}