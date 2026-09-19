#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

public class EnableIOSFileSharing
{
   
    public static void OnPostprocessBuild(BuildTarget buildTarget, string path)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            // 获取导出的 Xcode 项目中的 Info.plist 文件路径
            string plistPath = path + "/Info.plist";
            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile(plistPath);

            PlistElementDict rootDict = plist.root;

            // 1. 开启 iTunes 文件共享 (UIFileSharingEnabled)
            // 这将允许您的 App 出现在您截图中的 Mac Finder 的 Files 列表中
            rootDict.SetBoolean("UIFileSharingEnabled", true);

            // 2. 允许在原位打开文档 (LSSupportsOpeningDocumentsInPlace)
            // 这将允许您直接在 iPhone 自带的“文件”App 中看到您应用的文件夹
            rootDict.SetBoolean("LSSupportsOpeningDocumentsInPlace", true);

            // 保存修改
            File.WriteAllText(plistPath, plist.WriteToString());
        }
    }
}
#endif