using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.Events;

public static partial class FactorySceneBuilder
{
    static Color uiDark=new Color(.022f,.05f,.067f,.94f),uiCyan=new Color(.18f,.91f,.72f),uiMuted=new Color(.55f,.68f,.72f);
    static partial void Finish()
    {
        DeleteRoot("05  SIMULATION | sequence and operator controls");DeleteRoot("06  UI | live dashboard");DeleteRoot("07  VISITOR | first person inspection");
        var system=Group("05  SIMULATION | sequence and operator controls");var line=system.gameObject.AddComponent<AssemblyLineController>();
        line.upstream=GameObject.Find("Upstream pneumatic clamp").GetComponent<PneumaticClamp>();line.downstream=GameObject.Find("Downstream pneumatic clamp").GetComponent<PneumaticClamp>();
        line.sensor=UnityEngine.Object.FindFirstObjectByType<PhotoelectricSensor>();line.robot=UnityEngine.Object.FindFirstObjectByType<RobotArm>();
        line.templateA=GameObject.Find("PART A · machined housing template").transform;line.templateB=GameObject.Find("PART B · machined cover template").transform;
        line.beltRibs=GameObject.Find("Moving belt texture ribs").transform;line.finishedRoot=GameObject.Find("Finished parts buffer").transform;
        line.stackLights=new Renderer[3];for(int i=0;i<3;i++)line.stackLights[i]=GameObject.Find("Stack light "+i).GetComponent<Renderer>();
        line.lampOff=dark;line.lampGreen=green;line.lampAmber=yellow;line.lampRed=red;
        var player=Group("07  VISITOR | first person inspection");player.position=V(-5,1,-4.9f);var visitor=player.gameObject.AddComponent<WorkshopVisitor>();visitor.viewCamera=Camera.main;
        visitor.body=player.gameObject.AddComponent<CharacterController>();visitor.body.height=1.85f;visitor.body.radius=.23f;visitor.body.stepOffset=.25f;visitor.body.center=Vector3.zero;visitor.body.enabled=false;
        // Invisible perimeter collision keeps the observation route inside the building.
        Box("South wall collision",player.parent,V(0,2,-9.5f),V(28,4,.10f),dark,true).GetComponent<Renderer>().enabled=false;
        var ui=Group("06  UI | live dashboard");var dash=ui.gameObject.AddComponent<FactoryDashboard>();dash.line=line;dash.visitor=visitor;dash.font=font;
        var canvas=new GameObject("Operator HUD",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));canvas.transform.SetParent(ui,false);var can=canvas.GetComponent<Canvas>();can.renderMode=RenderMode.ScreenSpaceOverlay;can.sortingOrder=10;
        var cs=canvas.GetComponent<CanvasScaler>();cs.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;cs.referenceResolution=new Vector2(1920,1080);cs.matchWidthOrHeight=.5f;
        var root=Rect("HUD content (H to hide)",canvas.transform,Vector2.zero,new Vector2(1920,1080),new Vector2(0,0));dash.chrome=root.gameObject;
        root.anchorMin=Vector2.zero;root.anchorMax=Vector2.one;root.offsetMin=root.offsetMax=Vector2.zero;
        var top=Panel("Header",root,0,-0,1920,83,new Vector2(0,1),uiDark);
        top.anchorMax=new Vector2(1,1);top.sizeDelta=new Vector2(0,83);
        UIText("Eyebrow","ROBOTLAB   /   FACTORY TWIN",top,32,-12,700,22,16,uiMuted);
        UIText("Title","智能装配生产线",top,30,-36,630,40,29,Color.white);
        dash.status=UIText("Live state","待机就绪 / READY",top,1170,-22,700,40,25,uiCyan);dash.status.alignment=TextAnchor.MiddleRight;
        var right=Panel("Production panel",root,-28,-109,342,638,new Vector2(1,1),uiDark);right.pivot=new Vector2(1,1);
        UIText("Panel eyebrow","PRODUCTION  /  生产监控",right,22,-20,310,25,17,uiMuted);
        dash.count=UIText("Count","0000",right,18,-55,295,85,64,Color.white);
        UIText("Count label","已完成装配并送达 / PCS",right,22,-142,300,25,15,uiMuted);
        dash.metrics=UIText("Metrics","本循环  00.0 s     上次  00.0 s",right,22,-179,305,32,15,uiMuted);
        Panel("Divider",right,22,-222,295,1,new Vector2(0,1),new Color(.17f,.26f,.30f));
        string[] stepNames={"01   上游松件","02   红外检测","03   到位夹紧","04   真空吸取 B","05   对位 / 压合","06   机械臂复位","07   放行 / 成品输出"};dash.steps=new Text[7];
        for(int i=0;i<7;i++)dash.steps[i]=UIText("Step "+i,stepNames[i],right,22,-239-i*32,295,29,17,i==0?uiCyan:uiMuted);
        dash.io=UIText("Live IO","",right,22,-478,302,150,15,uiMuted);dash.io.lineSpacing=1.28f;
        var lower=Panel("Operator controls",root,28,25,1110,145,new Vector2(0,0),uiDark);lower.pivot=Vector2.zero;
        dash.phaseLabel=UIText("Active operation","待机就绪",lower,20,-10,700,32,22,Color.white);
        var track=Panel("Cycle progress track",lower,20,-47,1069,4,new Vector2(0,1),new Color(.16f,.24f,.28f));dash.progress=Filled("Cycle progress",track,1069,4);
        Button("启动 START",lower,20,-69,177,52,uiCyan,()=>line.StartLine(),true);
        Button("暂停 PAUSE",lower,209,-69,174,52,new Color(.13f,.22f,.26f),()=>line.TogglePause());
        Button("复位 RESET",lower,395,-69,174,52,new Color(.13f,.22f,.26f),()=>line.ResetLine());
        Button("单步 STEP",lower,581,-69,168,52,new Color(.13f,.22f,.26f),()=>line.StepOnce());
        Button("急停 STOP",lower,761,-69,175,52,new Color(.73f,.18f,.10f),()=>line.EmergencyStop());
        Button("速度 ×1",lower,948,-69,141,52,new Color(.13f,.22f,.26f),()=>{});
        var speedButton=lower.Find("速度 ×1").GetComponent<UnityEngine.UI.Button>();var speedText=speedButton.GetComponentInChildren<Text>();speedButton.onClick.RemoveAllListeners();
        // Runtime behavior is wired through a serializable helper; no editor-only closures are required.
        WireButton(lower.Find("启动 START").gameObject,dash,0);WireButton(lower.Find("暂停 PAUSE").gameObject,dash,1);WireButton(lower.Find("复位 RESET").gameObject,dash,2);WireButton(lower.Find("单步 STEP").gameObject,dash,4);WireButton(lower.Find("急停 STOP").gameObject,dash,3);
        var speed=speedButton.gameObject.AddComponent<SimulationSpeedButton>();speed.line=line;speed.caption=speedText;UnityEventTools.AddPersistentListener(speedButton.onClick,speed.CycleSpeed);
        var views=Panel("Inspection camera toolbar",root,28,184,1090,57,new Vector2(0,0),uiDark);views.pivot=Vector2.zero;
        string[] names={"1  全景","2  装配特写","3  光电检测","4  控制柜","F  自由巡视"};
        for(int i=0;i<5;i++){var b=Button(names[i],views,10+i*150,-8,140,41,new Color(.08f,.14f,.18f),()=>{});var vc=b.gameObject.AddComponent<VisitorViewButton>();vc.visitor=visitor;vc.index=i;UnityEventTools.AddPersistentListener(b.onClick,vc.SelectView);}
        dash.viewLabel=UIText("View mode","全景 / OVERVIEW",views,785,-13,290,30,16,uiMuted);
        var help=Panel("Navigation help",root,28,250,760,34,new Vector2(0,0),new Color(.02f,.045f,.065f,.7f));help.pivot=Vector2.zero;
        UIText("Shortcuts","WASD 移动   ·   按住右键环顾   ·   Shift 加速   ·   H 隐藏界面   ·   Space 启停",help,12,-6,740,22,14,new Color(.78f,.85f,.88f));
        var log=Panel("Event log",root,-28,25,342,264,new Vector2(1,0),uiDark);log.pivot=new Vector2(1,0);
        UIText("Log title","EVENT LOG  /  运行日志",log,22,-17,300,27,16,uiMuted);dash.events=UIText("Log events","等待启动…",log,22,-54,300,190,13,uiMuted);dash.events.lineSpacing=1.3f;
        if(!UnityEngine.Object.FindFirstObjectByType<EventSystem>())new GameObject("UI Event System",typeof(EventSystem),typeof(StandaloneInputModule));
        WorldDisplay(dash);
        foreach(var pair in new[]{("Physical start button",0),("Physical reset button",2),("Physical emergency mushroom",3)}){var g=GameObject.Find(pair.Item1);g.AddComponent<SphereCollider>().radius=.62f;g.AddComponent<WorkshopButton>().action=pair.Item2;}
        PhysicalEmergency(line.robot.transform,V(2.13f,1.08f,1.8f),"Robot emergency stop",5);
        PhysicalEmergency(line.transform,V(4.25f,1.15f,-1.1f),"Conveyor emergency stop",6);
        MakeAudio(line);
        var probe=new GameObject("Workshop reflection probe").AddComponent<ReflectionProbe>();probe.transform.position=V(0,2,1);probe.size=V(26,12,22);probe.mode=ReflectionProbeMode.Realtime;probe.refreshMode=ReflectionProbeRefreshMode.OnAwake;probe.resolution=128;probe.intensity=.7f;probe.clearFlags=ReflectionProbeClearFlags.SolidColor;probe.backgroundColor=new Color(.33f,.39f,.43f);
        // Standard shader in deferred mode supplies metallic response, soft shadows and local lights.
        Camera.main.renderingPath=RenderingPath.Forward;Camera.main.allowMSAA=true;QualitySettings.antiAliasing=4;
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }
    static RectTransform Rect(string name,Transform parent,Vector2 pos,Vector2 size,Vector2 anchor)
    {var g=new GameObject(name,typeof(RectTransform));var r=g.GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=anchor;r.pivot=new Vector2(0,1);r.anchoredPosition=pos;r.sizeDelta=size;return r;}
    static RectTransform Panel(string name,Transform parent,float x,float y,float w,float h,Vector2 anchor,Color c)
    {var r=Rect(name,parent,new Vector2(x,y),new Vector2(w,h),anchor);var im=r.gameObject.AddComponent<Image>();im.color=c;im.raycastTarget=false;return r;}
    static Text UIText(string name,string value,Transform parent,float x,float y,float w,float h,int size,Color color)
    {var r=Rect(name,parent,new Vector2(x,y),new Vector2(w,h),new Vector2(0,1));var t=r.gameObject.AddComponent<Text>();t.font=font;t.fontSize=size;t.text=value;t.color=color;t.alignment=TextAnchor.MiddleLeft;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Truncate;t.raycastTarget=false;return t;}
    static UnityEngine.UI.Button Button(string name,Transform parent,float x,float y,float w,float h,Color color,UnityEngine.Events.UnityAction action,bool black=false)
    {var r=Panel(name,parent,x,y,w,h,new Vector2(0,1),color);var b=r.gameObject.AddComponent<UnityEngine.UI.Button>();b.targetGraphic=r.GetComponent<Image>();b.targetGraphic.raycastTarget=true;var colors=b.colors;colors.highlightedColor=new Color(1.15f,1.15f,1.15f);colors.pressedColor=new Color(.65f,.75f,.8f);b.colors=colors;var text=UIText("Label",name,r,0,0,w,h,17,black?new Color(.015f,.13f,.11f):Color.white);text.alignment=TextAnchor.MiddleCenter;return b;}
    static Image Filled(string name,Transform parent,float w,float h)
    {var r=Panel(name,parent,0,0,w,h,new Vector2(0,1),uiCyan);var i=r.GetComponent<Image>();i.sprite=AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");i.type=Image.Type.Filled;i.fillMethod=Image.FillMethod.Horizontal;i.fillAmount=0;return i;}
    static void WireButton(GameObject go,FactoryDashboard dash,int action){var b=go.AddComponent<DashboardAction>();b.dashboard=dash;b.action=action;UnityEventTools.AddPersistentListener(go.GetComponent<UnityEngine.UI.Button>().onClick,b.Invoke);}
    static void PhysicalEmergency(Transform parent,Vector3 position,string name,int action)
    {var g=new GameObject(name);g.transform.SetParent(parent,true);g.transform.position=position;Box("Stop switch box",g.transform,Vector3.zero,V(.23f,.22f,.14f),yellow);var button=Cyl(name+" mushroom",g.transform,V(0,0,-.10f),.072f,.07f,red,V(90,0,0));button.AddComponent<SphereCollider>().radius=.6f;button.AddComponent<WorkshopButton>().action=action;}
    static void WorldDisplay(FactoryDashboard dash)
    {
        var mount=GameObject.Find("Production information wall display").transform;
        var c=new GameObject("World production dashboard",typeof(RectTransform),typeof(Canvas));c.transform.SetParent(mount,false);c.transform.localPosition=V(0,0,-.128f);c.transform.localScale=Vector3.one*.00444f;c.GetComponent<RectTransform>().sizeDelta=new Vector2(1200,550);c.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;
        var root=Panel("Screen content",c.transform,-600,275,1200,550,new Vector2(.5f,.5f),new Color(.012f,.033f,.048f));
        UIText("Display brand","ROBOTLAB  /  PRODUCTION CONTROL",root,38,-26,1000,36,26,uiMuted);
        UIText("World title","智能装配 · 生产监控中心",root,35,-83,810,70,46,Color.white);
        dash.worldStatus=UIText("World state","待机就绪 / READY",root,39,-170,860,45,31,uiCyan);
        UIText("Count caption","COMPLETED / 已组装",root,860,-104,310,37,22,uiMuted);
        dash.worldCount=UIText("World count","0000",root,850,-149,320,115,86,Color.white);
        Panel("Screen rule",root,40,-294,1120,2,new Vector2(0,1),new Color(.14f,.29f,.32f));
        dash.worldStep=UIText("World step","STEP 00  /  待机就绪",root,39,-319,1100,50,30,Color.white);
        var track=Panel("World progress track",root,40,-394,1120,9,new Vector2(0,1),new Color(.10f,.20f,.24f));dash.worldProgress=Filled("World progress",track,1120,9);
        dash.worldIO=UIText("World IO","SENSOR OFF   CLAMP OPEN   VACUUM OFF",root,40,-422,1130,35,23,uiCyan);
        dash.worldMetrics=UIText("World metrics","LAST CYCLE 0.0 s  |  AIR 0.60 MPa  |  24V DC",root,40,-484,1130,29,20,uiMuted);
    }
    static void MakeAudio(AssemblyLineController line)
    {
        string folder="Assets/FactoryTwin/Audio";Directory.CreateDirectory(folder);
        WriteWave(folder+"/ConveyorMotor.wav",2,false);WriteWave(folder+"/PneumaticRelease.wav",.32f,true);AssetDatabase.Refresh();
        line.motorAudio=line.gameObject.AddComponent<AudioSource>();line.motorAudio.clip=AssetDatabase.LoadAssetAtPath<AudioClip>(folder+"/ConveyorMotor.wav");line.motorAudio.loop=true;line.motorAudio.playOnAwake=true;line.motorAudio.volume=0;
        line.airAudio=line.gameObject.AddComponent<AudioSource>();line.airAudio.clip=AssetDatabase.LoadAssetAtPath<AudioClip>(folder+"/PneumaticRelease.wav");line.airAudio.playOnAwake=false;line.airAudio.volume=.12f;
    }
    static void WriteWave(string path,float seconds,bool hiss)
    {int rate=22050,n=(int)(rate*seconds);var rng=new System.Random(4);using(var f=new BinaryWriter(File.Create(path))){f.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));f.Write(36+n*2);f.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));f.Write(16);f.Write((short)1);f.Write((short)1);f.Write(rate);f.Write(rate*2);f.Write((short)2);f.Write((short)16);f.Write(System.Text.Encoding.ASCII.GetBytes("data"));f.Write(n*2);for(int i=0;i<n;i++){double t=(double)i/rate;double v=hiss?(rng.NextDouble()*2-1)*Math.Pow(1-(double)i/n,2):.24*Math.Sin(t*Math.PI*2*80)+.1*Math.Sin(t*Math.PI*2*160)+.03*(rng.NextDouble()*2-1);f.Write((short)(v*10000));}}}
}
