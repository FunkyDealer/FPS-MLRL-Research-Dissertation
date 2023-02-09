using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager inst { get { return _instance; } }

    [SerializeField]
    public Player player;

    public List<Icreature> MatchParticipants { get; private set; }

    Icreature Opponent = null;

    [SerializeField]
    private List<PlayerSpawner> playerSpawners = new List<PlayerSpawner>();

    [SerializeField]
    private List<HealthSpawner> healthSpawners = new List<HealthSpawner>();
    HealthSpawner[] chosenHealthSpawner;

    [SerializeField]
    private List<PlayerSpawner> TrainingObjectSpawner = new List<PlayerSpawner>();

    [SerializeField]
    PlayerSpawner PlayerTrainingSpawner;

    [SerializeField]
    GameObject HealthBoxPrefab;

    //Objects
    [SerializeField]
    GameObject Immobiletarget;
    [SerializeField]
    GameObject MobileTarget;
    [SerializeField]
    GameObject AgentClone;

    public int maxScore { get; private set; } //max score in curriculum phase    




    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
         
        if (MatchParticipants == null) MatchParticipants = new List<Icreature>();
     
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartMatch()
    {
        switch (EnvironmentManager.inst.currentPhase)
        {
            case EnvironmentManager.CurriculumPhase.DestroyImmobileTarget1: //Max score: 1
            case EnvironmentManager.CurriculumPhase.DestroyImmobileTarget2: //max score: 1
            case EnvironmentManager.CurriculumPhase.DestroyImmobileTarget3: //max score: 1
                maxScore = 1;

                FetchOpponent(Immobiletarget);

                StartBasicTraining();

                break;
            case EnvironmentManager.CurriculumPhase.DestroyImmobileTarget4: //max score: 1
                maxScore = 2;

                FetchOpponent(Immobiletarget);

                IntermediateTraining();

                break;
            case EnvironmentManager.CurriculumPhase.DestroyMobileTarget: //max score: 3
                maxScore = 3;               

                break;
            case EnvironmentManager.CurriculumPhase.BattleSelf: //max score: 5
                maxScore = 4;

                break;
            default:
                break;
        }
    }

    void StartBasicTraining()
    {
        //1. Set Obstacles off
        ObstacleManager.inst.DeActive();

        //2. Spawn Agent in middle
        PlayerTrainingSpawner.SpawnEntity(player);
        player.RotateToRandom();

        MatchParticipants.Clear();
        MatchParticipants.Add(player);
        MatchParticipants.Add(Opponent);      

        //3. Spawn Target 
        if (EnvironmentManager.inst.currentPhase == EnvironmentManager.CurriculumPhase.DestroyImmobileTarget1)
        {
            //directly in front of agent
            SpawnImmobileAgent1();
        }
        else if (EnvironmentManager.inst.currentPhase == EnvironmentManager.CurriculumPhase.DestroyImmobileTarget2)
        {
            //In front of agent
            SpawnImmobileAgent2();
        }
        else if (EnvironmentManager.inst.currentPhase == EnvironmentManager.CurriculumPhase.DestroyImmobileTarget3)
        {
            //in a random spawner around the middle
            SpawnImmobileAgent3();
        }      

    }

    void IntermediateTraining()
    {
        //1. Set Obstacles on
        ObstacleManager.inst.Activate();

        //2. Spawn Agent in a random pre place spawner

        PlayerSpawner pSpawner = SpawnInRandomSpawner(player);

        //3. Spawn Target in furthest away spawner

        PlayerSpawner ESpawner = SpawnInFurthestSpawner(pSpawner, Opponent);

        


    }



    //Continue episode in case the score isn't maxed yet
    public void ContinueEpisode()
    {
        switch (EnvironmentManager.inst.currentPhase)
        {
            case EnvironmentManager.CurriculumPhase.DestroyImmobileTarget2:

                SpawnImmobileAgent2();

                break;
            case EnvironmentManager.CurriculumPhase.DestroyImmobileTarget3:
                break;
            case EnvironmentManager.CurriculumPhase.DestroyImmobileTarget4:
                break;
            case EnvironmentManager.CurriculumPhase.DestroyMobileTarget:
                break;
            case EnvironmentManager.CurriculumPhase.BattleSelf:
                break;
            default:
                break;
        }
    }


    public void EndEpisode()
    {
        foreach (var p in MatchParticipants)
        {
            p.SetToSleep();
        }

        CleanUpOpponent();
        MatchParticipants.Clear();
    }

    private void FetchOpponent(GameObject OpponentPrefab)
    {
        Icreature l = OpponentPrefab.GetComponent<Icreature>();
        Opponent = l;
        l.SetToSleep();   
    }

    private void CleanUpOpponent()
    {
        MatchParticipants.Remove(Opponent);
        Opponent.Store();
        Opponent = null;
    }


    public void StartHealthRespawnTimer()
    {

    }

    private object[] ChooseRandomAmmountOfObjectsFromList(List<object> list, int ammount)
    {
        List<object> temp = list;
        object[] result = new object[ammount];

        for (int i = 0; i < ammount; i++)
        {
            object tempObj = temp[Random.Range(0, temp.Count)];

            result[i] = tempObj;
            temp.Remove(tempObj);
        }

        return result;
    }


    public void TransmitSound(SoundInfo info)
    {
        //if (MatchParticipants.Count == 0) Debug.Log("participants was empty");

        if (MatchParticipants.Count > 1)
        {
            foreach (var p in MatchParticipants)
            {
                if (p != info.emitter)
                {
                    p.receiveSound(info);
                }
            }
        }
    }

    public void TransmitLocation(VisionInfo info)
    {
        if (MatchParticipants.Count > 1)
        {
            foreach (var p in MatchParticipants)
            {
                if (p != info.emitter)
                {
                    p.ReceiveLocation(info);
                }
            }
        }
    }

    private void SpawnImmobileAgent1()
    {
        Vector3 EnemySpawnPos = player.transform.position + player.transform.forward * 12;

        Opponent.Respawn(EnemySpawnPos, Quaternion.identity);
    }

    private void SpawnImmobileAgent2()
    {
        Vector3 EnemySpawnPos = player.transform.position + player.transform.forward * 12;

        float y = EnemySpawnPos.y;
        EnemySpawnPos += Random.insideUnitSphere * 10;
        EnemySpawnPos.y = y;

        Opponent.Respawn(EnemySpawnPos, Quaternion.identity);
    }

    private void SpawnImmobileAgent3()
    {
        //3. Spawn Opponent randomly in one of the object spawners
        PlayerSpawner OpponentSpawn = TrainingObjectSpawner[0];
        OpponentSpawn = TrainingObjectSpawner[Random.Range(0, TrainingObjectSpawner.Count)];
        OpponentSpawn.SpawnEntity(Opponent);
    }

    private PlayerSpawner SpawnInRandomSpawner(Icreature p)
    {
        PlayerSpawner playerSpawner = playerSpawners[0];
        playerSpawner = playerSpawners[Random.Range(0, playerSpawners.Count)];
        playerSpawner.SpawnEntity(p);

        return playerSpawner;
    }

    private PlayerSpawner SpawnInFurthestSpawner(PlayerSpawner pSpawner, Icreature opponent)
    {
        PlayerSpawner furthest = playerSpawners[0];
        if (furthest == pSpawner) furthest = playerSpawners[1];

        float furthestDist = Vector3.Distance(pSpawner.transform.position, furthest.transform.position);

        foreach (var s in playerSpawners)
        {
            if (s == pSpawner) continue;
            float dist = Vector3.Distance(pSpawner.transform.position, s.transform.position);

            if (dist > furthestDist)
            {
                furthestDist = dist;
                furthest = s;
            }
        }

        furthest.SpawnEntity(opponent);

        return furthest;
    }

}
