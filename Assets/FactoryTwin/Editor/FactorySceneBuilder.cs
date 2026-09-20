using System;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;

public static partial class FactorySceneBuilder
{
    public const string ScenePath="Assets/FactoryTwin/Scenes/AssemblyWorkshop.unity";
    static Material floor, pale, dark, steel, orange, blue, yellow, rubber, white, green, red, glow, screen;
    static Font font;
    static Transform Group(string name) { var g=new GameObject(name); Undo.RegisterCreatedObjectUndo(g,"Build "+name); return g.transform; }
    static Vector3 V(float x,float y,float z)=>new Vector3(x,y,z);
    static Material Mat(string name,Color color,float metal=0,float smooth=.3f,bool emission=false)
    {
        string path="Assets/FactoryTwin/Materials/"+name+".mat";
        var m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(!m) {m=new Material(Shader.Find("Standard")); AssetDatabase.CreateAsset(m,path);}
        m.color=color; m.SetFloat("_Metallic",metal); m.SetFloat("_Glossiness",smooth);
        if(emission){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",color*1.3f);}
        return m;
    }
    static void Materials()
    {
        floor=Mat("01 Epoxy concrete",new Color(.26f,.30f,.31f),.1f,.32f);
        pale=Mat("02 Powder coat ivory",new Color(.72f,.75f,.73f),.25f,.4f);
        dark=Mat("03 Graphite steel",new Color(.07f,.09f,.105f),.5f,.45f);
        steel=Mat("04 Brushed aluminium",new Color(.57f,.65f,.69f),.8f,.55f);
        orange=Mat("05 Robot safety orange",new Color(1,.255f,.045f),.38f,.43f);
        blue=Mat("06 Pneumatic blue",new Color(.025f,.27f,.50f),.35f,.4f);
        yellow=Mat("07 Safety yellow",new Color(1,.68f,.055f),.1f,.35f);
        rubber=Mat("08 Rubber",new Color(.024f,.031f,.036f),.05f,.23f);
        white=Mat("09 Porcelain white",new Color(.85f,.89f,.91f),.05f,.4f);
        green=Mat("10 Signal green",new Color(.045f,.95f,.44f),.15f,.5f,true);
        red=Mat("11 Signal red",new Color(1,.04f,.025f),.05f,.4f,true);
        glow=Mat("12 Luminaire",new Color(.73f,.87f,1),0,.35f,true);
        screen=Mat("13 Display glass",new Color(.015f,.032f,.046f),.2f,.25f);
        font=AssetDatabase.LoadAssetAtPath<Font>("Assets/FactoryTwin/Fonts/WorkshopChinese.ttf");
        if(!font)font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }
    static GameObject Box(string name,Transform p,Vector3 pos,Vector3 scale,Material m,bool collision=false)
    {
        var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(p,false);g.transform.localPosition=pos;g.transform.localScale=scale;
        g.GetComponent<Renderer>().sharedMaterial=m;if(!collision)UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());return g;
    }
    static GameObject Cyl(string name,Transform p,Vector3 pos,float radius,float length,Material m,Vector3? euler=null)
    {
        var g=GameObject.CreatePrimitive(PrimitiveType.Cylinder);g.name=name;g.transform.SetParent(p,false);g.transform.localPosition=pos;g.transform.localScale=V(radius*2,length/2,radius*2);
        if(euler.HasValue)g.transform.localEulerAngles=euler.Value;g.GetComponent<Renderer>().sharedMaterial=m;UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());return g;
    }
    static GameObject Ball(string name,Transform p,Vector3 pos,Vector3 scale,Material m)
    {
        var g=GameObject.CreatePrimitive(PrimitiveType.Sphere);g.name=name;g.transform.SetParent(p,false);g.transform.localPosition=pos;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=m;UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());return g;
    }
    static TextMesh Label(string name,string value,Transform p,Vector3 pos,float size,Color color,Vector3? rotation=null,TextAnchor anchor=TextAnchor.MiddleCenter)
    {
        var g=new GameObject(name);g.transform.SetParent(p,false);g.transform.localPosition=pos;g.transform.localEulerAngles=rotation??Vector3.zero;
        var t=g.AddComponent<TextMesh>();t.text=value;t.font=font;t.fontSize=64;t.characterSize=size/5;t.anchor=anchor;t.alignment=TextAlignment.Center;t.color=color;t.GetComponent<MeshRenderer>().sharedMaterial=font.material;return t;
    }
    static void Rod(string name,Transform p,Vector3 a,Vector3 b,float r,Material mat)
    {
        var g=Cyl(name,p,(a+b)/2,r,Vector3.Distance(a,b),mat);g.transform.localRotation=Quaternion.FromToRotation(Vector3.up,(b-a).normalized);
    }
    static void Tube(string name,Transform p,Vector3[] points,float width,Material mat)
    {
        var g=new GameObject(name);g.transform.SetParent(p,false);var l=g.AddComponent<LineRenderer>();l.useWorldSpace=false;l.positionCount=points.Length;l.SetPositions(points);l.startWidth=width;l.endWidth=width;l.numCornerVertices=5;l.numCapVertices=5;l.sharedMaterial=mat;
    }
    static void Bolt(Transform p,Vector3 pos,float r=.022f){Cyl("Socket cap screw",p,pos,r,.025f,dark);Cyl("Hex recess",p,pos+V(0,.014f,0),r*.45f,.001f,rubber);}
    static void DeleteRoot(string name){var old=GameObject.Find(name);if(old)Undo.DestroyObjectImmediate(old);}
    [MenuItem("Factory Twin/Build 1 - Workshop")]
    static void Stage0()=>BuildStage(0);
    [MenuItem("Factory Twin/Build 2 - Conveyor")]
    static void Stage1()=>BuildStage(1);
    [MenuItem("Factory Twin/Build 3 - Robot")]
    static void Stage2()=>BuildStage(2);
    [MenuItem("Factory Twin/Build 4 - Controls")]
    static void Stage3()=>BuildStage(3);
    [MenuItem("Factory Twin/Build 5 - Simulation")]
    static void Stage4()=>BuildStage(4);
    public static void BuildStage(int stage)
    {
        Materials();
        if(stage==0)Workshop();
        if(stage==1)Conveyor();
        if(stage==2)RobotCell();
        if(stage==3)ControlRoom();
        if(stage==4)Finish();
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),ScenePath);AssetDatabase.SaveAssets();
        FactoryLiveEditor.Frame();
        SceneView.lastActiveSceneView.ShowNotification(new GUIContent("FACTORY TWIN  /  STAGE "+(stage+1)+" COMPLETE"),4);
        Debug.Log("[Factory Twin] Construction stage "+stage+" saved.");
    }
    static partial void Conveyor();
    static partial void RobotCell();
    static partial void ControlRoom();
    static partial void Finish();
    static void Workshop()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        PlayerSettings.companyName="RobotLab";PlayerSettings.productName="Factory Twin — Assembly Workshop";
        PlayerSettings.colorSpace=ColorSpace.Linear;PlayerSettings.defaultScreenWidth=1920;PlayerSettings.defaultScreenHeight=1080;PlayerSettings.runInBackground=true;
        QualitySettings.SetQualityLevel(QualitySettings.names.Length-1,true);QualitySettings.antiAliasing=4;QualitySettings.shadowDistance=45;QualitySettings.shadows=ShadowQuality.All;
        RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.57f,.65f,.73f);RenderSettings.ambientEquatorColor=new Color(.36f,.42f,.46f);RenderSettings.ambientGroundColor=new Color(.22f,.24f,.25f);RenderSettings.ambientIntensity=1;
        RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.006f;RenderSettings.fogColor=new Color(.29f,.36f,.42f);
        var p=Group("01  WORKSHOP | architectural shell");
        Box("Epoxy floor",p,V(0,-.16f,1),V(28,.3f,22),floor,true);
        for(int x=-14;x<=14;x+=2)Box("Floor expansion joint",p,V(x,.001f,1),V(.013f,.003f,22),dark);
        for(int z=-10;z<=12;z+=2)Box("Floor expansion joint",p,V(0,.002f,z),V(28,.003f,.013f),dark);
        Box("North wall",p,V(0,3.5f,10),V(28,7,.22f),pale,true);Box("West wall",p,V(-13,3.5f,0),V(.22f,7,20),pale,true);Box("East wall",p,V(13,3.5f,0),V(.22f,7,20),pale,true);
        Box("Blue wall dado",p,V(0,.75f,9.84f),V(26,1.5f,.08f),blue);
        for(int x=-12;x<=12;x+=6)
        {
            Box("Steel column",p,V(x,3.5f,9.6f),V(.32f,7,.32f),dark);
            Box("Column foot",p,V(x,.1f,9.6f),V(.6f,.2f,.6f),yellow);
            Box("Roof truss lower chord",p,V(x,6.5f,1),V(.17f,.22f,18),dark);
            for(int z=-6;z<9;z+=3)Rod("Triangulated truss",p,V(x,6.5f,z),V(x,7.25f,z+1.5f),.055f,steel);
        }
        for(int x=-10;x<=10;x+=4)
        {
            Box("Clerestory frame",p,V(x,4.6f,9.8f),V(3.45f,1.9f,.13f),dark);
            Box("Daylight glazing",p,V(x,4.6f,9.70f),V(3.24f,1.67f,.03f),glow);
            Box("Window mullion",p,V(x,4.6f,9.66f),V(.055f,1.8f,.06f),steel);
        }
        for(int z=-4;z<=6;z+=5)for(int x=-8;x<=8;x+=8)
        {
            Box("Suspended LED housing",p,V(x,6.15f,z),V(3.1f,.12f,.35f),dark);
            Box("LED diffuser",p,V(x,6.08f,z),V(2.92f,.025f,.28f),glow);
            Rod("Light suspension",p,V(x-1,6.2f,z),V(x-1,6.6f,z),.012f,steel);
            var l=new GameObject("Soft overhead illumination").AddComponent<Light>();l.transform.SetParent(p);l.transform.position=V(x,5.8f,z);l.type=LightType.Point;l.range=11;l.intensity=1.1f;l.color=new Color(.82f,.91f,1);l.shadows=LightShadows.None;
        }
        var sun=new GameObject("Daylight key").AddComponent<Light>();sun.type=LightType.Directional;sun.transform.rotation=Quaternion.Euler(48,-36,0);sun.intensity=1.35f;sun.color=new Color(1,.93f,.82f);sun.shadows=LightShadows.Soft;sun.shadowBias=.035f;sun.shadowNormalBias=.2f;
        var camera=new GameObject("Main Camera").AddComponent<Camera>();camera.tag="MainCamera";camera.transform.position=V(-9.6f,6.7f,-10.7f);camera.transform.LookAt(V(.2f,1.1f,1));camera.fieldOfView=51;camera.nearClipPlane=.06f;camera.farClipPlane=120;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.26f,.33f,.39f);camera.allowHDR=true;camera.gameObject.AddComponent<AudioListener>();
        for(int i=0;i<2;i++)Box("Pedestrian lane edge",p,V(0,.01f,-4.5f-i*1.7f),V(24,.015f,.08f),yellow);
        for(int x=-10;x<12;x+=3)Label("Walkway stencil","WALKWAY  →",p,V(x,.025f,-5.3f),.19f,new Color(.83f,.85f,.76f),V(90,0,0));
        Box("Work cell zone",p,V(0,.006f,1),V(14,.01f,7.3f),Mat("14 Cell epoxy",new Color(.18f,.24f,.26f),.1f,.35f));
        for(int i=-1;i<=1;i+=2){Box("Cell yellow long edge",p,V(0,.018f,1+i*3.7f),V(14,.014f,.075f),yellow);Box("Cell yellow end edge",p,V(i*7,.018f,1),V(.075f,.014f,7.4f),yellow);}
        Label("Hall title","ROBOTLAB  /  ASSEMBLY HALL 01",p,V(-.5f,6.05f,9.62f),.43f,new Color(.12f,.18f,.23f));
        Label("Hall subtitle","智能装配车间     ·     气动夹持 / 视觉化监控 / 真空装配",p,V(-.5f,5.55f,9.62f),.18f,new Color(.18f,.28f,.32f));
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
    }
}
