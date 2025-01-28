using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Grid : MonoBehaviour 
{

    public Dictionary<Vector2, Node> nodes = new Dictionary<Vector2, Node>();

    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (Mathf.Abs(x) == Mathf.Abs(y)) continue;
                

                int CheckX = node.GridX + x;
                int CheckY = node.GridY + y;

                if(nodes.ContainsKey(new Vector2(CheckX, CheckY)))
                {
                    neighbours.Add(nodes[new Vector2(CheckX, CheckY)]);
                }
                
            }
        }
        return neighbours;
    }

    public Node GetRandomNode()
    {
        return nodes.ElementAt(Random.Range(0, nodes.Count)).Value;
    }

    public void AddNode(Vector2 Position,Node node)
    {

        //TODO: Change this to not need to caculate anything
        nodes.Add(new Vector2((int)Position.x / 20,(int) Position.y / 20), node);
    }


    public Node GetNodeFromWorldPoint(Vector3 worldPoint)
    {
        int x;
        int y;
        if (worldPoint.x < 0) x = (int)(worldPoint.x - 10) / 20;
        else x = (int)(worldPoint.x + 10) / 20;
        if (worldPoint.z < 0) y = (int)(worldPoint.z - 10) / 20;
        else y = (int)(worldPoint.z + 10) / 20;

       // Debug.Log("Current at " + x + ", " + y + " because (" + worldPoint.x + "," + worldPoint.z + ")");
       if (nodes[new Vector2(x, y)] == null) {
            //carAgent.FallenOff = true;
            return null;
        }
        return nodes[new Vector2(x, y)];

    }

}
