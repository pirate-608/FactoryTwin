using UnityEngine;
using UnityEditor;

public static partial class FactorySceneBuilder
{
    static partial void RobotCell()
    {
        DeleteRoot("03  ROBOT | six-axis vacuum assembly");var p=Group("03  ROBOT | six-axis vacuum assembly");
        Box("Robot steel plinth",p,V(1.55f,.34f,2.15f),V(1.15f,.68f,1.10f),dark,true);
        Rounded("Robot base plate",p,V(1.55f,.72f,2.15f),V(1.28f,.10f,1.20f),.022f,steel);
        for(int i=-1;i<=1;i+=2)for(int j=-1;j<=1;j+=2)Bolt(p,V(1.55f+i*.48f,.79f,2.15f+j*.44f),.05f);
        Cyl("J1 fixed bearing",p,V(1.55f,.86f,2.15f),.43f,.2f,dark);
        var rig=p.gameObject.AddComponent<RobotArm>();
        rig.baseYaw=new GameObject("J1 base yaw turret").transform;rig.baseYaw.SetParent(p);rig.baseYaw.position=V(1.55f,.98f,2.15f);
        Cyl("Orange turret",rig.baseYaw,Vector3.zero,.35f,.22f,orange);
        Rounded("Shoulder yoke",rig.baseYaw,V(0,.23f,0),V(.45f,.51f,.45f),.04f,orange);
        rig.shoulder=new GameObject("J2 shoulder axis").transform;rig.shoulder.SetParent(p);rig.shoulder.position=V(1.55f,1.4f,2.15f);
        Cyl("J2 motor housing",rig.shoulder,Vector3.zero,.25f,.60f,orange,V(0,0,90));
        Cyl("J2 servo face",rig.shoulder,V(-.32f,0,0),.18f,.07f,dark,V(0,0,90));
        rig.upperArm=ArmLink(p,"J2 upper arm",1.45f,.29f);
        rig.forearm=ArmLink(p,"J3 forearm",1.48f,.21f);
        rig.elbowJoint=Cyl("J3 elbow axis",p,V(1,2,2),.23f,.54f,orange,V(0,0,90)).transform;
        Cyl("Elbow servo cap",rig.elbowJoint,V(0,.54f,0),.64f,.20f,dark);
        rig.wrist=new GameObject("J4-J5 wrist orientation").transform;rig.wrist.SetParent(p);
        Cyl("J4 roll bearing",rig.wrist,V(0,.02f,0),.155f,.23f,dark);
        Cyl("J5 wrist casting",rig.wrist,V(0,-.08f,0),.135f,.14f,orange);
        Cyl("J6 tool flange",rig.wrist,V(0,-.185f,0),.13f,.045f,steel);
        rig.tool=new GameObject("TCP dual suction tool").transform;rig.tool.SetParent(p);
        Rounded("Vacuum end effector bar",rig.tool,V(0,.12f,0),V(.47f,.075f,.20f),.009f,steel);
        Cyl("Tool flange adapter",rig.tool,V(0,.17f,0),.055f,.07f,steel);
        for(int s=-1;s<=1;s+=2)
        {
            Cyl("Vacuum spring shaft",rig.tool,V(s*.16f,.071f,0),.021f,.07f,steel);
            Lathe("Bellows suction cup "+(s+1),rig.tool,V(s*.16f,0,0),new[]{new Vector2(.057f,0),new Vector2(.060f,.01f),new Vector2(.039f,.022f),new Vector2(.053f,.035f),new Vector2(.031f,.05f),new Vector2(.023f,.055f)},rubber);
            Cyl("Vacuum fitting blue collar",rig.tool,V(s*.16f,.17f,0),.027f,.033f,blue);
            Tube("Tool vacuum hose",rig.tool,new[]{V(s*.16f,.186f,0),V(s*.20f,.24f,0),V(0,.29f,.04f)},.014f,blue);
        }
        rig.airHose=new GameObject("Flexible robot pneumatic loom").AddComponent<LineRenderer>();rig.airHose.transform.SetParent(p,false);rig.airHose.sharedMaterial=rubber;rig.airHose.startWidth=.042f;rig.airHose.endWidth=.032f;rig.airHose.numCornerVertices=8;rig.airHose.numCapVertices=5;
        rig.Solve(rig.Home);
        Label("Robot branding","ROBOTLAB\nR6 / VACUUM",p,V(1.55f,.43f,1.585f),.105f,new Color(.87f,.93f,.96f));
        var feeder=new GameObject("B cover feeder pallet").transform;feeder.SetParent(p,false);feeder.localPosition=V(-.9f,0,2.35f);
        Box("Feeder worktop",feeder,V(0,1.08f,0),V(1.6f,.14f,1.35f),steel,true);
        for(int i=-1;i<=1;i+=2)for(int j=-1;j<=1;j+=2)Box("Feeder leg",feeder,V(i*.65f,.53f,j*.52f),V(.07f,1.06f,.07f),dark,true);
        Box("B fixture nest",feeder,V(0,1.16f,0),V(.8f,.05f,.61f),dark);
        for(int s=-1;s<=1;s+=2)Box("Feeder locating fence",feeder,V(s*.39f,1.26f,0),V(.035f,.18f,.6f),blue);
        var b=new GameObject("PART B · machined cover template").transform;b.SetParent(p,false);b.position=V(-.9f,1.19f,2.35f);BuildCover(b);
        for(int i=0;i<3;i++){var spare=new GameObject("B supply cassette "+i).transform;spare.SetParent(feeder,false);spare.localPosition=V(0,1.23f+i*.065f,.43f);BuildCover(spare);spare.localScale=V(.75f,1,.5f);}
        Label("Feeder sign","B / COVER FEEDER",feeder,V(0,.88f,-.7f),.105f,Color.white);
        // Guard panels leave the front observation aisle and operator access open.
        for(int i=0;i<5;i++)
        {
            float x=-2.6f+i*1.4f;
            Box("Fence post",p,V(x,1.15f,4.3f),V(.065f,2.3f,.065f),yellow,true);
            Box("Fence foot",p,V(x,.035f,4.3f),V(.23f,.07f,.23f),dark);
            if(i<4)
            {
                Box("Guard top rail",p,V(x+.7f,2.22f,4.3f),V(1.4f,.05f,.045f),dark);
                Box("Guard lower rail",p,V(x+.7f,.25f,4.3f),V(1.4f,.05f,.045f),dark);
                for(int j=1;j<10;j++)Box("Welded mesh vertical",p,V(x+j*.14f,1.23f,4.3f),V(.009f,1.95f,.009f),dark);
                for(int j=0;j<12;j++)Box("Welded mesh horizontal",p,V(x+.7f,.3f+j*.16f,4.3f),V(1.4f,.009f,.009f),dark);
                Box("Guard collision",p,V(x+.7f,1.15f,4.3f),V(1.4f,2.3f,.04f),dark,true).GetComponent<Renderer>().enabled=false;
            }
        }
        Box("Robot warning placard",p,V(2.3f,1.6f,4.23f),V(.70f,.5f,.025f),yellow);
        Label("Warning sign","!  ROBOT CELL\n自动运行区域",p,V(2.3f,1.6f,4.20f),.065f,Color.black);
    }
    static Transform ArmLink(Transform p,string name,float length,float width)
    {
        var t=new GameObject(name).transform;t.SetParent(p,false);
        Rounded(name+" casting",t,V(0,length*.49f,0),V(width,length-.15f,width*.9f),.035f,orange);
        Rounded(name+" side cover",t,V(-width*.52f,length*.50f,0),V(.035f,length*.69f,width*.65f),.008f,pale);
        for(int i=0;i<3;i++)Cyl("Cover fastener",t,V(-width*.55f,.25f+i*length*.28f,0),.021f,.022f,dark,V(0,0,90));
        return t;
    }
}
