using UnityEngine;

public class WorkshopVisitor : MonoBehaviour
{
    public Camera viewCamera;
    public CharacterController body;
    public bool walking;
    float yaw,pitch,vertical;
    public int preset;
    public string ViewName=>walking?"自由巡视 / WALK":new[]{"全景 / OVERVIEW","装配 / ASSEMBLY","检测 / SENSOR","控制柜 / PLC"}[preset];
    void Start(){SetView(0);}
    public void SetView(int n)
    {
        walking=false;body.enabled=false;preset=n;
        var positions=new[]{new Vector3(-8.6f,5.35f,-9.4f),new Vector3(-1.8f,3.25f,-3.2f),new Vector3(-4.4f,2.05f,-2.5f),new Vector3(-6.2f,2.5f,2.5f)};
        var targets=new[]{new Vector3(.4f,1.4f,1.2f),new Vector3(1,1.5f,.8f),new Vector3(-2.7f,1.28f,0),new Vector3(-5.2f,1.6f,4.85f)};
        viewCamera.transform.SetParent(null);viewCamera.transform.position=positions[n];viewCamera.transform.LookAt(targets[n]);
    }
    public void Walk()
    {
        walking=true;transform.position=new Vector3(-5,1,-4.9f);transform.rotation=Quaternion.identity;body.enabled=true;
        viewCamera.transform.SetParent(transform);viewCamera.transform.localPosition=new Vector3(0,.65f,0);yaw=30;pitch=0;vertical=0;Rotate();
    }
    void Rotate(){transform.rotation=Quaternion.Euler(0,yaw,0);viewCamera.transform.localRotation=Quaternion.Euler(pitch,0,0);}
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))SetView(0);if(Input.GetKeyDown(KeyCode.Alpha2))SetView(1);if(Input.GetKeyDown(KeyCode.Alpha3))SetView(2);if(Input.GetKeyDown(KeyCode.Alpha4))SetView(3);if(Input.GetKeyDown(KeyCode.F))Walk();
        if(!walking)return;
        if(Input.GetMouseButton(1)) {yaw+=Input.GetAxisRaw("Mouse X")*2;pitch=Mathf.Clamp(pitch-Input.GetAxisRaw("Mouse Y")*2,-78,78);Rotate();}
        float speed=Input.GetKey(KeyCode.LeftShift)?4.3f:2.5f;Vector3 move=transform.right*Input.GetAxisRaw("Horizontal")+transform.forward*Input.GetAxisRaw("Vertical");move=Vector3.ClampMagnitude(move,1)*speed;
        if(body.isGrounded)vertical=-2;else vertical-=9.81f*Time.deltaTime;body.Move((move+Vector3.up*vertical)*Time.deltaTime);
        if(transform.position.y<-3){body.enabled=false;transform.position=new Vector3(-5,1,-4.9f);body.enabled=true;}
    }
}
