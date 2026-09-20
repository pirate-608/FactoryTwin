using System;
using System.Collections.Generic;
using UnityEngine;

public class AssemblyLineController : MonoBehaviour
{
    public enum Phase { Ready, ReleaseA, ToSensor, IdentifyA, ToAssembly, ClampA, ApproachB, LowerToB, VacuumOn, LiftB, TransferB, AlignB, SeatB, VacuumOff, Retract, HomeRobot, ReleaseAssembly, Discharge, Delivered }
    public PneumaticClamp upstream,downstream;
    public PhotoelectricSensor sensor;
    public RobotArm robot;
    public Transform templateA,templateB,beltRibs,finishedRoot;
    public Renderer[] stackLights;
    public Material lampOff,lampGreen,lampAmber,lampRed;
    public AudioSource motorAudio,airAudio;
    public Phase phase=Phase.Ready;
    public bool running,paused,emergency,robotStop,conveyorStop,vacuum,identified;
    public int completed;
    [Range(.25f,3)] public float simulationSpeed=1;
    public float phaseTime,cycleTime,lastCycleTime;
    public string fault="";
    public Transform activeA,activeB;
    public bool beltMoving;
    public readonly List<string> history=new List<string>();
    readonly List<GameObject> spawned=new List<GameObject>();
    Vector3 motionStart;
    Vector3 deliveryStart;
    bool singleStep;
    float beltTravel;
    public static readonly Vector3 FeedA=new Vector3(-4.7f,1.142f,0);
    public static readonly Vector3 FeedB=new Vector3(-.9f,1.19f,2.35f);
    public static readonly Vector3 Assembly=new Vector3(1.1f,1.142f,0);
    public string StateTitle => emergency?"急停锁定 / EMERGENCY":paused?"已暂停 / PAUSED":running?Names[(int)phase]:"待机就绪 / READY";
    public static readonly string[] Names={"待机就绪","上游夹具松开 A","输送至光电传感器","红外识别零件 A","输送至装配工位","下游气缸夹紧 A","机械臂移动至 B","双吸盘下降","建立真空 / 吸附 B","抬升零件 B","转运至装配工位","定位销对准","下降压合装配","解除真空","末端安全抬升","机械臂复位","下游夹具松开","成品输送至指定位置","成品入位 / 计数"};
    public float Progress => phase==Phase.Ready?0:(int)phase/(float)(Names.Length-1);
    public bool CanMove => running&&!paused&&!emergency;
    void Start(){Application.targetFrameRate=60;ResetLine();}
    public void StartLine()
    {
        if(emergency||!string.IsNullOrEmpty(fault))return;
        paused=false;running=true;singleStep=false;
        if(phase==Phase.Ready) {cycleTime=0;Enter(Phase.ReleaseA);}
        Log("启动 / 自动循环");
    }
    public void TogglePause(){if(!running||emergency)return;paused=!paused;Log(paused?"操作员暂停":"恢复运行");}
    public void StepOnce(){if(emergency||!string.IsNullOrEmpty(fault))return;running=true;paused=false;singleStep=true;if(phase==Phase.Ready)Enter(Phase.ReleaseA);}
    public void EmergencyStop(bool isRobot=false,bool isBelt=false)
    {
        emergency=true;paused=true;robotStop|=isRobot||!isBelt;conveyorStop|=isBelt||!isRobot;beltMoving=false;
        Log("急停锁定："+(isRobot?"机械臂":isBelt?"输送带":"整线")+"，复位后重新启动");
    }
    public void ResetLine()
    {
        running=false;paused=false;emergency=false;robotStop=conveyorStop=false;vacuum=false;identified=false;completed=0;phaseTime=cycleTime=lastCycleTime=0;phase=Phase.Ready;fault="";singleStep=false;beltMoving=false;beltTravel=0;
        foreach(var g in spawned)if(g){g.SetActive(false);Destroy(g);}spawned.Clear();
        templateA.gameObject.SetActive(false);templateB.gameObject.SetActive(false);upstream.SetImmediate(1);downstream.SetImmediate(0);robot.Solve(robot.Home);
        SpawnParts();history.Clear();Log("复位完成 · 气缸原位 · 计数清零");sensor.Sense();
    }
    void SpawnParts()
    {
        activeA=Instantiate(templateA,FeedA,Quaternion.identity,transform).transform;activeA.name="A housing / current workpiece";activeA.gameObject.SetActive(true);activeA.GetComponent<AssemblyWorkpiece>().assembled=false;spawned.Add(activeA.gameObject);
        activeB=Instantiate(templateB,FeedB,Quaternion.identity,transform).transform;activeB.name="B cover / current workpiece";activeB.gameObject.SetActive(true);spawned.Add(activeB.gameObject);
    }
    public void Simulate(float dt)
    {
        sensor.Sense();beltMoving=false;
        if(!CanMove){Lights();return;}
        phaseTime+=dt;cycleTime+=dt;upstream.Advance(dt);downstream.Advance(dt);
        switch(phase)
        {
            case Phase.ReleaseA: upstream.target=0;if(upstream.IsOpen)Next();break;
            case Phase.ToSensor:
                MoveA(-2.4f,dt);sensor.Sense();
                if(sensor.detected){identified=true;Next();}
                else if(Mathf.Abs(activeA.position.x+2.4f)<.002f && phaseTime>4)Fail("光电未检测到 A，已停止");break;
            case Phase.IdentifyA: if(phaseTime>.65f)Next();break;
            case Phase.ToAssembly: MoveA(1.1f,dt);if(Mathf.Abs(activeA.position.x-1.1f)<.001f)Next();break;
            case Phase.ClampA: downstream.target=1;if(downstream.IsClosed)Next();break;
            case Phase.ApproachB: Motion(FeedB+Vector3.up*.65f,1.2f);break;
            case Phase.LowerToB: Motion(FeedB+Vector3.up*.063f,.65f);break;
            case Phase.VacuumOn:
                if(phaseTime>.4f){vacuum=true;activeB.SetParent(robot.tool,true);activeB.localPosition=new Vector3(0,-.063f,0);Next();}break;
            case Phase.LiftB: Motion(FeedB+Vector3.up*.87f,.85f);break;
            case Phase.TransferB: Motion(Assembly+Vector3.up*1.10f,1.65f);break;
            case Phase.AlignB: Motion(Assembly+Vector3.up*.48f,.7f);break;
            case Phase.SeatB:
                if(!downstream.IsClosed||!identified){Fail("装配互锁未满足：夹紧 / 识别");break;}
                Motion(Assembly+Vector3.up*(.231f+.063f),.85f);break;
            case Phase.VacuumOff:
                if(phaseTime>.4f){vacuum=false;activeB.SetParent(activeA,true);activeB.localPosition=new Vector3(0,.231f,0);activeB.localRotation=Quaternion.identity;activeA.GetComponent<AssemblyWorkpiece>().assembled=true;Next();}break;
            case Phase.Retract: Motion(Assembly+Vector3.up*.95f,.8f);break;
            case Phase.HomeRobot: Motion(robot.Home,1.35f);break;
            case Phase.ReleaseAssembly: downstream.target=0;if(downstream.IsOpen)Next();break;
            case Phase.Discharge: MoveA(5.4f,dt);if(Mathf.Abs(activeA.position.x-5.4f)<.001f)Next();break;
            case Phase.Delivered:
                Vector3 dock=new Vector3(6.5f+completed%3*.74f,1.03f,(completed/3%2)*.70f-.32f);
                activeA.position=Vector3.Lerp(deliveryStart,dock,Mathf.SmoothStep(0,1,Mathf.Clamp01(phaseTime/1.5f)));
                if(phaseTime>=1.5f)
                {
                    if(!activeA.GetComponent<AssemblyWorkpiece>().assembled){Fail("成品未完成装配");break;}
                    completed++;lastCycleTime=cycleTime;
                    activeA.SetParent(finishedRoot,true);activeA.position=dock;
                    // Six visible buffer positions; retire the oldest pair to model downstream collection.
                    while(finishedRoot.childCount>6){var oldest=finishedRoot.GetChild(0);oldest.SetParent(null);oldest.gameObject.SetActive(false);Destroy(oldest.gameObject);}
                    spawned.RemoveAll(g=>!g);
                    identified=false;upstream.SetImmediate(1);SpawnParts();cycleTime=0;Enter(Phase.ReleaseA);
                    Log("成品到位 #"+completed.ToString("D4"));
                    if(singleStep){paused=true;singleStep=false;}
                }break;
        }
        if(beltMoving)
        {
            beltTravel=Mathf.Repeat(beltTravel+dt*1.25f,.148f);
            for(int i=0;i<beltRibs.childCount;i++){var t=beltRibs.GetChild(i);t.localPosition=new Vector3(-5.9f+i*.148f+beltTravel,1.134f,0);}
        }
        Lights();
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space)){if(!running)StartLine();else TogglePause();}
        if(Input.GetKeyDown(KeyCode.R))ResetLine();if(Input.GetKeyDown(KeyCode.X))EmergencyStop();if(Input.GetKeyDown(KeyCode.N))StepOnce();
        Simulate(Mathf.Min(Time.deltaTime,.06f)*simulationSpeed);
        if(motorAudio)motorAudio.volume=Mathf.MoveTowards(motorAudio.volume,beltMoving?.09f:0,Time.deltaTime*.5f);
    }
    void MoveA(float x,float dt){if(!upstream.IsOpen||!downstream.IsOpen){Fail("输送互锁：夹具尚未松开");return;}beltMoving=true;activeA.position=Vector3.MoveTowards(activeA.position,new Vector3(x,FeedA.y,0),dt*1.25f);}
    void Motion(Vector3 destination,float duration)
    {
        float t=Mathf.Clamp01(phaseTime/duration);float s=t*t*(3-2*t);robot.Solve(Vector3.Lerp(motionStart,destination,s));if(t>=1)Next();
    }
    void Next(){Enter((Phase)((int)phase+1));if(singleStep){paused=true;singleStep=false;}}
    void Enter(Phase value){phase=value;phaseTime=0;motionStart=robot.tcp;if(value==Phase.Delivered)deliveryStart=activeA.position;Log(Names[(int)value]);if(airAudio&&(value==Phase.ReleaseA||value==Phase.ClampA||value==Phase.VacuumOn||value==Phase.ReleaseAssembly))airAudio.Play();}
    void Fail(string message){fault=message;EmergencyStop();Log(message);}
    void Log(string s){history.Insert(0,DateTime.Now.ToString("HH:mm:ss")+"  "+s);if(history.Count>6)history.RemoveAt(6);}
    void Lights()
    {
        if(stackLights==null||stackLights.Length<3)return;
        stackLights[0].sharedMaterial=emergency?lampRed:lampOff;
        stackLights[1].sharedMaterial=paused||!running?lampAmber:lampOff;
        stackLights[2].sharedMaterial=CanMove?lampGreen:lampOff;
    }
}
