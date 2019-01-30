using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

//Authr: Sergei Merkulov
/// <summary>
///  (Father)Boss's controller.
///  creates all states and set transitions
///  containes all variables, references, and major functions run here (update)
/// </summary>
/// 

public class Boss_AIController : AdvancedFSM
{
    #region variables

    public Transform player;
    [Space]
    public Transform[] waypoints;

   [Space]
    public bool debugDraw;
    public bool isSleeping; //is npc sleeping?
    
    [Space]
    [Header("UI stuff")]
    public Text StateText;
    public Slider resurrectionSlider; // to show resurrection cast
    public Slider healthSlider; // to display health
    public Text timerText; //for resurrection
    public Text timerPatrolText; // for timer in patrol state
    public GameObject ressurectionBoard; // game object which hold info for Resurrection state
    private float backwardCounter;

    public GameObject fist;
    public GameObject bullet;

    [Space]
    public GameObject sleepingAnimation;

    [Space]
    public List<AIController> children; // list of small npcs

    [Space] //Set/Get/Decrement/Add to Health functions
    public int health;
    public int GetHealth() { return health; }
    public void SetHealth(int inHealth) { health = inHealth; }
    public void DecHealth(int amount) { Mathf.Max(0, health - amount); }
    public void AddHealth(int amount) { Mathf.Min(100, health + amount); }

    [HideInInspector]
    public float timer;
    [HideInInspector]
    public bool childrenFoundPlayer; //to track if children found player during patroling  state
    [HideInInspector]
    public bool playerHasAttacked;
    [HideInInspector]
    public bool backToSleepPoint;
    [HideInInspector]
    public float patrolTimer;
    [HideInInspector]
    public bool didntRessurectYet;

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
        else if (CurrentState.ID == FSMStateID.Chase)
        {
            state = "Chase";
        }
        else if (CurrentState.ID == FSMStateID.Attack)
        {
            state = "Attack";
        }
        else if (CurrentState.ID == FSMStateID.PatrolByChildren)
        {
            state = "Patrol by children";
        }
        else if (CurrentState.ID == FSMStateID.BackToSleepPoint)
        {
            state = "Back to a Sleep Point";
        }
        else if (CurrentState.ID == FSMStateID.Resurrection)
        {
            state = "Resurrection";
        }

        return state;
    }

    protected override void Initialize()
    {       
        playerTransform = player;
        health = 100;
        isSleeping = true;        
        resurrectionSlider.value = 0;
        ressurectionBoard.SetActive(false);
        childrenFoundPlayer = false;
        playerHasAttacked = false;
        backToSleepPoint = false;
        fist.SetActive(false);
        didntRessurectYet = true;
        shootRate = 2.0f;       
        bulletSpawnPoint = transform;
        GetComponent<MeshRenderer>().material.color = Color.blue;
        ConstructFSM();

        if(waypoints.Length == 0)
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
        CurrentState.Act(playerTransform, transform);       
               
        //if resurrection state is active, run timer
        if (CurrentState.ID == FSMStateID.Resurrection)
        {
            UpdateTimer(Time.deltaTime);
        }

        //if PatrolByChildren state is active, run timer
        elapsedTime += Time.deltaTime;
        if (CurrentState.ID == FSMStateID.PatrolByChildren)
        {
            UpdateTimer(Time.deltaTime);            
        }

        //UI update
        //update state text
        StateText.text = "AI STATE IS: " + GetStateString();

        //update resurrection casting bar
        resurrectionSlider.value = timer;

        //update health bar
        healthSlider.value = health;

        if (debugDraw)
        {
            UsefullFunctions.DebugRay(transform.position, transform.forward * 5.0f, Color.red);
        }
    }


    private void ConstructFSM()
    {        
        //Create States 
        
        //Dead state
        DeadState dead = new DeadState();
        //there are no transitions out of the dead state

        //Create the sleep state
        SleepState_BOSS sleepBoss = new SleepState_BOSS();
        //transitions
        sleepBoss.AddTransition(Transition.Attacked, FSMStateID.Attack);
        sleepBoss.AddTransition(Transition.ChildrenKilled_SeePlayer, FSMStateID.Chase);
        sleepBoss.AddTransition(Transition.NoHealth, FSMStateID.Dead);


        //Chase state
        Chase_BOSS chaseBoss = new Chase_BOSS(waypoints);
        //transitions
        chaseBoss.AddTransition(Transition.ReachedPlayer, FSMStateID.Attack);
        chaseBoss.AddTransition(Transition.ChildrenAlive_LostPlayer, FSMStateID.PatrolByChildren);
        chaseBoss.AddTransition(Transition.ChildrenKilled_LostPlayer, FSMStateID.Resurrection);
        chaseBoss.AddTransition(Transition.NoHealth, FSMStateID.Dead);


        //Attack state
        AttackState_BOSS attackBoss = new AttackState_BOSS();
        //transitions
        attackBoss.AddTransition(Transition.ChildrenKilled_OutOfAttackRange, FSMStateID.Resurrection);
        attackBoss.AddTransition(Transition.OutOfAttackRange, FSMStateID.Chase);
        attackBoss.AddTransition(Transition.NoHealth, FSMStateID.Dead);

        //Resurrection of children state
        Resurrection_BOSS resurrection = new Resurrection_BOSS();
        //transitions
        resurrection.AddTransition(Transition.ChildrenAlive_SeePlayer, FSMStateID.Attack);
        resurrection.AddTransition(Transition.ChildrenAlive_LostPlayer, FSMStateID.PatrolByChildren);
        resurrection.AddTransition(Transition.NoHealth, FSMStateID.Dead);


        //Patrol by children state
        PatrolByChildrenState_BOSS patrolByChildren = new PatrolByChildrenState_BOSS();
        //transitions
        patrolByChildren.AddTransition(Transition.SeePlayer, FSMStateID.Chase);
        patrolByChildren.AddTransition(Transition.NotFoundForLongTime, FSMStateID.BackToSleepPoint);
        patrolByChildren.AddTransition(Transition.NoHealth, FSMStateID.Dead);


        //BackToSleepPoint state
        BackToSleepState_BOSS backToSleepPointBoss = new BackToSleepState_BOSS(waypoints);
        //transitions
        backToSleepPointBoss.AddTransition(Transition.ReachedSleepPoint, FSMStateID.Sleep);
        backToSleepPointBoss.AddTransition(Transition.SeePlayer, FSMStateID.Chase);
        backToSleepPointBoss.AddTransition(Transition.NoHealth, FSMStateID.Dead);


        //Add state to the state list
        AddFSMState(sleepBoss);
        AddFSMState(chaseBoss);
        AddFSMState(attackBoss);
        AddFSMState(resurrection); 
        AddFSMState(patrolByChildren);
        AddFSMState(backToSleepPointBoss);
        AddFSMState(dead);

    }

    private void OnCollisionEnter(Collision collision)
    {
        //Reduce health
        if (collision.gameObject.tag == "PlayerBullet")
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

    //----------------------------------------------------------
    //Timer
    public bool PatrolTimer(float delayTime)
    {
        bool timeUp = false;
        backwardCounter = delayTime;
        if (timer >= delayTime)
        {
            timer = 0.0f;
            timeUp = true;
        }
        return timeUp;
    }

    void UpdateTimer(float deltaTime)
    {
        timer += deltaTime;

        //update timer text on the screen when resurrection state is active
        if (CurrentState.ID == FSMStateID.PatrolByChildren)
        {            
            backwardCounter = backwardCounter - timer;
            //update timer text on the screen
            timerPatrolText.text = backwardCounter.ToString("0.0") + " s";     
        }
        //update patrol timer text on the screen when patrol by children state is active
        else
        {
            timerText.text = timer.ToString("0.0") + " s";
        }
    }
    
    //reset timer
    public void InitTimer()
    {
        timer = 0;
    }


    //------------------------------------------------------------
    //Check all children are alive or not
    public bool ChildrenAreKilled()
    {        
        //check if all of children are killed
        foreach (AIController g in children)
        {
            if (g.GetHealth() > 0)
            {
                return false;
            }                        
        }

        return true;     
    }

    //move weapon of enemy forward then back
    public void ActivateMeleeWeapon()
    {
        fist.SetActive(true);
    }

    protected void Explode()
    {
        float rndX = Random.Range(10.0f, 30.0f);
        float rndZ = Random.Range(10.0f, 30.0f);
        for (int i = 0; i < 3; i++)
        {
            GetComponent<Rigidbody>().AddExplosionForce(10000.0f, transform.position - new Vector3(rndX, 10.0f, rndZ), 40.0f, 10.0f);
            GetComponent<Rigidbody>().velocity = transform.TransformDirection(new Vector3(rndX, 20.0f, rndZ));
        }

        Destroy(gameObject, 1.5f);
    }


    // Shoot the bullet from the boss
    public void ShootBullet()
    {
        if (elapsedTime >= shootRate)
        {
            Instantiate(bullet, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            elapsedTime = 0.0f;
        }
    }
} 