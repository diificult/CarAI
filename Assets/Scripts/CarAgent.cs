using System;
using System.Collections.Generic;
using TMPro;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Random = UnityEngine.Random;

public class CarAgent : Agent
{

    public TextMeshProUGUI yPos;
    public TextMeshProUGUI Reward;
    public TextMeshProUGUI lblDirection;
    public TextMeshProUGUI lblDistance;


    public GameObject Target;
    public GameObject[] Targets;
    public GameObject TargetsObj;
    private int targetX, targetY;
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

    bool touchingWall = false;

    public Node CurrentNode;
    public Node CurrentNextNode;
    public float timeAtCurrentNode = 0;
    [SerializeField] private float LastDistanceToNextNode;
    [SerializeField] private float decayFactor = 0.1f;

    private Vector3 ClosestPointOnNextNode;

    float rTowardsNode = 0f;
    float rAwayNode = 0f;
    float rFalling = 0f;
    float rAtTarget = 0f;
    float rCorrectNode = 0f;
    float rIncorrectNode = 0f;
    float rTimeAtNode = 0f;
    float rTrainingWheels = 0f;
    float rTurn = 0f;
    float rTouchingWall;
    

    int targetNumber = 0;
    int episodeTargetCount = 0;
    int countTarget = 0;

    public TextMeshProUGUI lblTowardsNode;
    public TextMeshProUGUI lblAwayNode;
    public TextMeshProUGUI lblFalling;
    public TextMeshProUGUI lblAtTarget;
    public TextMeshProUGUI lblCorrectNode;
    public TextMeshProUGUI lblIncorrectNode;
    public TextMeshProUGUI lblTimeAtNode;
    public TextMeshProUGUI lblTurnReward;
    public TextMeshProUGUI lblTouchingWall;


    private Rigidbody rb;

    private Tuple<int, int> lastInstruction;

    [SerializeField] bool TrainingWheels = true;

    public void Start()
    {
        Application.targetFrameRate = 30;
        rb = GetComponent<Rigidbody>();
        Node n = grid.GetNodeFromWorldPoint(Target.transform.position);
        targetX = n.GridX;
        targetY = n.GridY;
    }
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision with" + collision.gameObject.tag);
        if (collision.gameObject.CompareTag("Wall"))
        {
            touchingWall = true;
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            touchingWall = false;
        }
    }


    public void FixedUpdate()
    {

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

        //TODO update these to work on events (check efficiency of this as im thinking this may cause lag)
        lblDistance.text = "Number of Nodes: " + distance;
        lblTowardsNode.text = "Towards Goal " + rTowardsNode;
        lblAwayNode.text = "Away from goal " + rAwayNode;
        lblFalling.text = "Falling Off " + rFalling;
        lblAtTarget.text = "Turn " + rTurn;
        lblCorrectNode.text = "Training Wheels " + rTrainingWheels;
        lblIncorrectNode.text = " " + rIncorrectNode;
        lblTimeAtNode.text = "Time At Node " + rTimeAtNode;
        lblTouchingWall.text = "Touching wall " + rTouchingWall;

        GetPath();
        lastChecked = 0;
        if (CurrentNextNode == null) CurrentNextNode = Route[1];
        if (CurrentNode == null) CurrentNode = Route[0];
        else if (Route[0] == CurrentNode) timeAtCurrentNode += Time.fixedDeltaTime;
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
            LastDistanceToNextNode = 0;
            timeAtCurrentNode = 0;
            CurrentNode = Route[0];

        }
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
        //Debug.Log(forwardDot + " " + rightDot);

        if (Mathf.Abs(forwardDot) > Mathf.Abs(rightDot))
        {

            //In front or behind
            if (forwardDot > 0)
            {
                bool isStraight = true;
                for (int i = 1; i < Route.Count - 1; i++)
                {
                    Vector3 a = Route[i - 1].transform.position;
                    Vector3 b = Route[i].transform.position;
                    Vector3 c = Route[i + 1].transform.position;
                    Vector3 ab = (b - a).normalized;
                    Vector3 bc = (c - b).normalized;

                    float angle = Vector3.Angle(ab, bc);

                    // Debug.Log("Angle: " + angle);
                    if (angle > 1)
                    {
                        Vector3 cross = Vector3.Cross(ab, bc);

                        Direction = cross.y > 0 ? 1 : 3;
                        distance = (int)Vector3.Distance(Route[0].transform.position, b) / 20;
                        isStraight = false;
                        // Debug.Log("Cross : " + cross.y + ", distance " + (int) Vector3.Distance(Route[0].transform.position, b) / 10);
                        break;
                    }
                }
                if (isStraight)
                {
                    Direction = 0;
                    distance = (int)Vector3.Distance(Route[0].transform.position, Route[Route.Count - 1].transform.position) / 20;
                }
            }
            else
            {
                Direction = 2;
                distance = 0;
            }
        }
        else
        {
            if (rightDot > 0) Direction = 1;
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


        Vector3 v = rb.velocity;
        //Velocity
        sensor.AddObservation(v.x / 18.0f);
        sensor.AddObservation(v.z / 18.0f);
        //speed
        float speed = v.magnitude / 18f;
        sensor.AddObservation(speed);
        //Position on board
        sensor.AddObservation(Route[0].GridX / 15.0f);
        sensor.AddObservation(Route[0].GridY / 15.0f);


        //distance to target
        sensor.AddObservation((targetX - Route[0].GridX) / 10.0f);
        sensor.AddObservation((targetY - Route[0].GridY) / 10.0f);

        //sensor.AddObservation(transform.position.x / 210f);
       // sensor.AddObservation(transform.position.z / 210f);
        sensor.AddObservation(LastDistanceToNextNode / 20f);


        if (Route.Count > 1)
        {
            //angle to node
            Vector3 dirToNode = (Route[1].transform.position - transform.position).normalized;
            sensor.AddObservation(dirToNode.x);
            sensor.AddObservation(dirToNode.z);
            //Next node position
            Vector3 localNodePos = transform.InverseTransformPoint(Route[1].transform.position);
            sensor.AddObservation(localNodePos.x / 20.0f);
            sensor.AddObservation(localNodePos.z / 20.0f);
        }
        if (Route.Count > 2)
        {
            //Future node position
            Vector3 futureNodePos = transform.InverseTransformPoint(Route[2].transform.position);
            sensor.AddObservation(futureNodePos.x / 20.0f);
            sensor.AddObservation(futureNodePos.z / 20.0f);
        }
       
        sensor.AddObservation(Mathf.Sin(transform.rotation.eulerAngles.y * Mathf.Deg2Rad));
        sensor.AddObservation(Mathf.Cos(transform.rotation.eulerAngles.y * Mathf.Deg2Rad));

        sensor.AddObservation(touchingWall);
        
        //distance for next instruction
        sensor.AddObservation(distance / 15.0f);
        //Direction
        AddOneHotEncoding(sensor, Direction + 1, 4);
        // Debug.Log("Current Observatios: Vx " + gameObject.GetComponent<Rigidbody>().velocity.x / 8.0f + ", Vy" + gameObject.GetComponent<Rigidbody>().velocity.z /8.0f + ", PosX: " + (Route[0].GridX / 15.0f) + " PosY: " + (((float)Route[0].GridY) / 15.0f) + " Distance: " + (distance / 15.0f));

        // sensor.AddObservation(FallenOff);


    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Vertical");
        continuousActionsOut[1] = Input.GetAxis("Horizontal");
    }


    public override void OnActionReceived(ActionBuffers actions)
    {
        //  Debug.Log("Got an action");
        var continuousActions = actions.ContinuousActions;
        GetComponent<CarControl>().UpdateValues(continuousActions[0], continuousActions[1]);

        //Calculate rewards

        //TODO MOVE CALCULATIONS TO SEPERATE FUNCTIONS

        if (AtTarget)
        {
            Debug.Log("Got to end");
            countTarget++;

            if (countTarget == 4)
            {
                targetNumber = targetNumber >= 19 ? 0 : targetNumber + 1;
                countTarget = 0;
            }
            AddReward(1f);
            rAtTarget += 1f;
            EndEpisode();
        }
        else if (this.transform.localPosition.y < 0.03)
        {
            AddReward(-16.0f / 35.0f);
            rFalling += -16.0f / 35.0f;
            EndEpisode();
        }
        if (maxSteps == 0)
        {
            maxSteps = Route.Count;
        }

        if (touchingWall)
        {
            AddReward(-0.001f);
            rTouchingWall += -0.001f;
        }
        if (TrainingWheels)
        {

            if (Route.Count > 1)
            {
                Vector3 directionToNode = (Route[1].transform.position - transform.position).normalized;
                float angleToNode = Vector3.SignedAngle(transform.forward, directionToNode, Vector3.up) / 180f;
                float reward = 1.0f - Mathf.Abs(angleToNode) / 180.0f; // Closer to 0° = more reward
                AddReward((reward - 0.994f) * 0.01f);
                lblTurnReward.text = "Current Angle Reward: " + ((reward - 0.994f) * 0.01f);
                rTurn += (reward - 0.994f) * 0.01f;
            }
            float distanceToNextNode = 0f;

            if (transform.position.x > Route[1].transform.position.x + 10)
            {
                //West
                distanceToNextNode = transform.position.x - (Route[1].transform.position.x + 10);
                ClosestPointOnNextNode = new Vector3(Route[1].transform.position.x + 10, 0, transform.position.z);

            }
            else if (transform.position.x < Route[1].transform.position.x - 10)
            {
                //To the East
                distanceToNextNode = (Route[1].transform.position.x - 10) - transform.position.x;
                ClosestPointOnNextNode = new Vector3(Route[1].transform.position.x - 10, 0, transform.position.z);

            }
            else if (transform.position.z > Route[1].transform.position.z + 10)
            {
                //Player is North
                distanceToNextNode = transform.position.z - (Route[1].transform.position.z + 10);
                ClosestPointOnNextNode = new Vector3(transform.position.x, 0, (Route[1].transform.position.z + 10));
            }
            else if (transform.position.z < Route[1].transform.position.z - 10)
            {
                //Player is South

                distanceToNextNode = (Route[1].transform.position.z - 10) - transform.position.z;
                ClosestPointOnNextNode = new Vector3(transform.position.x, 0, (Route[1].transform.position.z - 10));
            }

            if (LastDistanceToNextNode != 0)
            {
                float progressReward = ((LastDistanceToNextNode - distanceToNextNode) * Mathf.Exp(-timeAtCurrentNode * decayFactor)) / 80f;
                AddReward(progressReward);
                rTrainingWheels += progressReward;


            }
            // Update last distance
            LastDistanceToNextNode = distanceToNextNode;
        }
        if (lastInstruction == null) lastInstruction = new Tuple<int, int>(Direction, distance);
        else if (lastInstruction.Item1 != Direction || lastInstruction.Item2 != distance)
        {
            //     Debug.Log($"New instruction: Direction {Direction}, distance {distance}, maxStep {maxSteps}, Route Count {Route.Count}");


            //If the direction is straight or is left or right and decreases number of steps
            // Correct
            //If the direction is left or right and becomes left right or straight with same number of steps but increase number of direction
            // Correct
            //If direction is left or right or straight and changes to left right and straight and same number of steps but decreased distance 
            // Incorrect
            // If number of steps increase
            // Incorrect

            if (Route.Count < maxSteps)
            {
                //Correct
                //Done the correct instruction
                AddReward(0.13f);
                rTowardsNode += 0.2f;
                maxSteps = Route.Count;


            }
            else if (Route.Count == maxSteps)
            {

                if (lastInstruction.Item1 == 0 && (Direction == 1 || Direction == 3))
                {
                    //Incorrect
                    AddReward(-0.15f);
                    rAwayNode += -0.15f;
                    maxSteps = Route.Count;
                }
                else if ((lastInstruction.Item1 == 1 || lastInstruction.Item1 == 3) && (lastInstruction.Item2 > distance))
                {
                    //Incorrect
                    AddReward(-0.15f);
                    rAwayNode += -0.15f;
                    maxSteps = Route.Count;
                }
                else
                {
                    //Correct
                    //Done the correct instruction
                    AddReward(0.13f);
                    rTowardsNode += 0.2f;
                    maxSteps = Route.Count;
                }


            }
            else
            {
                //Incorrect
                AddReward(-0.15f);
                rAwayNode += -0.15f;
                maxSteps = Route.Count;
            }

            lastInstruction = new Tuple<int, int>(Direction, distance);

        }

        float totalReward = rTrainingWheels + rTowardsNode + rAwayNode + rTimeAtNode + rAtTarget + rFalling;
        //     Debug.Log($"Total Reward: {totalReward}, Step: {StepCount}");
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


        //Resets rewards counters
        rTowardsNode = 0f;
        rAwayNode = 0f;
        rFalling = 0f;
        rAtTarget = 0f;
        rCorrectNode = 0f;
        rIncorrectNode = 0f;
        rTimeAtNode = 0f;
        rTrainingWheels = 0f;
        rTurn = 0f;
        rTouchingWall = 0f;

        //Choose a random target
        //TODO put into a managers class;
        Node random = grid.GetRandomNode();
        //GameObject nextTarget = Targets[Random.Range(0, Targets.Length)];
        Transform nextTarget = TargetsObj.transform.GetChild(targetNumber);
        Target.transform.position = nextTarget.position;
        Node n = grid.GetNodeFromWorldPoint(Target.transform.position);
        targetX = n.GridX;
        targetY = n.GridY;
        // Target.transform.position = random.transform.position;
        //transform.position = new Vector3(0, 0.15f, 0);
        transform.position = new Vector3(Random.Range(-5, 5), 0.15F, Random.Range(-5, 5));
        transform.rotation = Quaternion.identity;
        //transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        rb.velocity = Vector3.zero;
        AtTarget = false;
        lastChecked = 0;
        CurrentNextNode = null;
        CurrentNode = null;
        lastInstruction = null;
        timeAtCurrentNode = 0;
        LastDistanceToNextNode = 0f;
        maxSteps = 0;
        GetPath();
    }


}
