using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.AI;
public enum directions
{
    forward,
    uturn,
    left,
    right,
    arrive
}

public class CarController : MonoBehaviour
{
    //

    public directions direction;
    public float distance;

    public Transform Target;
    Node targetNode;
    public List<Node> path = new List<Node>();
   // public Tuple<directions, float> direction = new Tuple<directions, float>(directions.forward, 0.0f);

    public void GetPath()
    {
         path = GameObject.Find("AStar").GetComponent<PathFInding>().FindPath(this.transform.position, Target.position);
        targetNode = GameObject.Find("AStar").GetComponent<Grid>().GetNodeFromWorldPoint(Target.position);
    }

    public void CalulateDirection()
    {
        GetPath();
        Vector3 NodePositionLocal = transform.InverseTransformPoint(path[0].gameObject.transform.position);

        Node TargetNode;
        if (NodePositionLocal.x < -3)
        {
            Debug.Log("Left because " + NodePositionLocal.x);
            TargetNode = path[0];
        }
        else if (NodePositionLocal.x > 3) { 
            Debug.Log("Right because " + NodePositionLocal.x);
            TargetNode = path[0];
        }
        else
        {
            Debug.Log("Forward or backwards because " + NodePositionLocal.x);
            bool forwardX = false;
            if (path[0].GridX != path[1].GridX) { 
                forwardX = true;
            }
            for (int i = 1; i < path.Count - 1; i++) { 
                if (forwardX)
                {
                    if (path[i].GridY != path[i + 1].GridY)
                    {
                        //Calculate if its left or right
                    }
                } else
                {
                    if (path[i].GridY != path[(i + 1)].GridY)
                    {
                        //Calulate if left or right.
                    }
                }
            }
            
        
        }
       

    }
}
