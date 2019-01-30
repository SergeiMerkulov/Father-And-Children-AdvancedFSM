using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PCControl : MonoBehaviour
{	
    // consts for range of bounds 
	public const float MIN_X= -70;
	public const float MAX_X= 70;
	public const float MIN_Z= -70;
	public const float MAX_Z= 70;

    public float health;
    public Slider healthSlider;
    public float speed;
    public float turnSpeed;    
    [Space]
    public Transform shotSpawnTransform;
    public Rigidbody bullet;
    public float launchForce;
    [Space]
    public AudioClip punch;
    public AudioSource aSourse;

    private Rigidbody rb;   
    private Vector3 movement;  // The vector to store the direction of the player's movement.
    private float moveHorizontal;
    private float moveVertical;
    

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start ()
    {
        speed = 20f;
        turnSpeed = 90f;
        health = 100f;        
        healthSlider.value = health;        
    }

    private void OnEnable()
    {
        // When the tank is turned on, make sure it's not kinematic.
        rb.isKinematic = false;

        // Also reset the input values.
        moveHorizontal = 0f;
        moveVertical = 0f;
    }


    private void OnDisable()
    {
        // When the tank is turned off, set it to kinematic so it stops moving.
        rb.isKinematic = true;
    }


    void Update ()
    {
        moveHorizontal = Input.GetAxis("Horizontal");
        moveVertical = Input.GetAxis("Vertical");

        if (Input.GetMouseButtonDown(0))
        {            
            Fire();
        }
       
        UsefullFunctions.DebugRay(transform.position, transform.forward * 5.0f, Color.red);
    }


    private void FixedUpdate()
    {          
        // Move the player around the scene.
        Move();
        Turn();

        healthSlider.value = health;

        //Check Bounds
        rb.position = new Vector3
        (
            Mathf.Clamp(rb.position.x, MIN_X, MAX_X),
            1.5f,
            Mathf.Clamp(rb.position.z, MIN_Z, MAX_Z)
        );

    }    
    
    void Move()
    {
        // Create a vector in the direction the tank is facing with a magnitude based on the input, speed and the time between frames.
       movement = transform.forward * moveVertical * speed * Time.deltaTime;

        // Move the player to it's current position plus the movement.
        rb.MovePosition(rb.position + movement);
    }

    private void Turn()
    {
        // Determine the number of degrees to be turned based on the input, speed and time between frames.
        float turn = moveHorizontal * turnSpeed * Time.deltaTime;

        // Make this into a rotation in the y axis.
        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);

        // Apply this rotation to the rigidbody's rotation.
        rb.MoveRotation(rb.rotation * turnRotation);
    }


    private void Fire()
    {       
        // Create an instance of the shell and store a reference to it's rigidbody.
        Rigidbody shellInstance = Instantiate(bullet, shotSpawnTransform.position, shotSpawnTransform.rotation) as Rigidbody;

        // Set the shell's velocity to the launch force in the fire position's forward direction.
        shellInstance.velocity = launchForce * shotSpawnTransform.forward;
        aSourse.PlayOneShot(punch, 1.0f);
    }


    private void OnCollisionEnter(Collision collision)
    {
        //Reduce health
        if (collision.transform.tag == "Bullet")
        {
            health -= 2;            
            Destroy(collision.gameObject);                        
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        //Reduce health
        if (collision.gameObject.tag == "Fist")
        {
            health -= 0.5f;
        }
    }

}
