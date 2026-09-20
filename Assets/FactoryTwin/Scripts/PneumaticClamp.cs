using UnityEngine;

public class PneumaticClamp : MonoBehaviour
{
    public Transform[] jaws;
    public Transform[] rods;
    [Range(0,1)] public float closed;
    public float target;
    public bool IsClosed => closed > .985f;
    public bool IsOpen => closed < .015f;
    public void SetImmediate(float value) { closed=target=value; Pose(); }
    public void Advance(float dt) { closed=Mathf.MoveTowards(closed,target,dt*1.65f);Pose(); }
    public void Pose()
    {
        for(int i=0;i<jaws.Length;i++)
        {
            float s=i==0?-1:1;
            jaws[i].localPosition=new Vector3(0,1.285f,s*Mathf.Lerp(.58f,.288f,closed));
            for(int j=0;j<2;j++)
            {
                var r=rods[i*2+j];float end=Mathf.Abs(jaws[i].localPosition.z)+.055f;float len=1.02f-end;
                r.localPosition=new Vector3(j==0?-.13f:.13f,1.265f,s*(end+len*.5f));r.localScale=new Vector3(.038f,len*.5f,.038f);
            }
        }
    }
}
