"""Local MCP client for the installed Coplay Unity server; no custom wire protocol."""
import asyncio, json, sys, os, base64
from pathlib import Path
from mcp import ClientSession, StdioServerParameters
from mcp.client.stdio import stdio_client

ROOT=Path(__file__).resolve().parents[1]
sys.stdout.reconfigure(encoding="utf-8",errors="replace")
sys.stderr.reconfigure(encoding="utf-8",errors="replace")
async def main():
    params=StdioServerParameters(command=r"C:\Users\think\.local\bin\uvx.exe",args=["--offline","--from","mcpforunityserver==10.1.2","mcp-for-unity","--transport","stdio","--project-scoped-tools"],env=dict(os.environ,ALLUSERSPROFILE=r"C:\ProgramData"))
    errlog=open(ROOT/"Automation/mcp-server.log","a",encoding="utf-8")
    async with stdio_client(params,errlog=errlog) as (read,write):
        async with ClientSession(read,write) as session:
            await session.initialize()
            mode=sys.argv[1] if len(sys.argv)>1 else "discover"
            if mode=="discover":
                r=await session.list_resources();print(r.model_dump_json())
                ts=await session.list_tools();print(json.dumps([{"name":t.name,"description":t.description[:160]} for t in ts.tools]))
            elif mode=="schema":
                ts=await session.list_tools();print(json.dumps([t.model_dump() for t in ts.tools if t.name in sys.argv[2:]],ensure_ascii=False))
            elif mode=="resource":
                r=await session.read_resource(sys.argv[2]);print(r.model_dump_json())
            elif mode=="call":
                request=json.loads(Path(sys.argv[2]).read_text(encoding="utf-8-sig"))
                if isinstance(request,dict):request=[request]
                for q in request:
                    if "resource" in q:r=await session.read_resource(q["resource"])
                    else:r=await session.call_tool(q["tool"],q.get("args",{}))
                    data=r.model_dump(mode="json")
                    for c in data.get("content",[]):
                        if c.get("type")=="image":
                            dest=ROOT/"Evidence"/(q.get("image_name","mcp-capture")+".png");dest.write_bytes(base64.b64decode(c.pop("data")));c["saved_path"]=str(dest)
                    print(json.dumps(data,ensure_ascii=False))
asyncio.run(main())
