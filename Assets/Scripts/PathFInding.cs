using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PathFInding : MonoBehaviour
{

    [SerializeField] Transform Target;
    [SerializeField] Transform Start;

    Grid Grid;
    // Start is called before the first frame update
    void Awake()
    {
        Grid = GetComponent<Grid>();
    }

    public void StartFind()
    {
        FindPath(Start.position, Target.position);
    }


    

    public List<Node> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Node startNode = Grid.GetNodeFromWorldPoint(startPos);
        Node targetNode = Grid.GetNodeFromWorldPoint(targetPos);
    //    Debug.Log("Start node is at " + startNode.GridX + ", " + startNode.GridY);
   //     Debug.Log("Target node is at " + targetNode.GridX + ", " + targetNode.GridY);

        List<Node> openSet = new List<Node>();
        HashSet<Node> closeSet = new HashSet<Node>();
        openSet.Add(startNode);

        while(openSet.Count > 0 ) {
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost)
                {
                    currentNode = openSet[i];
                }
            }
            openSet.Remove(currentNode);
            closeSet.Add(currentNode);
            if (currentNode == targetNode)
            {
                List<Node> path = ReTracePath(startNode, targetNode);
                //Found Path
                return path;
            }

            foreach (Node neighbour in Grid.GetNeighbours(currentNode)) {
                if (closeSet.Contains(neighbour)) continue;

                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;
                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                    }

                }
            
            }


        }
        return null;
    }

    List<Node> ReTracePath(Node Startnode, Node endNode)
    {
        List<Node> path = new List<Node>(); 
        Node currentNode = endNode;
        while (currentNode != Startnode) {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Add(currentNode);
        path.Reverse();


       return path;   
    }

    int GetDistance (Node nodeA, Node nodeB)
    {
        int distX = Mathf.Abs(nodeA.GridX - nodeB.GridX);
        int distY = Mathf.Abs(nodeA.GridY - nodeB.GridY);

        if (distX > distY)
        {
            return 14 *distY + (10* (distX-distY));
        }
        return 14 * distX + (10 * (distY - distX));
    }
}
