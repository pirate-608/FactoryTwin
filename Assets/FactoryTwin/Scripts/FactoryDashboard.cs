using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class FactoryDashboard : MonoBehaviour
{
    public AssemblyLineController line;
    public WorkshopVisitor visitor;
    public Font font;
    public Text status,count,phaseLabel,metrics,io,events,viewLabel;
    public Text worldStatus,worldCount,worldMetrics,worldIO,worldStep;
    public Image progress,worldProgress;
    public Text[] steps;
    public GameObject chrome;
    float nextUI;
    readonly Color cyan=new Color(.2f,.94f,.76f), muted=new Color(.47f,.61f,.66f);
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.H))chrome.SetActive(!chrome.activeSelf);
        if(Input.GetMouseButtonDown(0) && !(EventSystem.current&&EventSystem.current.IsPointerOverGameObject()))
        {
            if(Physics.Raycast(visitor.viewCamera.ScreenPointToRay(Input.mousePosition),out var hit,6))
            {
                var b=hit.collider.GetComponent<WorkshopButton>();if(b)Press(b.action);
            }
        }
        if(Time.unscaledTime<nextUI)return;nextUI=Time.unscaledTime+.1f;
        status.text=line.StateTitle;status.color=line.emergency?new Color(1,.3f,.2f):cyan;
        count.text=line.completed.ToString("D4");phaseLabel.text=line.fault!=""?line.fault:AssemblyLineController.Names[(int)line.phase];
        metrics.text="本循环  "+line.cycleTime.ToString("00.0")+" s     上次  "+line.lastCycleTime.ToString("00.0")+" s";
        io.text="● 光电检测   "+(line.sensor.detected?"DETECTED":"CLEAR")+"\n● 下游夹持   "+(line.downstream.IsClosed?"CLAMPED":"OPEN")+"\n● 真空吸附   "+(line.vacuum?"−60 kPa":"OFF")+"\n● 输送电机   "+(line.beltMoving?"RUNNING":"IDLE")+"\n● 气源压力   0.60 MPa";
        events.text=string.Join("\n",line.history);viewLabel.text=visitor.ViewName;progress.fillAmount=line.Progress;
        for(int i=0;i<steps.Length;i++)
        {
            int phase=(int)line.phase;int stage=phase<=1?0:phase<=3?1:phase<=5?2:phase<=9?3:phase<=13?4:phase<=15?5:6;
            steps[i].color=i==stage?cyan:i<stage?new Color(.60f,.72f,.77f):muted;
        }
        worldStatus.text=line.StateTitle;worldStatus.color=status.color;worldCount.text=line.completed.ToString("D4");worldMetrics.text="LAST CYCLE  "+line.lastCycleTime.ToString("0.0")+" s     |     AIR  0.60 MPa     |     24V DC";
        worldIO.text="SENSOR   "+(line.sensor.detected?"ON":"OFF")+"      CLAMP   "+(line.downstream.IsClosed?"LOCK":"OPEN")+"      VACUUM   "+(line.vacuum?"−60 kPa":"OFF");
        worldStep.text="STEP "+((int)line.phase).ToString("D2")+"   /   "+AssemblyLineController.Names[(int)line.phase];worldProgress.fillAmount=line.Progress;
    }
    public void Press(int action)
    {
        switch(action){case 0:line.StartLine();break;case 1:line.TogglePause();break;case 2:line.ResetLine();break;case 3:line.EmergencyStop();break;case 4:line.StepOnce();break;case 5:line.EmergencyStop(true,false);break;case 6:line.EmergencyStop(false,true);break;}
    }
}
