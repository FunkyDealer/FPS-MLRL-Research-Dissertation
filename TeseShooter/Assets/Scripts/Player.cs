using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class Player : Agent, Icreature
{
    // mlagents-learn --run-id=ShooterAgent<id> ConfigFiles/ShootingConfig.yaml

    [SerializeField]
    protected int maxHealth;
    protected int currentHealth;

    protected bool moving = false;
    protected bool canStep = true; //can the entity play a step sound?

    GameManager gameManager;
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
    float sensitivity = 1F;
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
    Transform storageSpace;

    public int score { get; private set; } //agent's current score this episode
    int ConsecutiveWinsThisPhase = 0;

    [SerializeField]
    int ManualMaxStep = 1000;

    [SerializeField]
    bool clone = false;

    int sucessfullGames = 0;
    

    private void Awake()
    {
        gameManager = GetComponentInParent<GameManager>();
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

    public override void OnEpisodeBegin()
    {
        score = 0;

        SetMaxStep();

        if (!clone)
        {            
            gameManager.StartMatch();
        }

    }

    // Update is called once per frame
    void Update()
    {


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
                TransmitSound();
                StartCoroutine(StepDelay());
            }
        }

        ResetInput();

        if (StepCount > ManualMaxStep) //manually end episode
        {
           if (!clone) EndEpisodeInFailure();
        }

        AddReward(-0.003f); //Time penalty
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

        if (Input.GetKeyDown(KeyCode.L))
        {
            currentHealth -= 50;
            //Debug.Log($"health is now {currentHealth}");
        }

        // inputs[2] = mouseY;
        inputs[2] = mouseX;

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

        lookInput = MouseInput(0, axis[2]);

        movementInput = new Vector2(HorizontalInput, VerticalInput);

        if (System.Convert.ToBoolean(shootingAction[0]))
        {            
            if (canShoot) shooting = true;
        }
    }

    //input to the neural network
    public override void CollectObservations(VectorSensor sensor)
    {
        //3 floats for position
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
        //sensor.AddObservation(lastEnemyRotation);

        //3 floats for last heard sound position
        sensor.AddObservation(lastEnemyHeardPosition.x);
        sensor.AddObservation(lastEnemyHeardPosition.y);
        sensor.AddObservation(lastEnemyHeardPosition.z);

        //1 float for weapon firing readiness
        sensor.AddObservation(canShoot);

        //1 float for enemy was hit last frame
        //sensor.AddObservation(HitEnemy);
       // HitEnemy = false;

        //1 float for current health diference to max health
        sensor.AddObservation(maxHealth - currentHealth);

        //3 float for nearest health pickup position
        //sensor.AddObservation(nearestHealthKit.x);
        //sensor.AddObservation(nearestHealthKit.y);
        //sensor.AddObservation(nearestHealthKit.z);

        //total 3 + 1 + 3 + 3 + 1 + 1 = 12
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
        //if (gameManager.currentPhase != GameManager.CurriculumPhase.DestroyImmobileTarget1 && gameManager.currentPhase != GameManager.CurriculumPhase.DestroyImmobileTarget2)
        //{
        //    rotationY += (YInput * sensitivity) * 100 * Time.deltaTime;
        //}
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
        TransmitSound();
        l.direction = shootingPoint.forward;
        l.shooter = this;
    }

    protected void TransmitSound()
    {
        Vector3 position = this.transform.localPosition;
        Icreature e = this;

        SoundInfo info = new SoundInfo(position, e);

        gameManager.TransmitSound(info);
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
        Vector3 location = this.transform.localPosition;
        float rotation = this.transform.eulerAngles.y;
        Icreature entity = this;

        VisionInfo info = new VisionInfo(location, rotation, entity);

        gameManager.TransmitLocation(info);
    }

    public void ReceiveLocation(VisionInfo info)
    {
        Vector3 direction = info.pos - this.transform.localPosition;
        float horizontalAngle = Vector3.Angle(direction, this.transform.forward);
        float verticalAngle = Vector3.Angle(direction, playerCamera.forward);

        Vector3 offset = Vector3.up * 5;

        if (verticalAngle < 35)
        {
            AddReward(0.0003f);
            if (horizontalAngle < 45) //enemy is within the agent's field of vision
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, direction, out hit, 1000))
                {
                    //Debug.DrawRay(transform.localPosition, direction * hit.distance, Color.red);

                    if (hit.collider.gameObject.CompareTag("Player")) //Agent can see the Enemy
                    {
                        Debug.DrawLine(transform.position + offset, hit.point + offset, Color.red);
                        lastEnemySeenPosition = info.pos;
                        lastEnemyRotation = info.degreeRotation;

                        if (horizontalAngle < 15f && verticalAngle < 10f) //agent has enemy in target sight
                        {
                            Debug.DrawLine(transform.position + offset, hit.point + offset, Color.red);
                            AddReward(0.004f);
                        }
                        else
                        {
                            Debug.DrawLine(transform.position + offset, hit.point + offset, Color.yellow);
                            AddReward(0.002f);
                        }
                    }
                    else //enemy is blocked by something
                    {
                        Debug.DrawLine(transform.position + offset, hit.point + offset, Color.green);
                    }
                }
                else
                {
                    Debug.DrawRay(transform.position + offset, direction * 1000, Color.blue);
                }
            }
        }
        else if (verticalAngle > 60) //check to see if the agent isn't looking up or down, which is unnecessary 
        {
            AddReward(-0.005f);
        }
    }
    public void Respawn(Vector3 location, Quaternion rotation)
    {
        //Debug.Log($"{gameObject.name} is respawning");

        gameObject.SetActive(true);
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

        TransmitSound();

    }

    public bool ReceiveHealth(int ammount)
    {
        if (currentHealth < maxHealth) //check if current health is not maxed or overcharged
        {
            if (currentHealth < maxHealth / 2)
            {
                AddReward(2f); //Reward agent for picking up health while it is less than half. 
            }

            currentHealth += ammount; //fill health

            if (currentHealth > maxHealth) currentHealth = maxHealth; //don't let health overflow over the max 
            //Debug.Log($"health is now {currentHealth}");
            

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
            //gameObject.SetActive(false);
            AddReward(-2);
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
            AddReward(2); //reward for hitting enemy
            gameManager.IncreaseEpisodeLimit((int)(ManualMaxStep * 0.2f)); //increase Episode limit

            if (info.Destroy)
            {               
                score++;
                AddReward(5 * score); //Reward for destroying enemy
                if (score == gameManager.maxScore) //if agent reaches max score, end episode
                {
                    EndEpisodeInSuccess();
                }
                else //if max has not been reached, respawn opponent, continue episode
                {
                    gameManager.ContinueEpisode(this);
                }
            }
        }
        else
        {
            AddReward(-0.02f); //penalty for missing enemy
        }
    }

    public void Store()
    {
       // transform.localPosition = storageSpace.localPosition;
        transform.rotation = Quaternion.identity;

       // gameObject.SetActive(false);
    }

    private void EndEpisodeInSuccess()
    {
        score = 0;

        //Debug.Log("ending episode in sucess");
        AddReward(10 * gameManager.maxScore);

        sucessfullGames++;

        Debug.Log($"Completed game number {sucessfullGames} sucessfully");

        if (!gameManager.ReadyForPhaseUp)
        {
            ConsecutiveWinsThisPhase++;
            if (ConsecutiveWinsThisPhase == 10)
            {
                ConsecutiveWinsThisPhase = 0;
                gameManager.MoveToNextPhase();
            }
        }

        //end episode
        gameManager.EndEpisode(); //end current episode
    }

    private void EndEpisodeInFailure()
    {
        //Debug.Log("ending episode in sucess");
        score = 0;

        ConsecutiveWinsThisPhase = 0;

        gameManager.EndEpisode();
    }

    private void SetMaxStep()
    {
        switch (gameManager.currentPhase)
        {
            case GameManager.CurriculumPhase.Phase1_ImmobileTarget1:
                this.ManualMaxStep = 500;
                break;
            case GameManager.CurriculumPhase.Phase2_ImmobileTarget2:
                this.ManualMaxStep = 700;
                break;
            case GameManager.CurriculumPhase.Phase3_ImmobileTarget3:
                this.ManualMaxStep = 700;
                break;
            case GameManager.CurriculumPhase.Phase4_WanderingTarget1:
                this.ManualMaxStep = 800;
                break;
            case GameManager.CurriculumPhase.Phase5_Obstacles:

                this.ManualMaxStep = 1200;
                break;

            case GameManager.CurriculumPhase.Phase6_Arena1:
                this.ManualMaxStep = 1200;
                break;
            case GameManager.CurriculumPhase.Phase7_Arena2:
                this.ManualMaxStep = 1500;
                break;
            case GameManager.CurriculumPhase.Phase8_BattleSelf:
                this.ManualMaxStep = 2000;
                break;

            default:
                this.ManualMaxStep = 1000;                
                break;
        }
    }

    public void IncreaseMaxStep(int ammount)
    {      
        //Debug.Log("Increasing step ammount by " + ammount);
        this.ManualMaxStep += ammount;
    }

    public void ObstacleAvoidancePenalty()
    {
        AddReward(-0.01f);
        //Debug.Log("2close to an obstacle");
    }


}
