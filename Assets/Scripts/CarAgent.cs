using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class CarAgent : Agent
{

    public TextMeshProUGUI yPos;
    public TextMeshProUGUI Reward;
    public TextMeshProUGUI lblDirection;
    public TextMeshProUGUI lblDistance;


    public GameObject Target;
    public Grid grid;

    public List<Node> Route;

    public Material pathMat;
    public Material Road;

    public bool AtTarget;

    //public Node targetNode;


    //0 = forward, 1 = right, 2 = reverse, 3 = left
    public int Direction;
    public int distance;


    int lastChecked = 9;

    int maxSteps = 0;

    public float vxMax = 0f;
    public float vyMax = 0f;   

    public bool FallenOff = false;

    public Node CurrentNode;
    public Node CurrentNextNode;
    public int lengthAtCurrentNode = 0;

    float rTowardsNode = 0f;
    float rAwayNode = 0f;
    float rFalling = 0f;
    float rAtTarget = 0f;
    float rCorrectNode = 0f;
    float rIncorrectNode = 0f;
    float rTimeAtNode = 0f;

    public TextMeshProUGUI lblTowardsNode;
    public TextMeshProUGUI lblAwayNode;
    public TextMeshProUGUI lblFalling;
    public TextMeshProUGUI lblAtTarget;
    public TextMeshProUGUI lblCorrectNode;
    public TextMeshProUGUI lblIncorrectNode;
    public TextMeshProUGUI lblTimeAtNode;


    

    public void FixedUpdate()
    {
        if (GetComponent<Rigidbody>().velocity.y > vyMax) vyMax = GetComponent<Rigidbody>().velocity.z;
        if (GetComponent<Rigidbody>().velocity.x > vxMax) vxMax = GetComponent<Rigidbody>().velocity.x;
        yPos.text = "Local Y Pos: " + this.transform.localPosition.y;
        Reward.text = "Current Reward: " + GetCumulativeReward();
        switch (Direction)
        {
            case 0:
                lblDirection.text = "Current Direction: Straight";
                break;
            case 1:
                lblDirection.text = "Current Direction: Right";
                break;
            case 2:
                lblDirection.text = "Current Direction: U-Turn";
                break;
            case 3:
                lblDirection.text = "Current Direction: Left";
                break;
        }
        lblDistance.text = "Number of Nodes: " + distance;

        lblTowardsNode.text = "Towards Goal " + rTowardsNode;
        lblAwayNode.text = "Away from goal " + rAwayNode;
        lblFalling.text = "Falling Off" + rFalling;
        lblAtTarget.text = "At Target" + rAtTarget;
        lblCorrectNode.text = "Correct Node" + rCorrectNode;
        lblIncorrectNode.text = "Incorrect Node" + rIncorrectNode;
        lblTimeAtNode.text = " Time At Node" + rTimeAtNode;


        if (lastChecked >= 10)
        {
            
            GetPath();
            lastChecked = 0;
            if (CurrentNextNode == null) CurrentNextNode = Route[1];
            if (CurrentNode == null) CurrentNode = Route[0];
            else if (Route[0] == CurrentNode) lengthAtCurrentNode++;
            else
           {
           /*     Debug.Log("Moved nodes");
                if (Route[0] == CurrentNextNode)
                {
                    AddReward(3f);
                    rCorrectNode += 3f;
                    CurrentNextNode = Route[1];
                }
                else
                {
                    AddReward(-3f);
                    rIncorrectNode += -3f;
                    CurrentNextNode = Route[1];
                }*/
                lengthAtCurrentNode = 0;
                CurrentNode = Route[0];
                
            }

        }
        else lastChecked++;
    }



    public void GetPath()
    {
        foreach (Node node in Route)
        {
            node.gameObject.GetComponent<MeshRenderer>().material = Road;
        }
        Route = GameObject.Find("AStar").GetComponent<PathFInding>().FindPath(this.transform.position, Target.transform.position);
        foreach (Node node in Route)
        {
            node.gameObject.GetComponent<MeshRenderer>().material = pathMat;
        }
        findNextTurn();
       // targetNode = GameObject.Find("AStar").GetComponent<Grid>().GetNodeFromWorldPoint(Target.transform.position);
    }

    public void findNextTurn()
    {

        if (Route.Count == 1)
        {
            AtTarget = true;
            return;
        }
        //Check to see if the player is on the turn
        Vector3 nextNodeDirection = (Route[1].transform.position - transform.position).normalized;

        float forwardDot = Vector3.Dot(transform.forward, nextNodeDirection);
        float rightDot = Vector3.Dot(transform.right, nextNodeDirection);

        if (Mathf.Abs(forwardDot) > Mathf.Abs(rightDot))
        {
            //In front or behind
            if (forwardDot > 0)
            {

                for (int i = 1; i < Route.Count -1; i++)
                {
                    Vector3 a = Route[i-1].transform.position;  
                    Vector3 b = Route[i].transform.position;  
                    Vector3 c = Route[i+1].transform.position;
                    Vector3 ab = (b-a).normalized;
                    Vector3 bc = (c-b).normalized;

                    float angle = Vector3.Angle(ab, bc);

                   // Debug.Log("Angle: " + angle);
                    if (angle > 1)
                    {
                        Vector3 cross = Vector3.Cross(ab,bc);

                        Direction = cross.y > 0 ? 1 : 3;
                        distance = (int) Vector3.Distance(Route[0].transform.position, b) / 10;
                       // Debug.Log("Cross : " + cross.y + ", distance " + (int) Vector3.Distance(Route[0].transform.position, b) / 10);
                            break;
                    }
                    



                }


            } else
            {
                Direction = 2;
                distance = 0;
            }

        } else
        {
            if (rightDot > 0 ) Direction = 1;
            else Direction = 3;
            distance = 0;
        }


    }


    public override void Initialize()
    {
        //Initalise
    }

    public override void CollectObservations(VectorSensor sensor)
    {

        /** 
         *
         *  List of observations:
         *     - Velocity
         *     - View of the road
         *     - Sat-nav direction.
         *
         */


        //Velocity
        sensor.AddObservation(gameObject.GetComponent<Rigidbody>().velocity.x / 18.0f);
        sensor.AddObservation(gameObject.GetComponent<Rigidbody>().velocity.z /18.0f);
        sensor.AddObservation(Route[0].GridX / 15.0f);
        sensor.AddObservation(Route[0].GridY / 15.0f);
        sensor.AddObservation(transform.position.x / 150f);
        sensor.AddObservation(transform.position.z / 150f);
        sensor.AddObservation(distance / 15.0f);
        float speed = gameObject.GetComponent<Rigidbody>().velocity.magnitude / 18f;
        sensor.AddObservation(speed);

        AddOneHotEncoding(sensor, Direction + 1, 4);
       // Debug.Log("Current Observatios: Vx " + gameObject.GetComponent<Rigidbody>().velocity.x / 8.0f + ", Vy" + gameObject.GetComponent<Rigidbody>().velocity.z /8.0f + ", PosX: " + (Route[0].GridX / 15.0f) + " PosY: " + (((float)Route[0].GridY) / 15.0f) + " Distance: " + (distance / 15.0f));
        
       // sensor.AddObservation(FallenOff);

    
    }


    public override void OnActionReceived(ActionBuffers actions)
    {
      //  Debug.Log("Got an action");
        var continuousActions = actions.ContinuousActions;
        GetComponent<CarControl>().UpdateValues(continuousActions[0], continuousActions[1]);
        base.OnActionReceived(actions);

        //Calculate rewards

        if (maxSteps == 0 )
        {
            maxSteps = Route.Count;
        }

        
        if (Route.Count < maxSteps)
        {

            AddReward(3f * (maxSteps - Route.Count));
            rTowardsNode += 3.1f * (maxSteps - Route.Count);
            maxSteps = Route.Count;
        } else if (Route.Count > maxSteps)
        {
            AddReward(-3 * ( Route.Count - maxSteps));
            rAwayNode += -3 * (Route.Count - maxSteps);
            maxSteps = Route.Count; 
        }
        if (lengthAtCurrentNode > 60)
        {
            AddReward(-0.001f);
            rTimeAtNode += -0.0001f;
        }


        if (AtTarget)
        {
            AddReward(25.0f);
            rAtTarget += 25f;
            EndEpisode();   
        }
        else if (this.transform.localPosition.y < 0.06 )
        {
            AddReward(-45.0f);
            rFalling += -45f;
            EndEpisode();
        }
    }
    private void AddOneHotEncoding(VectorSensor sensor, int category, int numCategories)
    {
        if (category < 1 || category > numCategories)
        {
            Debug.LogError($"Invalid category: {category}. It must be between 1 and {numCategories}.");
            return;
        }

        for (int i = 1; i <= numCategories; i++)
        {
            sensor.AddObservation(i == category ? 1f : 0f);
        }
    }


    public override void OnEpisodeBegin()
    {

        rTowardsNode = 0f;
        rAwayNode = 0f;
        rFalling = 0f;
        rAtTarget = 0f;
        rCorrectNode = 0f;
        rIncorrectNode = 0f;
        rTimeAtNode = 0f;

        //Choose a random target
        //TODO put into a managers class;
        Node random = grid.GetRandomNode();
       // Target.transform.position = random.transform.position;
        transform.position = new Vector3(0, 0.15f, 0);
        transform.rotation = Quaternion.identity;
        AtTarget = false;
        lastChecked = 0;
        CurrentNextNode = null;
        CurrentNode = null;
        lengthAtCurrentNode = 0;
        maxSteps = 0;
        GetPath();
    }


}
