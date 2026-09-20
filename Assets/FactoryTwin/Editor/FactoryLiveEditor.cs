using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Small, project-local command mailbox. All scene changes use Unity's Editor APIs.
[InitializeOnLoad]
public static class FactoryLiveEditor
{
    [Serializable] public class Command { public string action; public int stage; public string view; }
    static double nextPoll;
    static string Root => Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
    static string Mailbox => Path.Combine(Root, "Automation/command.json");
    static FactoryLiveEditor() { EditorApplication.update += Tick; }
    static void Tick()
    {
        if (EditorApplication.timeSinceStartup < nextPoll || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        nextPoll = EditorApplication.timeSinceStartup + 1;
        Directory.CreateDirectory(Path.Combine(Root, "Automation"));
        if (!File.Exists(Mailbox)) return;
        try
        {
            var c = JsonUtility.FromJson<Command>(File.ReadAllText(Mailbox));
            File.Delete(Mailbox);
            switch (c.action)
            {
                case "refresh": AssetDatabase.Refresh(); break;
                case "stage": FactorySceneBuilder.BuildStage(c.stage); break;
                case "play": EditorApplication.isPlaying = true; break;
                case "stop": EditorApplication.isPlaying = false; break;
                case "save": EditorSceneManager.SaveOpenScenes(); AssetDatabase.SaveAssets(); break;
                case "capture": Capture(c.view); break;
                case "frame": Frame(); break;
                case "game": EditorApplication.ExecuteMenuItem("Window/General/Game"); break;
                case "resolve-ui": UnityEditor.PackageManager.Client.Add("file:E:/UnityProjects/OdysseyNostos/Library/PackageCache/com.unity.ugui@67707a67a4ab"); break;
                case "install-mcp": UnityEditor.PackageManager.Client.Add("file:../LocalPackages/com.coplaydev.unity-mcp"); break;
                case "start-mcp":
                    EditorPrefs.SetBool("MCPForUnity.UseHttpTransport",false);
                    foreach(var a in AppDomain.CurrentDomain.GetAssemblies())
                    {var t=a.GetType("MCPForUnity.Editor.Services.Transport.Transports.StdioBridgeHost");if(t!=null){t.GetMethod("StartAutoConnect").Invoke(null,null);break;}}
                    break;
                case "quit": EditorSceneManager.SaveOpenScenes(); EditorApplication.Exit(0); break;
            }
            File.WriteAllText(Path.Combine(Root,"Automation/last-command.txt"), DateTime.Now.ToString("O") + " " + c.action + " " + c.stage);
        }
        catch(Exception e) { Debug.LogException(e); File.WriteAllText(Path.Combine(Root,"Automation/command-error.txt"), e.ToString()); }
    }
    public static void Frame()
    {
        var sv = SceneView.lastActiveSceneView ?? EditorWindow.GetWindow<SceneView>();
        sv.LookAt(new Vector3(0,1.25f,0.8f), Quaternion.Euler(26,-27,0), 12.2f, false, true);
        sv.sceneLighting = true; sv.Repaint();
    }
    public static void Capture(string view)
    {
        var cam = Camera.main;
        if (!cam) return;
        var pos=cam.transform.position; var rot=cam.transform.rotation;
        if(view=="detail") {cam.transform.position = new Vector3(-0.8f,3,-3.3f); cam.transform.LookAt(new Vector3(1,1.45f,0.75f));}
        if(view=="rear") {cam.transform.position = new Vector3(8,6,8); cam.transform.LookAt(new Vector3(0,1,0));}
        var rt=new RenderTexture(1920,1080,24); var old=cam.targetTexture; var prev=RenderTexture.active;
        cam.targetTexture=rt; cam.Render(); RenderTexture.active=rt;
        var tex=new Texture2D(1920,1080,TextureFormat.RGB24,false); tex.ReadPixels(new Rect(0,0,1920,1080),0,0); tex.Apply();
        Directory.CreateDirectory(Path.Combine(Root,"Evidence"));
        File.WriteAllBytes(Path.Combine(Root,"Evidence/"+(string.IsNullOrEmpty(view)?"overview":view)+".png"),tex.EncodeToPNG());
        cam.targetTexture=old; RenderTexture.active=prev; UnityEngine.Object.DestroyImmediate(tex); rt.Release(); UnityEngine.Object.DestroyImmediate(rt);
        cam.transform.SetPositionAndRotation(pos,rot);
    }
}
