using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class Player : Agent, Icreature
{
    [SerializeField]
    protected int maxHealth;
    protected int currentHealth;

    protected bool moving = false;
    protected bool canStep = true; //can the entity play a step sound?

    [SerializeField]
    public GameManager gameManager;
    protected int currentScore;

    protected bool canShoot = true;

    //Weapons
    protected enum Weapon
    {
        None,
        LaserGun,
    }
    protected Weapon currentEquipedWeapon = Weapon.None;

    [SerializeField]
    protected Weapon StartingWeapon = Weapon.None;

    [SerializeField]
    protected GameObject laserProjectile;


    [Header("Movement")]
    [SerializeField, Range(3f, 10.0f)]
    private float movementSpeed = 5;

    [SerializeField, Min(0.01f)]
    private float movSmoothLerp = 0.03f;

    [SerializeField, Range(0.1f, 10.0f)]
    private float lookSensivity = 2.5f;

    private Vector3 direction;

    private Rigidbody myRigidbody;

    private Vector2 movementInput = new Vector2(0f, 0f);
    private Quaternion[] lookInput;

    private bool shooting;
    [SerializeField]
    private Transform shootingPoint;

    //Mouse stuff
    float sensitivity = 10F;
    float rotationX = 0F;
    float rotationY = 0F;
    float rotArrayX;
    float rotAverageX = 0F;
    float rotArrayY;
    float rotAverageY = 0F;
    Quaternion[] originalRotation;

    [SerializeField]
    Transform playerCamera;

    Vector3 lastEnemyHeardPosition = new Vector3(999, 999, 999);
    float lastEnemyRotation = 9999;
    Vector3 lastEnemySeenPosition = new Vector3(999, 999, 999);

    Vector3 nearestHealthKit = new Vector3(999, 999, 999);

    bool HitEnemy = false;

    [SerializeField]
    Transform StorageSpace;

    public int score { get; private set; } //agent's current score this episode
    int ConsecutiveWinsThisPhase = 0;

    [SerializeField]
    int ManualMaxStep = 1000;

    private void Awake()
    {
        score = 0;
    }

    // Start is called before the first frame update
    void Start()
    {
        myRigidbody = GetComponent<Rigidbody>();

        currentEquipedWeapon = StartingWeapon;

        originalRotation = new Quaternion[2];

        originalRotation[0] = playerCamera.localRotation;
        originalRotation[1] = transform.localRotation;


        lookInput = new Quaternion[2];

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

    }

    public override void Initialize()
    {
        MaxStep = 0;

        if (!Academy.Instance.IsCommunicatorOn)
        {
            this.MaxStep = 0;
        }
    }

    public override void OnEpisodeBegin()
    {
        //base.OnEpisodeBegin();

        Debug.Log("Episode Start");

        score = 0;

        SetMaxStep();

        gameManager.StartMatch();        

    }

    // Update is called once per frame
    void Update()
    {

        //if (Input.GetKeyDown(KeyCode.L))
        //{
        //    currentHealth -= 50;
        //    Debug.Log($"health is now {currentHealth}");
        //}
    }

    void FixedUpdate()
    {
        TransmitLocation();

        movementInput = Vector2.ClampMagnitude(movementInput, 1);

        float dirY = myRigidbody.velocity.y;

        direction = transform.right * movementInput.x + transform.forward * movementInput.y;

        direction *= movementSpeed;
        direction.y = dirY;

        myRigidbody.velocity = direction;

        if (myRigidbody.velocity.magnitude > 0.01f) moving = true;
        else moving = false;

        //transform.Rotate(new Vector3(0, lookSensivity * lookInput,0), Space.Self);

        if (shooting)
        {
            shootWeapon(shootingPoint);
        }


        //Rotate
        transform.localRotation = originalRotation[1] * lookInput[1];
        playerCamera.localRotation = originalRotation[0] * lookInput[0];

        if (moving)
        {
            if (canStep)
            {
                PlaySound();
                StartCoroutine(StepDelay());
            }
        }

        ResetInput();

        if (StepCount > ManualMaxStep) //manually end episode
        {
            EndEpisodeInFailure();
        }
    }

    void LateUpdate()
    {

    }

    // Generates the simulated output to the neural network
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        ActionSegment<float> inputs = actionsOut.ContinuousActions;
        ActionSegment<int> shootingAction = actionsOut.DiscreteActions;
        inputs[0] = horizontalInput;
        inputs[1] = verticalInput;

        float mouseY = Input.GetAxis("Mouse Y");
        float mouseX = Input.GetAxis("Mouse X");

        inputs[2] = mouseY;
        inputs[3] = mouseX;

        shootingAction[0] = 0;
        if (Input.GetButton("Fire1")) {
            
            //Debug.Log("Shooting");
            shootingAction[0] = 1;
        }
    }

    // process output of the neural network
    public override void OnActionReceived(ActionBuffers actions)
    {
        // obtain actions
        ActionSegment<float> axis = actions.ContinuousActions;
        ActionSegment<int> shootingAction = actions.DiscreteActions;
        float HorizontalInput = axis[0];
        float VerticalInput = axis[1];

        lookInput = MouseInput(axis[2], axis[3]);

        movementInput = new Vector2(HorizontalInput, VerticalInput);

        if (System.Convert.ToBoolean(shootingAction[0]))
        {            
            if (canShoot) shooting = true;
        }
    }

    //input to the neural network
    public override void CollectObservations(VectorSensor sensor)
    {

        //3 float for position
        sensor.AddObservation(transform.localPosition.x);
        sensor.AddObservation(transform.localPosition.y);
        sensor.AddObservation(transform.localPosition.z);

        //1 float for rotation of agent
        sensor.AddObservation(transform.eulerAngles.y);

        //3 floats for last seen enemy position
        sensor.AddObservation(lastEnemySeenPosition.x);
        sensor.AddObservation(lastEnemySeenPosition.y);
        sensor.AddObservation(lastEnemySeenPosition.z);
        
        //1 floats for rotation in degrees of last seen enemy
        sensor.AddObservation(lastEnemyRotation);

        //3 floats for last heard sound position
        sensor.AddObservation(lastEnemyHeardPosition.x);
        sensor.AddObservation(lastEnemyHeardPosition.y);
        sensor.AddObservation(lastEnemyHeardPosition.z);

        //1 float for weapon firing readiness
        sensor.AddObservation(canShoot);

        //1 float for enemy was hit last frame
        sensor.AddObservation(HitEnemy);
        HitEnemy = false;

        //1 float for current health diference to max health
        sensor.AddObservation(maxHealth - currentHealth);

        //3 float for nearest health pickup position
        sensor.AddObservation(nearestHealthKit.x);
        sensor.AddObservation(nearestHealthKit.y);
        sensor.AddObservation(nearestHealthKit.z);

        //total 3 + 1 + 3 + 1 + 3 + 1 + 1 + 1 + 3 = 17
    }

    private void ResetInput()
    {
        shooting = false;
    }

    private Quaternion[] MouseInput(float YInput, float XInput)
    {
        //Resets the average rotation
        rotAverageY = 0f;
        rotAverageX = 0f;

        //Gets rotational input from the mouse
        rotationY += (YInput * sensitivity) * 100 * Time.deltaTime;
        rotationX += (XInput * sensitivity) * 100 * Time.deltaTime;

        rotationY = Mathf.Clamp(rotationY, -90, 90);

        //Adds the rotation values to their relative array
        rotArrayY = rotationY;
        rotArrayX = rotationX;

        //Adding up all the rotational input values from each array
        rotAverageY += rotArrayY;
        rotAverageX += rotArrayX;

        //Get the rotation you will be at next as a Quaternion
        Quaternion yQuaternion = Quaternion.AngleAxis(rotAverageY, Vector3.left);
        Quaternion xQuaternion = Quaternion.AngleAxis(rotAverageX, Vector3.up);

        Quaternion[] lookInput = new Quaternion[2];
        lookInput[0] = yQuaternion;
        lookInput[1] = xQuaternion;

        return lookInput;
    }

    public void RotateToRandom()
    {
        //var euler = transform.eulerAngles;
        float x = Random.Range(0.0f, 360.0f);
        //Debug.Log($"Rotating player {x} degrees");
        //this.transform.eulerAngles = euler;

        Quaternion xQuaternion = Quaternion.AngleAxis(x, Vector3.up);

        transform.localRotation = originalRotation[1] * xQuaternion;
        originalRotation[1] = transform.localRotation;

        //gameObject.transform.Rotate(gameObject.transform.up, x);
    }

    private IEnumerator ShootingDelay()
    {
        canShoot = false;
        yield return new WaitForSeconds(0.5f);
        if (!canShoot) canShoot = true;
    }
    private IEnumerator StepDelay()
    {
        canStep = false;
        yield return new WaitForSeconds(1f);
        if (!canStep) canStep = true;
    }

    protected void shootWeapon(Transform shootingPoint)
    {
        switch (currentEquipedWeapon)
        {
            case Weapon.None:
                break;
            case Weapon.LaserGun:
                ShootLaserGun(shootingPoint);
                StartCoroutine(ShootingDelay());
                break;
            default:
                break;
        }
    }

    private void ShootLaserGun(Transform shootingPoint)
    {
        GameObject o = Instantiate(laserProjectile, shootingPoint.position, Quaternion.identity);
        LaserProjectile l = o.GetComponent<LaserProjectile>();
        PlaySound();
        l.direction = shootingPoint.forward;
        l.shooter = this;
    }


    public void receiveSound(SoundInfo sound)
    {
        //Handle sound receiving

        //Debug.Log($"{gameObject.name}: heard sound from {sound.pos} by {sound.emitter.gameObject.name}");
        lastEnemyHeardPosition = sound.pos;

    }

    /// <summary>
    /// Transmit visual location to opponent
    /// </summary>
    protected void TransmitLocation()
    {
        Vector3 location = this.transform.position;
        float rotation = this.transform.eulerAngles.y;
        Icreature entity = this;

        VisionInfo info = new VisionInfo(location, rotation, entity);

        gameManager.TransmitLocation(info);
    }

    public void ReceiveLocation(VisionInfo info)
    {
        Vector3 direction = info.pos - this.transform.position;
        float horizontalAngle = Vector3.Angle(direction, this.transform.forward);
        float verticalAngle = Vector3.Angle(direction, playerCamera.forward);

        if (horizontalAngle < 45 && verticalAngle < 35) //enemy is within the agent's field of vision
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.localPosition, direction, out hit, 1000))
            {
                //Debug.DrawRay(transform.localPosition, direction * hit.distance, Color.red);

                if (hit.collider.gameObject.CompareTag("Player")) //Agent can see the Enemy
                {
                    Debug.DrawLine(transform.localPosition, hit.point, Color.red);
                    lastEnemySeenPosition = info.pos;
                    lastEnemyRotation = info.degreeRotation;

                    if (horizontalAngle < 15f && verticalAngle < 10f) //agent has enemy in target sight
                    {
                        Debug.DrawLine(transform.localPosition, hit.point, Color.red);
                        AddReward(0.0006f);
                    }
                    else {
                        Debug.DrawLine(transform.localPosition, hit.point, Color.yellow);
                        AddReward(0.0003f);
                    }

                }
                else //enemy is blocked by something
                {
                    Debug.DrawLine(transform.localPosition, hit.point, Color.green);
                }
            }
            else
            {
                Debug.DrawRay(transform.localPosition, direction * 1000, Color.blue);
            }
        }
    }

    public void Respawn(Vector3 location, Quaternion rotation)
    {
        //gameObject.SetActive(true);
        transform.position = location;
        transform.rotation = rotation;

        currentHealth = maxHealth;
        canShoot = true;
        canStep = true;

        lastEnemyHeardPosition = new Vector3(999, 999, 999);
        lastEnemyRotation = 9999;
        lastEnemySeenPosition = new Vector3(999, 999, 999);
        nearestHealthKit = new Vector3(999, 999, 999);

        //Debug.Log($"{gameObject.name}: health is now {currentHealth}");

        PlaySound();

        lastEnemyHeardPosition = Vector3.zero;
        lastEnemySeenPosition = Vector3.zero;
    }

    protected void PlaySound()
    {
        Vector3 position = this.transform.position;
        Icreature e = this;

        SoundInfo info = new SoundInfo(position, e);

        gameManager.TransmitSound(info);
    }

    public bool ReceiveHealth(int ammount)
    {
        if (currentHealth < maxHealth) //check if current health is not maxed or overcharged
        {
            if (currentHealth < maxHealth / 2)
            {
                AddReward(0.05f); //Reward agent for picking up health while it is less than half. 
            }

            currentHealth += ammount; //fill health

            if (currentHealth > maxHealth) currentHealth = maxHealth; //don't let health overflow over the max 
            Debug.Log($"health is now {currentHealth}");
            

            return true;
        }
        return false;
    }

    public void SetToSleep()
    {
       // gameObject.SetActive(false); 
    }

    public bool ReceiveDamage(int ammount)
    {
        currentHealth -= ammount;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            gameObject.SetActive(false);
            return true;
        }
        else
        {
            return false;
        }
    }

    public void SetGameManager(GameManager manager)
    {
        this.gameManager = manager;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public void SetHitStatus(HitInfo info)
    {
        HitEnemy = info.Hit;        

        if (info.Hit)
        {
            AddReward(0.01f); //reward for hitting enemy

            if (info.Destroy)
            {
                AddReward(0.2f); //Reward for destroying enemy
                score++;                

                if (score == gameManager.maxScore) //if agent reaches max score, end episode
                {
                    EndEpisodeInSuccess();
                }
                else //if max has not been reaches, respawn opponent, continue episode
                {
                    gameManager.ContinueEpisode();
                }
            }
        }
        else
        {
            AddReward(-0.0008f); //penalty for missing enemy
        }
    }

    public void Store()
    {
        //do nothing
    }

    private void EndEpisodeInSuccess()
    {
        score = 0;

        Debug.Log("ending episode in sucess");
        AddReward(1);

        ConsecutiveWinsThisPhase++;
        if (ConsecutiveWinsThisPhase == 5)
        {
            ConsecutiveWinsThisPhase = 0;
            EnvironmentManager.inst.MoveToNextPhase();
        }

        //end episode
        gameManager.EndEpisode();
        EndEpisode(); //end current episode
    }

    private void EndEpisodeInFailure()
    {
        score = 0;

        gameManager.EndEpisode();

        ConsecutiveWinsThisPhase = 0;

        EndEpisode();
    }

    private void SetMaxStep()
    {
        switch (EnvironmentManager.inst.currentPhase)
        {
            case EnvironmentManager.CurriculumPhase.DestroyImmobileTarget1:
                this.ManualMaxStep = 500;
                break;
            case EnvironmentManager.CurriculumPhase.DestroyImmobileTarget2:
                this.ManualMaxStep = 800;
                break;
            case EnvironmentManager.CurriculumPhase.DestroyImmobileTarget3:
                this.ManualMaxStep = 1300;
                break;
            case EnvironmentManager.CurriculumPhase.DestroyImmobileTarget4:
                this.ManualMaxStep = 25000;
                break;
            case EnvironmentManager.CurriculumPhase.DestroyMobileTarget:
                this.ManualMaxStep = 3000;
                break;
            case EnvironmentManager.CurriculumPhase.BattleSelf:
                this.ManualMaxStep = 50000;
                break;
            default:
                break;
        }
    }
}
