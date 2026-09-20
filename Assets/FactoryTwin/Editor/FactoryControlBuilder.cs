using UnityEngine;
using UnityEditor;

public static partial class FactorySceneBuilder
{
    static partial void ControlRoom()
    {
        DeleteRoot("04  CONTROLS | PLC pneumatics and workshop equipment");var p=Group("04  CONTROLS | PLC pneumatics and workshop equipment");
        var roof=Box("Insulated roof panels",p,V(0,7.45f,1),V(28,.15f,22),pale);roof.GetComponent<Renderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
        for(int x=-12;x<=12;x+=2)Box("Roof panel seam",p,V(x,7.355f,1),V(.025f,.025f,22),dark);
        Box("Back-wall service conduit",p,V(0,2.4f,9.72f),V(25,.07f,.06f),steel);
        var cabinet=new GameObject("PLC cabinet / open inspection panel").transform;cabinet.SetParent(p,false);cabinet.localPosition=V(-5.2f,0,4.9f);
        Box("Cabinet plinth",cabinet,V(0,.12f,0),V(1.85f,.24f,.75f),dark,true);
        Box("Cabinet back",cabinet,V(0,1.38f,.3f),V(1.8f,2.55f,.08f),pale,true);
        Box("Cabinet left",cabinet,V(-.87f,1.38f,0),V(.07f,2.55f,.65f),pale,true);
        Box("Cabinet right",cabinet,V(.87f,1.38f,0),V(.07f,2.55f,.65f),pale,true);
        Box("Cabinet top",cabinet,V(0,2.65f,0),V(1.80f,.08f,.65f),pale,true);
        Box("Mounting backplate",cabinet,V(0,1.42f,.24f),V(1.64f,2.23f,.027f),steel);
        for(int i=0;i<3;i++)Box("DIN rail",cabinet,V(0,.75f+i*.65f,.17f),V(1.59f,.055f,.08f),steel);
        Box("Cable duct left",cabinet,V(-.72f,1.42f,.14f),V(.13f,2.2f,.16f),pale);
        Box("Cable duct right",cabinet,V(.72f,1.42f,.14f),V(.13f,2.2f,.16f),pale);
        for(int j=0;j<20;j++)for(int s=-1;s<=1;s+=2)Box("Duct finger slot",cabinet,V(s*.72f,.4f+j*.10f,.05f),V(.1f,.012f,.005f),dark);
        for(int i=0;i<5;i++)
        {
            Box("Incoming circuit breaker",cabinet,V(-.46f+i*.125f,2.12f,.03f),V(.11f,.32f,.22f),white);
            Box("Breaker switch",cabinet,V(-.46f+i*.125f,2.12f,-.09f),V(.065f,.05f,.035f),blue);
        }
        Box("NDR-150-24 power supply",cabinet,V(.43f,2.1f,.015f),V(.34f,.39f,.26f),steel);
        for(int i=0;i<7;i++)Box("PSU ventilation slot",cabinet,V(.31f+i*.036f,2.19f,-.122f),V(.013f,.17f,.005f),dark);
        Label("24V supply label","24V DC",cabinet,V(.43f,1.985f,-.125f),.036f,Color.black);
        Rounded("S7-1200 CPU 1214C",cabinet,V(-.25f,1.47f,-.025f),V(.52f,.40f,.26f),.012f,dark);
        Box("PLC face stripe",cabinet,V(-.25f,1.50f,-.167f),V(.48f,.065f,.015f),blue);
        Label("PLC face label","SIMATIC S7-1200\nCPU 1214C",cabinet,V(-.25f,1.405f,-.185f),.038f,Color.white);
        for(int i=0;i<8;i++)Ball("PLC IO LED",cabinet,V(-.44f+i*.054f,1.615f,-.175f),V(.015f,.015f,.01f),green);
        for(int i=0;i<2;i++){Box("IO relay module",cabinet,V(.15f+i*.18f,1.46f,-.005f),V(.15f,.39f,.25f),dark);for(int j=0;j<5;j++)Ball("IO channel LED",cabinet,V(.15f+i*.18f,1.57f-j*.047f,-.137f),V(.017f,.017f,.008f),green);}
        Box("Industrial Ethernet switch",cabinet,V(.52f,1.46f,-.01f),V(.14f,.4f,.23f),blue);
        for(int i=0;i<14;i++)Box("Numbered terminal block",cabinet,V(-.54f+i*.082f,.76f,.02f),V(.067f,.16f,.19f),i<2?yellow:pale);
        for(int i=0;i<10;i++)Tube("24V ferruled wiring",cabinet,new[]{V(-.51f+i*.104f,.84f,-.10f),V(-.51f+i*.104f,1.01f,.02f),V(-.61f,1.07f,.06f),V(-.61f,1.28f,.02f)},.008f,i%2==0?blue:orange);
        var door=new GameObject("Open cabinet door").transform;door.SetParent(cabinet,false);door.localPosition=V(-.91f,1.4f,-.30f);door.localEulerAngles=V(0,-107,0);
        Box("Door skin",door,V(.85f,0,0),V(1.7f,2.4f,.06f),pale,true);Box("Door handle",door,V(1.53f,0,-.075f),V(.07f,.30f,.08f),dark);
        Label("Cabinet service legend","ELECTRICAL CONTROL\nPLC / 24VDC / IO\n电气控制柜",door,V(.85f,.75f,-.04f),.085f,new Color(.12f,.22f,.28f));
        Label("Cabinet caution","⚡  220V",door,V(.85f,-.7f,-.041f),.10f,new Color(.8f,.35f,0));
        Compressor(p);
        // Freestanding operator HMI with tactile push buttons.
        var hmi=new GameObject("Operator HMI pedestal").transform;hmi.SetParent(p,false);hmi.localPosition=V(-3.0f,0,-2.25f);
        Box("HMI foot",hmi,V(0,.055f,0),V(.75f,.11f,.55f),dark,true);Box("HMI pedestal",hmi,V(0,.7f,.02f),V(.15f,1.35f,.15f),steel,true);
        Rounded("Operator console housing",hmi,V(0,1.39f,0),V(1.12f,.60f,.18f),.03f,pale);
        Box("Touchscreen bezel",hmi,V(-.13f,1.45f,-.106f),V(.73f,.39f,.035f),dark);
        Box("Touchscreen glass",hmi,V(-.13f,1.45f,-.13f),V(.65f,.31f,.01f),screen);
        Label("Local HMI face","LINE 01  /  AUTO\n触屏操作终端",hmi,V(-.13f,1.45f,-.139f),.053f,new Color(.16f,.91f,.7f));
        Cyl("Physical start button",hmi,V(-.35f,1.18f,-.125f),.042f,.04f,green,V(90,0,0));
        Cyl("Physical reset button",hmi,V(-.05f,1.18f,-.125f),.042f,.04f,blue,V(90,0,0));
        Cyl("Emergency stop yellow base",hmi,V(.37f,1.46f,-.13f),.09f,.035f,yellow,V(90,0,0));
        Cyl("Physical emergency mushroom",hmi,V(.37f,1.46f,-.17f),.06f,.075f,red,V(90,0,0));
        Label("Start reset label","启动           复位",hmi,V(-.22f,1.08f,-.125f),.04f,new Color(.1f,.15f,.18f));
        Label("Emergency label","EMERGENCY",hmi,V(.36f,1.29f,-.13f),.026f,Color.black);
        var mon=new GameObject("Production information wall display").transform;mon.SetParent(p,false);mon.position=V(1.1f,3.42f,5.65f);
        Rounded("Large screen frame",mon,Vector3.zero,V(5.60f,2.70f,.18f),.045f,dark);
        Box("Large screen active panel",mon,V(0,0,-.104f),V(5.37f,2.47f,.025f),screen);
        Box("Screen pedestal left",p,V(-.9f,1.02f,5.75f),V(.12f,2.04f,.15f),dark);Box("Screen pedestal right",p,V(3.1f,1.02f,5.75f),V(.12f,2.04f,.15f),dark);
        Cyl("Signal tower pole",p,V(3.1f,2.02f,2.85f),.028f,.9f,steel);
        for(int i=0;i<3;i++)Cyl("Stack light "+i,p,V(3.1f,2.77f-i*.16f,2.85f),.086f,.13f,i==0?red:i==1?yellow:green);
        Cyl("Stack light top",p,V(3.1f,2.86f,2.85f),.095f,.055f,dark);
        Props(p);
    }
    static void Compressor(Transform p)
    {
        var c=new GameObject("Pneumatic supply / compressor AFR2000 valve manifold").transform;c.SetParent(p,false);c.localPosition=V(-7,0,2.6f);
        Cyl("Compressed air tank",c,V(0,.47f,0),.3f,1.2f,pale,V(0,0,90));
        Ball("Tank head left",c,V(-.60f,.47f,0),V(.30f,.6f,.6f),pale);Ball("Tank head right",c,V(.60f,.47f,0),V(.30f,.6f,.6f),pale);
        for(int s=-1;s<=1;s+=2){Cyl("Compressor wheel",c,V(s*.4f,.19f,.21f),.15f,.12f,rubber,V(90,0,0));Box("Compressor rubber foot",c,V(s*.4f,.12f,-.2f),V(.10f,.24f,.12f),rubber);}
        Box("Compressor twin head block",c,V(0,.88f,0),V(.72f,.22f,.40f),dark);
        for(int s=-1;s<=1;s+=2)for(int j=0;j<5;j++)Box("Pump cooling fin",c,V(s*.23f,.96f+j*.042f,0),V(.25f,.018f,.32f),steel);
        Tube("Compressor grab handle",c,new[]{V(-.65f,.4f,.19f),V(-.9f,1.04f,.19f),V(-.9f,1.04f,-.19f),V(-.65f,.4f,-.19f)},.033f,dark);
        Box("Air regulator stand",c,V(1.03f,.77f,0),V(.08f,1.54f,.08f),steel);
        Box("AFR2000 valve body",c,V(1.02f,1.36f,-.02f),V(.26f,.20f,.20f),steel);
        Cyl("Regulator adjustment knob",c,V(1.02f,1.54f,-.02f),.075f,.17f,dark);
        Cyl("Filter bowl",c,V(1.02f,1.12f,-.02f),.073f,.29f,blue);
        Cyl("Pressure gauge bezel",c,V(1.02f,1.37f,-.17f),.105f,.08f,dark,V(90,0,0));
        Cyl("Pressure gauge face",c,V(1.02f,1.37f,-.217f),.086f,.008f,white,V(90,0,0));
        Rod("Pressure gauge needle",c,V(1.02f,1.37f,-.225f),V(1.061f,1.42f,-.225f),.005f,red);
        Label("Pressure setting","0.60 MPa",c,V(1.02f,1.327f,-.231f),.024f,Color.black);
        Box("Solenoid valve manifold",c,V(1.6f,.8f,-.04f),V(.56f,.16f,.21f),steel);
        for(int i=0;i<4;i++){Box("SY valve solenoid",c,V(1.4f+i*.13f,.92f,-.04f),V(.10f,.12f,.23f),blue);Tube("Manifold outlet tube",c,new[]{V(1.4f+i*.13f,.93f,.1f),V(1.4f+i*.13f,.4f,.25f),V(2.0f,.22f,.4f),V(2.3f,.30f,-1.5f)},.015f,blue);}
        Tube("Primary air supply",c,new[]{V(.5f,.6f,-.05f),V(.74f,.85f,-.05f),V(.85f,1.37f,-.02f)},.024f,blue);
        Label("Compressor ID","AIR SUPPLY / 0.60 MPa",c,V(0,.48f,-.307f),.057f,new Color(.1f,.16f,.19f));
    }
    static void Props(Transform p)
    {
        // Loaded industrial shelving provides workshop depth without obscuring the cell.
        for(int bay=0;bay<2;bay++)
        {
            var r=new GameObject("Material rack "+bay).transform;r.SetParent(p,false);r.localPosition=V(7.3f+bay*3.1f,0,7.7f);
            for(int s=-1;s<=1;s+=2)for(int z=-1;z<=1;z+=2)Box("Rack upright",r,V(s*1.35f,1.8f,z*.57f),V(.10f,3.6f,.10f),blue,true);
            for(int level=0;level<3;level++)
            {
                float y=.30f+level*1.15f;Box("Orange rack crossbeam",r,V(0,y,-.57f),V(2.8f,.11f,.10f),orange);Box("Rack shelf",r,V(0,y+.05f,0),V(2.70f,.065f,1.15f),steel);
                for(int j=0;j<3;j++){
                    var mat=Mat("16 Kraft carton",new Color(.49f,.36f,.21f),0,.15f);Box("Supply carton",r,V(-.9f+j*.88f,y+.35f,0),V(.77f,.55f,.85f),mat);
                    Box("Packing tape",r,V(-.9f+j*.88f,y+.63f,0),V(.10f,.01f,.84f),pale);Label("Carton inventory label","PARTS\nA / B",r,V(-.9f+j*.88f,y+.36f,-.43f),.054f,Color.white);
                }
            }
        }
        var bench=new GameObject("Maintenance workbench").transform;bench.SetParent(p,false);bench.localPosition=V(-9.5f,0,6.5f);
        Box("Workbench steel top",bench,V(0,1,0),V(2.8f,.12f,1.05f),steel,true);for(int s=-1;s<=1;s+=2)Box("Tool chest",bench,V(s*.9f,.48f,0),V(.75f,.9f,.9f),blue,true);
        for(int s=-1;s<=1;s+=2)for(int i=0;i<4;i++)Box("Drawer pull",bench,V(s*.9f,.25f+i*.19f,-.48f),V(.54f,.023f,.04f),steel);
        Box("Tool pegboard",bench,V(0,1.8f,.46f),V(2.75f,1.45f,.05f),dark);
        for(int i=0;i<9;i++){Rod("Hanging wrench",bench,V(-1.1f+i*.27f,1.45f,.42f),V(-1.1f+i*.27f,2.0f,.42f),.022f,steel);Cyl("Wrench ring",bench,V(-1.1f+i*.27f,2.04f,.42f),.05f,.023f,steel,V(90,0,0));}
        for(int j=0;j<2;j++)
        {
            var pal=new GameObject("Wood pallet "+j).transform;pal.SetParent(p,false);pal.localPosition=V(9.4f,.08f,3+j*1.6f);
            var wood=Mat("17 Pallet timber",new Color(.43f,.30f,.16f),0,.1f);
            for(int i=0;i<7;i++)Box("Pallet top slat",pal,V(-.6f+i*.20f,.07f,0),V(.16f,.07f,1.1f),wood);
            for(int i=-1;i<=1;i++)Box("Pallet bearer",pal,V(0,-.005f,i*.43f),V(1.4f,.10f,.10f),wood);
            Box("Stackable blue logistics bin",pal,V(0,.43f,0),V(1.1f,.65f,.85f),blue,true);Box("Bin rim",pal,V(0,.77f,0),V(1.14f,.035f,.90f),dark);
        }
        Box("Roll-up delivery door",p,V(12.8f,2.0f,4),V(.11f,4,4.1f),dark);
        for(int i=0;i<19;i++)Box("Door steel slat",p,V(12.72f,.15f+i*.20f,4),V(.035f,.18f,4),steel);
        Cyl("Fire extinguisher",p,V(-11.9f,.59f,3),.14f,.75f,red);Box("Extinguisher wall sign",p,V(-12.84f,1.9f,3),V(.025f,.55f,.4f),red);
        for(int i=0;i<6;i++){Box("Safety bollard",p,V(11,.44f,-4+i*2.3f),V(.16f,.88f,.16f),yellow,true);Box("Bollard black band",p,V(11,.58f,-4+i*2.3f),V(.17f,.15f,.17f),dark);}
    }
}
