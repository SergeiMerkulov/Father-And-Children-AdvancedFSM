using UnityEngine;
using UnityEngine.UI;
using System.Collections;

//Author: Sergei Merkulov
/// <summary>
///  children's controller.
///  creates all states and set transitions
///  containes all variables, references, and major functions run here (update)
/// </summary>


public class AIController : AdvancedFSM
{
    #region variables
    public Boss_AIController father;
    public GameObject playerPoint;

    [Space]
    public Transform[] waypoints;

    [Space]
    public bool debugDraw;
    public bool called; //have npc been called?
    public bool isSleeping; //is npc sleeping?
    public bool askedToPatrolByBoss; //boss lost player and asking to patrol area.

    [Header("UI stuff")]   
    public Text StateText;
    public Text patrolTimerText;
    public Slider healthSlider;
    private float backwardCounter;   

    [Space]
    public Transform shotSpawnTransform;
    public Rigidbody bullet;
    public float fireRate;
    public float launchForce;
    public GameObject explosion;
   
    [Space] //Sounds
    public AudioClip punch;
    public AudioClip explode;
    public AudioSource aSourse;
    public GameObject sleepingAnimation;

    [Space] //Set/Get/Decrement/Add to Health functions
    public int health;
    public int GetHealth() { return health; }
    public void SetHealth(int inHealth) { health = inHealth; }
    public void DecHealth(int amount) { Mathf.Max(0, health - amount); }
    public void AddHealth(int amount) { Mathf.Min(100, health + amount); }

    protected ParticleSystem explodePartciles;
    [HideInInspector]
    public Transform defWeaponPos;
    [HideInInspector]
    public bool resurrected; // to track if player ahs been resurrected
    [HideInInspector]
    public float patrolTimer;
    [HideInInspector]
    public bool backToSleepPoint; // to track if player has been back to the sleep point
    [HideInInspector]
    public bool playerHasAttacked;  // to track if player has attect current npc
    [HideInInspector]
    public Transform posOfCalledAlly; // use this transform to give position of called npc

    #endregion

    private string GetStateString()
    {
        string state = "NONE";
        if (CurrentState.ID == FSMStateID.Dead)
        {
            state = "Dead";
        }
        else if (CurrentState.ID == FSMStateID.Sleep)
        {
            state = "Sleep";
        }
        else if (CurrentState.ID == FSMStateID.RunToCallingAlly)
        {
            state = "Run To a Calling Ally";
        }
        else if (CurrentState.ID == FSMStateID.Chase)
        {
            state = "Chase";
        }
        else if (CurrentState.ID == FSMStateID.Attack)
        {
            state = "Attack";
        }
        else if (CurrentState.ID == FSMStateID.Patrol)
        {
            state = "Patrol";
        }
        else if (CurrentState.ID == FSMStateID.BackToSleepPoint)
        {
            state = "Back to a Sleep Point";
        }
        else if (CurrentState.ID == FSMStateID.PatrolByChildren)
        {
            state = "Looking for player";
        }
        if (CurrentState.ID == FSMStateID.Resurrection)
        {
            state = "Resurrection";
        }

        return state;
    }

    protected override void Initialize()
    {        
        playerTransform = playerPoint.transform;
        SetHealth(100);
        healthSlider.value = GetHealth();
        called = false;
        isSleeping = true;
        askedToPatrolByBoss = false;
        resurrected = false;
        fireRate = 0.5f;
        backToSleepPoint = false;
        playerHasAttacked = false;      
        ConstructFSM();
        explodePartciles = explosion.GetComponent<ParticleSystem>();

        if (waypoints.Length == 0)
        {
            pointList = GameObject.FindGameObjectsWithTag("WayPoint");

            //Creating a waypoint transform array for each state       
            waypoints = new Transform[pointList.Length];
            int i = 0;
            foreach (GameObject obj in pointList)
            {
                waypoints[i] = obj.transform;
                i++;
            }
        }
    }

    protected override void FSMUpdate()
    {        
        CurrentState.Reason(playerTransform, transform);

        //if current state is not "run to calling ally" just send player's transform and current npc        
        if(CurrentState.ID != FSMStateID.RunToCallingAlly)
        {
            CurrentState.Act(playerPoint.transform, transform);
        }
        else //otherwise send position of calling ally and npc's
        {
            CurrentState.Act(posOfCalledAlly, transform);           
        }

        //update health bar
        healthSlider.value = GetHealth();

        //if patrol state is active, run timer
        elapsedTime += Time.deltaTime;

        if (CurrentState.ID == FSMStateID.Patrol)
        {
            UpdatePatrolTimer(Time.deltaTime);
        }

        //display current state on the screen
        StateText.text = "AI STATE IS: " + GetStateString();

        if (debugDraw)
        {
            UsefullFunctions.DebugRay(transform.position, transform.forward * 5.0f, Color.red); 
        }
    }


    private void ConstructFSM()
    {              
        //Create States        
        
        //Dead state
        DeadState_AI01 dead = new DeadState_AI01();
        //transition
        dead.AddTransition(Transition.ResurrectedByBoss, FSMStateID.Chase);

        //Sleep state
        SleepState_AI01 sleep = new SleepState_AI01();
        //transitions
        sleep.AddTransition(Transition.SeePlayer, FSMStateID.Attack);
        sleep.AddTransition(Transition.Attacked, FSMStateID.Chase);
        sleep.AddTransition(Transition.Called, FSMStateID.RunToCallingAlly);
        sleep.AddTransition(Transition.AskedToPatrolByBoss, FSMStateID.Patrol);
        sleep.AddTransition(Transition.NoHealth, FSMStateID.Dead);


        //Run to a calling Ally state
        RunToAllyState_AI01 runToAlly = new RunToAllyState_AI01();
        //transitions
        runToAlly.AddTransition(Transition.SeePlayer, FSMStateID.Chase);
        runToAlly.AddTransition(Transition.ReachedAllyPos, FSMStateID.Patrol);
        runToAlly.AddTransition(Transition.NoHealth, FSMStateID.Dead);


        //Chase state
        ChaseState_AI01 chase = new ChaseState_AI01(waypoints);
        //transitions
        chase.AddTransition(Transition.ReachedPlayer, FSMStateID.Attack);
        chase.AddTransition(Transition.LostPlayer, FSMStateID.Patrol);
        chase.AddTransition(Transition.NoHealth, FSMStateID.Dead);


        //Attack state
        AttackState_AI01 attack = new AttackState_AI01();
        //transitions
        attack.AddTransition(Transition.OutOfAttackRange, FSMStateID.Chase);
        attack.AddTransition(Transition.NoHealth, FSMStateID.Dead);

        //Patrol state
        PatrolState_AI01 patrol = new PatrolState_AI01(waypoints);
        //transitions
        patrol.AddTransition(Transition.SeePlayer, FSMStateID.Chase);
        patrol.AddTransition(Transition.NotFoundForLongTime, FSMStateID.BackToSleepPoint);
        patrol.AddTransition(Transition.NoHealth, FSMStateID.Dead);


        //BackToSleepPoint state
        BackToSleepPointState_AI01 backToSleepPoint = new BackToSleepPointState_AI01(waypoints);
        //transitions
        backToSleepPoint.AddTransition(Transition.ReachedSleepPoint, FSMStateID.Sleep);
        backToSleepPoint.AddTransition(Transition.SeePlayer, FSMStateID.Chase);
        backToSleepPoint.AddTransition(Transition.Attacked, FSMStateID.Patrol);
        backToSleepPoint.AddTransition(Transition.NoHealth, FSMStateID.Dead);

        //Add state to the state list
        AddFSMState(sleep);
        AddFSMState(runToAlly);
        AddFSMState(chase);
        AddFSMState(attack);
        AddFSMState(patrol);
        AddFSMState(backToSleepPoint);
        AddFSMState(dead);
    }


    //Patrol Timer
    public bool PatrolTimer( float delayTime)
    {
        bool timeUp = false;
        backwardCounter = delayTime;
        if (patrolTimer >= delayTime)
        {
            patrolTimer = 0.0f;           
            timeUp = true;
        }
        return timeUp;
    }

    void UpdatePatrolTimer(float deltaTime)
    {
        patrolTimer += deltaTime;
        backwardCounter = backwardCounter - patrolTimer;
        //update timer text on the screen
        patrolTimerText.text = backwardCounter.ToString("0.0") + " s";
    }

    public void InitPatrolTimer()
    {
        patrolTimer = 0;
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Reduce health
        if (collision.transform.tag == "PlayerBullet")
        {          
            health -= 15;
            playerHasAttacked = true;
            Destroy(collision.gameObject);
            if (GetHealth() <= 0)
            {
                SetHealth(0);
                Debug.Log("Switch to Dead State");               
            }            
        }
    }

    //move weapon of enemy forward then back
    public void HitPLayer()
    {           
        // Create an instance of the shell and store a reference to it's rigidbody.
        Rigidbody shellInstance = Instantiate(bullet, shotSpawnTransform.position, shotSpawnTransform.rotation) as Rigidbody;

        // Set the shell's velocity to the launch force in the fire position's forward direction.
        shellInstance.velocity = launchForce * shotSpawnTransform.forward;
        aSourse.PlayOneShot(punch, 1.0f);
    }

    //play particles
    public void PlayParticles()
    {
        explodePartciles.Play();  
    }
}
