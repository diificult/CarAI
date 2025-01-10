using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Node : MonoBehaviour
{

    public int gCost;
    public int hCost;

    public int GridX;
    public int GridY;

    public Node parent;

    public TextMeshPro TextMeshPro;

    public void Awake()
    {
        GameObject.Find("AStar").GetComponent<Grid>().AddNode(new Vector2(transform.position.x, transform.position.z), this);
        TextMeshPro.text = GridX + ", " + GridY;
    }

    public Node()
    {

    }

    public int fCost
    {
        get
        {
            return gCost + hCost;
        }

    }
}
