using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Deterministic integration checks against the actual scene, UI events, sensor raycasts and IK.
public class FactoryAcceptance : MonoBehaviour
{
    [Serializable] public class Report {public bool passed;public string timestamp;public List<string> checks=new List<string>();public string failure;public float measuredCycleSeconds;}
    Report report=new Report();AssemblyLineController line;
    void Check(bool condition,string description){if(!condition)throw new Exception(description);report.checks.Add(description);}
    public IEnumerator Run()
    {
        yield return null;
        line=FindFirstObjectByType<AssemblyLineController>();line.enabled=false;
        try{RunChecks();report.passed=true;}catch(Exception e){report.failure=e.ToString();Debug.LogError("[Factory acceptance] "+e.Message);}
        report.timestamp=DateTime.Now.ToString("O");Directory.CreateDirectory(Path.Combine(Application.dataPath,"../Evidence"));File.WriteAllText(Path.Combine(Application.dataPath,"../Evidence/acceptance.json"),JsonUtility.ToJson(report,true));
        line.ResetLine();line.enabled=true;
        Debug.Log("[Factory acceptance] "+(report.passed?"PASS":"FAIL")+" / "+report.checks.Count+" checks");
        Destroy(this);
    }
    void RunChecks()
    {
        line.ResetLine();
        Check(line.phase==AssemblyLineController.Phase.Ready&&line.completed==0&&line.upstream.IsClosed&&line.downstream.IsOpen,"Reset: idle, zero count, upstream closed, downstream open");
        var dash=FindFirstObjectByType<FactoryDashboard>();
        dash.transform.Find("Operator HUD/HUD content (H to hide)/Operator controls/启动 START").GetComponent<Button>().onClick.Invoke();
        Check(line.running&&line.phase==AssemblyLineController.Phase.ReleaseA,"Start button invokes the serialized runtime sequence");
        for(int i=0;i<55;i++)line.Simulate(.04f);
        Check(line.activeA.position.x>AssemblyLineController.FeedA.x,"Upstream release advances workpiece on belt");
        dash.transform.Find("Operator HUD/HUD content (H to hide)/Operator controls/暂停 PAUSE").GetComponent<Button>().onClick.Invoke();
        var before=line.activeA.position;float time=line.phaseTime;var tcp=line.robot.tcp;
        for(int i=0;i<30;i++)line.Simulate(.04f);
        Check(line.paused&&before==line.activeA.position&&time==line.phaseTime&&tcp==line.robot.tcp,"Pause freezes workpiece, sequence clock and robot TCP");
        line.TogglePause();
        var visited=new HashSet<AssemblyLineController.Phase>();bool seenSensor=false,seenCarry=false,seenSeated=false,ikOkay=true;
        for(int i=0;i<12000&&line.completed<2;i++)
        {
            line.Simulate(.04f);visited.Add(line.phase);seenSensor|=line.sensor.detected;
            if(line.vacuum)seenCarry|=line.activeB.parent==line.robot.tool;
            if(line.phase==AssemblyLineController.Phase.Retract)seenSeated|=line.activeB.parent==line.activeA&&Mathf.Abs(line.activeB.localPosition.y-.231f)<.001f;
            ikOkay&=line.robot.reachable;
            if(line.emergency)throw new Exception("Cycle fault: "+line.fault);
        }
        Check(seenSensor&&visited.Contains(AssemblyLineController.Phase.IdentifyA),"IR raycast detects the real workpiece collider and gates the sequence");
        Check(seenCarry,"Vacuum attaches cover B to the moving suction tool");
        Check(seenSeated,"Cover B seats on A at the alignment datum and remains attached");
        Check(ikOkay,"Robot targets remain inside the articulated arm reach envelope");
        Check(line.completed==2&&line.finishedRoot.childCount==2,"Two complete cycles produce exactly two delivered assemblies");
        report.measuredCycleSeconds=line.lastCycleTime;
        line.EmergencyStop(true,false);before=line.activeA.position;time=line.phaseTime;line.StartLine();line.Simulate(1);
        Check(line.emergency&&line.robotStop&&before==line.activeA.position&&time==line.phaseTime,"Robot emergency stop latches and rejects restart until reset");
        line.ResetLine();line.StartLine();for(int i=0;i<40;i++)line.Simulate(.04f);line.EmergencyStop(false,true);time=line.phaseTime;line.Simulate(1);
        Check(line.conveyorStop&&line.phaseTime==time,"Conveyor emergency stop freezes the interlocked cell");
        line.ResetLine();line.StepOnce();for(int i=0;i<100&&!line.paused;i++)line.Simulate(.04f);
        Check(line.paused&&line.phase==AssemblyLineController.Phase.ToSensor,"Single step completes exactly one operation then pauses");
        line.ResetLine();line.StartLine();for(int i=0;i<1500&&!line.vacuum;i++)line.Simulate(.04f);
        Check(line.vacuum,"Test setup reaches a suspended, vacuum-held part");line.ResetLine();
        Check(!line.vacuum&&line.completed==0&&!line.emergency&&line.activeB.parent!=line.robot.tool&&Vector3.Distance(line.robot.tcp,line.robot.Home)<.001f,"Reset during pickup clears vacuum, restores parts and returns robot home");
        line.StartLine();for(int i=0;i<12000&&line.completed<8;i++)line.Simulate(.04f);
        Check(line.completed==8&&line.finishedRoot.childCount==6,"Long-run buffer caps visible finished assemblies at six without count loss");
        line.ResetLine();line.StartLine();line.upstream.SetImmediate(0);line.downstream.SetImmediate(1);line.phase=AssemblyLineController.Phase.ToAssembly;line.Simulate(.04f);
        Check(line.emergency&&line.fault.Contains("互锁"),"Closed clamp prevents conveyor movement and raises an interlock fault");
        line.ResetLine();var visitor=FindFirstObjectByType<WorkshopVisitor>();visitor.Walk();
        Check(visitor.walking&&visitor.body.enabled,"Inspection mode enables the visitor collision capsule");
        for(int i=0;i<150;i++)visitor.body.Move(new Vector3(0,-.03f,.05f));
        Check(visitor.transform.position.z<-.85f&&visitor.transform.position.y>.7f,"Visitor capsule collides with conveyor and remains above the workshop floor");
        for(int i=0;i<4;i++){visitor.SetView(i);Check(!visitor.walking&&visitor.preset==i,"Camera preset "+(i+1)+" selects successfully");}
        visitor.SetView(0);
    }
}
