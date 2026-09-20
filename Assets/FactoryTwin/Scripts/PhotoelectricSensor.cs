using UnityEngine;

public class PhotoelectricSensor : MonoBehaviour
{
    public Transform emitter;
    public LineRenderer beam;
    public Renderer indicator;
    public Material idleMaterial,activeMaterial;
    public bool detected;
    public void Sense()
    {
        Physics.SyncTransforms();
        detected=false;Vector3 start=emitter.position,end=start+Vector3.forward*1.55f;
        if(Physics.Raycast(start,Vector3.forward,out var hit,1.55f))
        {
            detected=hit.collider.GetComponentInParent<AssemblyWorkpiece>()!=null;
            if(detected)end=hit.point;
        }
        beam.SetPosition(0,start);beam.SetPosition(1,end);
        indicator.sharedMaterial=detected?activeMaterial:idleMaterial;
    }
}
