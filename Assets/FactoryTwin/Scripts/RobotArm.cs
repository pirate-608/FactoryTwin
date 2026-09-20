using UnityEngine;

// Geometric two-link IK with base yaw and a level, downward-facing wrist.
// Named joint housings and wrist axes remain separate editable objects.
public class RobotArm : MonoBehaviour
{
    public Transform shoulder,upperArm,forearm,elbowJoint,wrist,tool,baseYaw;
    public LineRenderer airHose;
    public float upperLength=1.45f, foreLength=1.48f;
    public Vector3 tcp=new Vector3(1.6f,2.55f,1.05f);
    public Vector3 Home => new Vector3(1.6f,2.55f,1.05f);
    public bool reachable=true;
    public void Solve(Vector3 target)
    {
        tcp=target;
        Vector3 a=shoulder.position,b=target+Vector3.up*.36f,d=b-a;
        float raw=d.magnitude;reachable=raw<upperLength+foreLength-.005f && raw>Mathf.Abs(upperLength-foreLength)+.01f;
        float len=Mathf.Clamp(raw,.05f,upperLength+foreLength-.005f);Vector3 dir=d.normalized;
        Vector3 bend=(Vector3.up-dir*Vector3.Dot(Vector3.up,dir)).normalized;
        if(bend.sqrMagnitude<.1f)bend=Vector3.back;
        float along=(upperLength*upperLength-foreLength*foreLength+len*len)/(2*len);
        float height=Mathf.Sqrt(Mathf.Max(0,upperLength*upperLength-along*along));
        Vector3 elbow=a+dir*along+bend*height;
        upperArm.SetPositionAndRotation(a,Quaternion.FromToRotation(Vector3.up,elbow-a));
        forearm.SetPositionAndRotation(elbow,Quaternion.FromToRotation(Vector3.up,b-elbow));
        elbowJoint.position=elbow;elbowJoint.rotation=Quaternion.Euler(0,Mathf.Atan2(d.x,d.z)*Mathf.Rad2Deg,90);
        baseYaw.rotation=Quaternion.Euler(0,Mathf.Atan2(d.x,d.z)*Mathf.Rad2Deg,0);
        wrist.SetPositionAndRotation(b,Quaternion.identity);tool.SetPositionAndRotation(target,Quaternion.identity);
        if(airHose){airHose.positionCount=6;airHose.SetPositions(new[]{a+Vector3.back*.17f,Vector3.Lerp(a,elbow,.5f)+Vector3.up*.22f,elbow+Vector3.up*.20f,Vector3.Lerp(elbow,b,.55f)+Vector3.up*.18f,b+Vector3.right*.14f,target+new Vector3(.12f,.12f,0)});}
    }
    void LateUpdate(){Solve(tcp);}
}
