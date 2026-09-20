using UnityEngine;
using UnityEditor;

public static partial class FactorySceneBuilder
{
    // Editable lathed mesh used for industrial rubber bellows, machined rims and cups.
    static GameObject Lathe(string name,Transform parent,Vector3 pos,Vector2[] profile,Material mat,int segments=40)
    {
        var vertices=new Vector3[(segments+1)*profile.Length];var uv=new Vector2[vertices.Length];
        for(int j=0;j<profile.Length;j++)for(int i=0;i<=segments;i++)
        {float a=i*Mathf.PI*2/segments;vertices[j*(segments+1)+i]=V(Mathf.Cos(a)*profile[j].x,profile[j].y,Mathf.Sin(a)*profile[j].x);uv[j*(segments+1)+i]=new Vector2((float)i/segments,(float)j/(profile.Length-1));}
        var tri=new int[segments*(profile.Length-1)*6];int t=0;
        for(int j=0;j<profile.Length-1;j++)for(int i=0;i<segments;i++){int a=j*(segments+1)+i,b=a+segments+1;tri[t++]=a;tri[t++]=b;tri[t++]=a+1;tri[t++]=a+1;tri[t++]=b;tri[t++]=b+1;}
        var mesh=new Mesh{name=name};mesh.vertices=vertices;mesh.triangles=tri;mesh.uv=uv;mesh.RecalculateNormals();mesh.RecalculateBounds();
        string path="Assets/FactoryTwin/Materials/"+name.Replace(" ","_")+".asset";
        var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(existing){EditorUtility.CopySerialized(mesh,existing);UnityEngine.Object.DestroyImmediate(mesh);mesh=existing;}else AssetDatabase.CreateAsset(mesh,path);
        var g=new GameObject(name);g.transform.SetParent(parent,false);g.transform.localPosition=pos;g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=mat;return g;
    }
    static GameObject Rounded(string name,Transform parent,Vector3 pos,Vector3 size,float bevel,Material mat)
    {
        // Rounded rectangle with an actual upper/lower bevel, not a scaled cube.
        int n=24;var v=new Vector3[n*4+2];var tris=new System.Collections.Generic.List<int>();
        for(int ring=0;ring<4;ring++)
        {
            float inset=(ring==0||ring==3)?bevel:0;float y=ring==0?-size.y/2: ring==1?-size.y/2+bevel: ring==2?size.y/2-bevel:size.y/2;
            float rx=size.x/2-inset,rz=size.z/2-inset,r=Mathf.Max(.007f,bevel*2-inset);
            for(int i=0;i<n;i++) {int corner=i/6;float a=(corner*90+(i%6)*18)*Mathf.Deg2Rad;float cx=corner==0||corner==3?rx-r:-rx+r,cz=corner<2?rz-r:-rz+r;v[ring*n+i]=V(cx+Mathf.Cos(a)*r,y,cz+Mathf.Sin(a)*r);}
        }
        v[n*4]=V(0,-size.y/2,0);v[n*4+1]=V(0,size.y/2,0);
        for(int i=0;i<n;i++){int k=(i+1)%n;tris.Add(n*4);tris.Add(i);tris.Add(k);tris.Add(n*4+1);tris.Add(n*3+k);tris.Add(n*3+i);for(int j=0;j<3;j++){int a=j*n+i,b=j*n+k;tris.Add(a);tris.Add(a+n);tris.Add(b);tris.Add(b);tris.Add(a+n);tris.Add(b+n);}}
        var m=new Mesh{name=name};m.vertices=v;m.triangles=tris.ToArray();m.RecalculateNormals();m.RecalculateBounds();
        string path="Assets/FactoryTwin/Materials/"+name.Replace(" ","_").Replace("/","_")+"_mesh.asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if(old){EditorUtility.CopySerialized(m,old);UnityEngine.Object.DestroyImmediate(m);m=old;}else AssetDatabase.CreateAsset(m,path);
        var g=new GameObject(name);g.transform.SetParent(parent,false);g.transform.localPosition=pos;g.AddComponent<MeshFilter>().sharedMesh=m;g.AddComponent<MeshRenderer>().sharedMaterial=mat;return g;
    }
    static partial void Conveyor()
    {
        DeleteRoot("02  CONVEYOR | pneumatic assembly line");var p=Group("02  CONVEYOR | pneumatic assembly line");
        Box("Continuous dark rubber belt",p,V(0,1.085f,0),V(11.8f,.09f,1.25f),rubber,true);
        Box("Lower return belt",p,V(0,.85f,0),V(11.8f,.04f,1.25f),rubber);
        for(int s=-1;s<=1;s+=2)
        {
            Box("Anodized side extrusion",p,V(0,.99f,s*.715f),V(12,.24f,.12f),steel,true);
            Box("Extrusion T-slot",p,V(0,1.01f,s*.779f),V(11.85f,.027f,.008f),dark);
            Box("Lower T-slot",p,V(0,.94f,s*.779f),V(11.85f,.015f,.008f),dark);
            Box("Guide rail",p,V(0,1.205f,s*.62f),V(11.6f,.045f,.035f),steel);
            for(float x=-5.5f;x<=5.6f;x+=2.2f)
            {
                Box("40x40 support leg",p,V(x,.49f,s*.62f),V(.10f,.98f,.10f),steel,true);
                Box("Leg groove",p,V(x,.48f,s*.678f),V(.035f,.8f,.004f),dark);
                Cyl("Adjustable foot",p,V(x,.07f,s*.62f),.13f,.08f,dark);Cyl("Threaded leveling stem",p,V(x,.14f,s*.62f),.027f,.12f,steel);
                Box("Rail standoff",p,V(x,1.15f,s*.64f),V(.045f,.16f,.045f),steel);
                Bolt(p,V(x,1.125f,s*.72f));
            }
        }
        for(float x=-5.5f;x<6;x+=2.2f)Box("Cross brace",p,V(x,.27f,0),V(.08f,.09f,1.25f),steel);
        for(float x=-5.7f;x<5.9f;x+=.36f)Cyl("Belt support roller",p,V(x,1,0),.071f,1.28f,steel,V(90,0,0));
        for(int x=-1;x<=1;x+=2)Cyl("Drive drum",p,V(x*5.86f,1,0),.13f,1.31f,dark,V(90,0,0));
        var slats=new GameObject("Moving belt texture ribs").transform;slats.SetParent(p,false);
        for(int i=0;i<80;i++)Box("Belt transverse rib",slats,V(-5.9f+i*.148f,1.134f,0),V(.015f,.006f,1.20f),dark);
        var motor=Cyl("Conveyor electric motor",p,V(5.25f,.78f,-1.05f),.18f,.52f,blue,V(90,0,0));
        for(int i=0;i<9;i++)Cyl("Motor cooling fin",p,V(5.25f,.78f,-.84f-i*.049f),.198f,.015f,dark,V(90,0,0));
        Box("Gear reduction case",p,V(5.25f,.90f,-.7f),V(.36f,.36f,.24f),steel);Box("Motor terminal box",p,V(5.25f,1.015f,-1.02f),V(.24f,.10f,.22f),blue);
        Clamp(p,"Upstream pneumatic clamp",-4.7f,true);Clamp(p,"Downstream pneumatic clamp",1.1f,false);
        Sensor(p);
        for(int i=0;i<4;i++)
        {
            float x=new[]{-4.7f,-2.4f,1.1f,5.15f}[i];
            Box("Station label plate",p,V(x,.72f,-.807f),V(1.55f,.28f,.035f),dark);
            Label("Station ID",new[]{"01  RELEASE / 上料","02  DETECT / 检测","03  ASSEMBLE / 装配","04  OUTPUT / 下料"}[i],p,V(x,.73f,-.832f),.09f,new Color(.85f,.95f,1));
        }
        Box("Output docking tray",p,V(5.45f,1.145f,0),V(.8f,.023f,.61f),blue);
        var template=Group("PART A · machined housing template");template.SetParent(p,false);template.localPosition=V(-4.7f,1.142f,0);
        BuildHousing(template);template.gameObject.AddComponent<AssemblyWorkpiece>();var col=template.gameObject.AddComponent<BoxCollider>();col.center=V(0,.12f,0);col.size=V(.70f,.24f,.52f);
        var parts=Group("Finished parts buffer");parts.SetParent(p,false);
        Box("Gravity output buffer table",p,V(7.25f,.86f,.05f),V(2.5f,.13f,1.7f),steel,true);
        for(int i=0;i<10;i++)Cyl("Output gravity roller",p,V(6.2f+i*.24f,.97f,.05f),.055f,1.60f,steel,V(90,0,0));
        for(int i=-1;i<=1;i+=2)for(int j=-1;j<=1;j+=2)Box("Output table leg",p,V(7.25f+i*1.05f,.42f,.05f+j*.67f),V(.07f,.84f,.07f),dark,true);
        Label("Output marker","FINISHED GOODS  /  成品区",p,V(7.25f,.62f,-.85f),.11f,Color.white);
    }
    static void Clamp(Transform parent,string name,float x,bool closed)
    {
        var p=new GameObject(name).transform;p.SetParent(parent,false);p.localPosition=V(x,0,0);var c=p.gameObject.AddComponent<PneumaticClamp>();c.jaws=new Transform[2];c.rods=new Transform[4];
        for(int i=0;i<2;i++)
        {
            int s=i==0?-1:1;
            Box("Cylinder mounting bracket",p,V(0,1.06f,s*1.02f),V(.50f,.08f,.76f),dark);
            Rounded("TN twin-rod cylinder body",p,V(0,1.27f,s*1.13f),V(.40f,.22f,.45f),.015f,pale);
            Box("Cylinder front plate",p,V(0,1.265f,s*.93f),V(.42f,.23f,.06f),steel);
            for(int j=0;j<2;j++)
            {
                float xx=j==0?-.13f:.13f;
                Cyl("Rod bearing seal",p,V(xx,1.265f,s*.892f),.048f,.025f,rubber,V(90,0,0));
                c.rods[i*2+j]=Cyl("Polished piston rod",p,V(xx,1.265f,s*.77f),.019f,.24f,steel,V(90,0,0)).transform;
                Bolt(p,V(xx,1.39f,s*1.12f));
            }
            var jaw=new GameObject("Actuated jaw "+i).transform;jaw.SetParent(p,false);c.jaws[i]=jaw;
            Box("Jaw guide plate",jaw,Vector3.zero,V(.43f,.20f,.08f),steel);
            Box("Soft clamping pad",jaw,V(0,.005f,-s*.052f),V(.35f,.135f,.035f),rubber);
            for(int j=0;j<2;j++){Cyl("Blue push fitting",p,V(.05f,1.405f,s*(1.02f+j*.21f)),.034f,.045f,blue);Tube("Pneumatic air tube",p,new[]{V(.05f,1.44f,s*(1.02f+j*.21f)),V(.19f,1.52f,s*1.3f),V(.38f,1.15f,s*1.43f),V(.4f,.45f,s*1.2f),V(.4f,.3f,.4f)},.017f,j==0?blue:rubber);}
            Box("Magnetic reed sensor",p,V(-.205f,1.28f,s*1.12f),V(.025f,.06f,.14f),dark);
        }
        c.SetImmediate(closed?1:0);
    }
    static void Sensor(Transform p)
    {
        var g=new GameObject("E3Z-D81 photoelectric sensor");g.transform.SetParent(p,false);g.transform.localPosition=V(-2.4f,0,0);
        var s=g.AddComponent<PhotoelectricSensor>();
        Box("Sensor L bracket",g.transform,V(0,1.25f,-.80f),V(.19f,.31f,.055f),steel);
        Rounded("Photoelectric black housing",g.transform,V(0,1.345f,-.745f),V(.15f,.21f,.08f),.01f,dark);
        Cyl("IR optical lens",g.transform,V(0,1.31f,-.694f),.036f,.021f,red,V(90,0,0));
        var emitter=new GameObject("IR emitter").transform;emitter.SetParent(g.transform,false);emitter.localPosition=V(0,1.31f,-.67f);s.emitter=emitter;
        s.indicator=Ball("Detection LED",g.transform,V(.055f,1.461f,-.745f),V(.026f,.012f,.026f),green).GetComponent<Renderer>();s.idleMaterial=green;s.activeMaterial=yellow;
        s.beam=new GameObject("Visible IR demonstration ray").AddComponent<LineRenderer>();s.beam.transform.SetParent(g.transform,false);s.beam.positionCount=2;s.beam.startWidth=.009f;s.beam.endWidth=.009f;s.beam.sharedMaterial=red;s.beam.SetPositions(new[]{V(-2.4f,1.31f,-.67f),V(-2.4f,1.31f,.72f)});
        Tube("Sensor cable",g.transform,new[]{V(0,1.32f,-.8f),V(.25f,1.16f,-.95f),V(.3f,.45f,-.82f),V(1,.3f,-.7f)},.012f,rubber);
        Label("Sensor legend","E3Z-D81\nPHOTOELECTRIC",g.transform,V(.01f,1.34f,-.805f),.027f,Color.white);
    }
    static void BuildHousing(Transform p)
    {
        Rounded("A cast aluminium bottom",p,V(0,.042f,0),V(.70f,.084f,.52f),.014f,steel);
        Rounded("A hollow rim left",p,V(-.297f,.144f,0),V(.106f,.16f,.49f),.009f,steel);
        Rounded("A hollow rim right",p,V(.297f,.144f,0),V(.106f,.16f,.49f),.009f,steel);
        Rounded("A hollow rim front",p,V(0,.144f,-.205f),V(.54f,.16f,.10f),.009f,steel);
        Rounded("A hollow rim back",p,V(0,.144f,.205f),V(.54f,.16f,.10f),.009f,steel);
        Box("Recessed cavity",p,V(0,.088f,0),V(.49f,.012f,.30f),dark);
        var copper=Mat("15 Interior bronze",new Color(.56f,.33f,.13f),.75f,.5f);
        Cyl("Internal precision bearing",p,V(0,.105f,0),.10f,.028f,copper);Cyl("Bearing bore",p,V(0,.122f,0),.055f,.005f,rubber);
        for(int i=-1;i<=1;i+=2)for(int j=-1;j<=1;j+=2){Cyl("Threaded boss",p,V(i*.275f,.225f,j*.18f),.036f,.015f,dark);Cyl("Locating dowel",p,V(i*.275f,.237f,j*.18f),.014f,.05f,steel);}
        for(int i=0;i<7;i++)Box("Casting cooling rib",p,V(-.22f+i*.073f,.13f,-.26f),V(.018f,.13f,.023f),steel);
        Label("Part marking","A · AL6061",p,V(0,.135f,-.275f),.039f,new Color(.12f,.18f,.21f));
    }
    static void BuildCover(Transform p)
    {
        Rounded("B precision machined lid",p,V(0,.023f,0),V(.70f,.046f,.52f),.01f,steel);
        Rounded("B anodized top insert",p,V(0,.049f,0),V(.46f,.014f,.31f),.005f,blue);
        for(int i=-1;i<=1;i+=2)for(int j=-1;j<=1;j+=2){Cyl("Countersunk socket",p,V(i*.275f,.047f,j*.18f),.027f,.004f,dark);Cyl("Socket inner",p,V(i*.275f,.050f,j*.18f),.016f,.004f,steel);}
        Label("Cover etching","ROBOTLAB  /  B-02",p,V(0,.06f,0),.032f,new Color(.78f,.90f,.96f),V(90,0,0));
    }
}
