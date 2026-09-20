using UnityEngine;
using UnityEngine.UI;
public class SimulationSpeedButton : MonoBehaviour { public AssemblyLineController line;public Text caption;public void CycleSpeed(){line.simulationSpeed=line.simulationSpeed==1?1.5f:line.simulationSpeed==1.5f?2:line.simulationSpeed==2?.5f:1;caption.text="速度 ×"+line.simulationSpeed.ToString("0.0");} }
