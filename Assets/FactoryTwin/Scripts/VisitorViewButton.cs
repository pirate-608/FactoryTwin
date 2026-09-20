using UnityEngine;
public class VisitorViewButton : MonoBehaviour {public WorkshopVisitor visitor;public int index;public void SelectView(){if(index==4)visitor.Walk();else visitor.SetView(index);} }
