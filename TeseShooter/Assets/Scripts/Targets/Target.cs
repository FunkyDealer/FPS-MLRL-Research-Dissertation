using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour, Icreature
{  
    [SerializeField]
    protected int maxHealth;
    protected int currentHealth;

    protected bool moving = false;
    protected bool canStep = true; //can the entity play a step sound?

    public GameManager gameManager;
    protected int currentScore;

    protected bool canShoot = true;

    [SerializeField]
    Transform storageSpace;

    protected virtual void Awake()
    {
        gameManager = GetComponentInParent<GameManager>();
        currentHealth = maxHealth;
        currentScore = 0;
    }

    // Start is called before the first frame update
    void Start()
    {
       

    }

    void OnEnable()
    {
        //Debug.Log("Target Starting");
    }

    // Update is called once per frame
    protected virtual void Update()
    {

    }

    protected virtual void FixedUpdate()
    {
        TransmitLocation();
    }

    protected virtual void LateUpdate()
    {
        if (moving)
        {
            if (canStep)
            {
                PlaySound();
                StartCoroutine(StepDelay());
            }
        }
    }

    public virtual bool ReceiveDamage(int ammount)
    {
        currentHealth -= ammount;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            return true;
        }

        return false;
    }

    public bool ReceiveHealth(int ammount)
    {
        if (currentHealth < maxHealth) //check if current health is not maxed or overcharged
        {
            currentHealth += ammount;

            if (currentHealth > maxHealth) currentHealth = maxHealth;
            //Debug.Log($"health is now {currentHealth}");

            return true;
        }
        return false;
    }

    public void SetToSleep()
    {
        gameObject.SetActive(false);
    }

    public virtual void Respawn(Vector3 location, Quaternion rotation)
    {
        this.gameObject.SetActive(true);
        transform.position = location;
        transform.rotation = rotation;

        currentHealth = maxHealth;
        canShoot = true;
        canStep = true;

        //Debug.Log($"{gameObject.name} is spawning");

        PlaySound();
    }

    private IEnumerator StepDelay()
    {
        canStep = false;
        yield return new WaitForSeconds(1f);
        if (!canStep) canStep = true;
    }

    protected void PlaySound()
    {
        Vector3 position = this.transform.localPosition;
        Icreature e = this;

        SoundInfo info = new SoundInfo(position, e);

        gameManager.TransmitSound(info);

    }

    public virtual void receiveSound(SoundInfo sound)
    {
        //Handle sound receiving

        //Debug.Log($"{gameObject.name}: heard sound from {sound.pos} by {sound.emitter.gameObject.name}");

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

    public virtual void ReceiveLocation(VisionInfo info)
    {
        //do nothing
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
        //do nothing
    }

    public void Store()
    {
        //transform.localPosition = storageSpace.localPosition;
        transform.rotation = Quaternion.identity;


        //gameObject.SetActive(false);

    }
}

