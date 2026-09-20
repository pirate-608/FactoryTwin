using UnityEngine;
public class DashboardAction : MonoBehaviour { public FactoryDashboard dashboard;public int action;public void Invoke(){dashboard.Press(action);} }
