using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public enum CurriculumPhase
    {
        DestroyImmobileTarget1,     //Phase 1 - Destroy Target directly in front
        DestroyImmobileTarget2,     //Phase 2 - Destroy Target that is slightly off centered from the front of the agent
        DestroyImmobileTarget3,     //Phase 3 - Destroy target that spawns randomly arround agent
        DestroyImmobileTarget4,     //Phase 4 - Find and Destroy target in arena
        DestroyMobileTarget,        //Phase 5 - Find and destroy target that moves in arena
        BattleSelf                  //Phase 6 - Agent Fights agaisnt itself
    }

    public CurriculumPhase currentPhase;

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

    [SerializeField]
    private ObstacleManager obstacleManager;

    private void Awake()
    {

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
        switch (currentPhase)
        {
            case CurriculumPhase.DestroyImmobileTarget1: //Max score: 1
            case CurriculumPhase.DestroyImmobileTarget2: //max score: 1
            case CurriculumPhase.DestroyImmobileTarget3: //max score: 1
                maxScore = 1;

                FetchOpponent(Immobiletarget);

                StartBasicTraining();

                break;
            case CurriculumPhase.DestroyImmobileTarget4: //max score: 1
                maxScore = 2;

                FetchOpponent(Immobiletarget);

                IntermediateTraining();

                break;
            case CurriculumPhase.DestroyMobileTarget: //max score: 3
                maxScore = 3;

                FetchOpponent(MobileTarget);
                IntermediateTraining();

                break;
            case CurriculumPhase.BattleSelf: //max score: 5
                maxScore = 4;

                break;
            default:
                break;
        }
    }

    void StartBasicTraining()
    {
        //1. Set Obstacles off
        obstacleManager.DeActive();

        //2. Spawn Agent in middle
        PlayerTrainingSpawner.SpawnEntity(player);
        player.RotateToRandom();

        MatchParticipants.Clear();
        MatchParticipants.Add(player);
        MatchParticipants.Add(Opponent);

        //3. Spawn Target 
        if (currentPhase == CurriculumPhase.DestroyImmobileTarget1)
        {
            //directly in front of agent
            SpawnImmobileAgent1();
        }
        else if (currentPhase == CurriculumPhase.DestroyImmobileTarget2)
        {
            //In front of agent
            SpawnImmobileAgent2();
        }
        else if (currentPhase == CurriculumPhase.DestroyImmobileTarget3)
        {
            //in a random spawner around the middle
            SpawnImmobileAgent3();
        }

    }

    void IntermediateTraining()
    {

        MatchParticipants.Clear();
        MatchParticipants.Add(player);
        MatchParticipants.Add(Opponent);

        //1. Set Obstacles on
        obstacleManager.Activate();

        //2. Spawn Agent in a random pre place spawner

        PlayerSpawner pSpawner = SpawnInRandomSpawner(player);

        //3. Spawn Target in furthest away spawner

        PlayerSpawner ESpawner = SpawnInFurthestSpawner(pSpawner, Opponent);


    }

    //Continue episode in case the score isn't maxed yet
    public void ContinueEpisode()
    {
        switch (currentPhase)
        {
            case CurriculumPhase.DestroyImmobileTarget2:

                SpawnImmobileAgent2();

                break;
            case CurriculumPhase.DestroyImmobileTarget3:
                SpawnImmobileAgent3();
                break;
            case CurriculumPhase.DestroyImmobileTarget4:

                SpawnInFurthestSpawner(player.transform.localPosition, Opponent);

                break;
            case CurriculumPhase.DestroyMobileTarget:

                SpawnInFurthestSpawner(player.transform.localPosition, Opponent);

                break;
            case CurriculumPhase.BattleSelf:

                break;
            default:
                break;
        }
    }


    public void EndEpisode()
    {
        Debug.Log("Ending Episode");

        foreach (var p in MatchParticipants)
        {
            p.SetToSleep();
        }

        CleanUpOpponent();
        MatchParticipants.Clear();
    }

    private void FetchOpponent(GameObject OpponentObject)
    {
        Icreature l = OpponentObject.GetComponent<Icreature>();
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

        float furthestDist = Vector3.Distance(pSpawner.transform.localPosition, furthest.transform.localPosition);

        foreach (var s in playerSpawners)
        {
            if (s == pSpawner) continue;
            float dist = Vector3.Distance(pSpawner.transform.localPosition, s.transform.localPosition);

            if (dist > furthestDist)
            {
                furthestDist = dist;
                furthest = s;
            }
        }

        furthest.SpawnEntity(opponent);

        return furthest;
    }

    private PlayerSpawner SpawnInFurthestSpawner(Vector3 location, Icreature opponent)
    {
        PlayerSpawner furthest = playerSpawners[0];
        float furthestDist = Vector3.Distance(location, furthest.transform.localPosition);

        foreach (var s in playerSpawners)
        {
            float dist = Vector3.Distance(location, s.transform.localPosition);

            if (dist > furthestDist)
            {
                furthestDist = dist;
                furthest = s;
            }
        }

        furthest.SpawnEntity(opponent);

        return furthest;
    }

    public void MoveToNextPhase()
    {
        switch (currentPhase)
        {
            case CurriculumPhase.DestroyImmobileTarget1:
                Debug.Log("Moving to phase 2");
                currentPhase = CurriculumPhase.DestroyImmobileTarget2;
                break;
            case CurriculumPhase.DestroyImmobileTarget2:
                Debug.Log("Moving to phase 3");
                currentPhase = CurriculumPhase.DestroyImmobileTarget3;
                break;
            case CurriculumPhase.DestroyImmobileTarget3:
                Debug.Log("Moving to phase 4");
                currentPhase = CurriculumPhase.DestroyImmobileTarget4;
                break;
            case CurriculumPhase.DestroyImmobileTarget4:
                Debug.Log("Moving to phase 5");
                currentPhase = CurriculumPhase.DestroyMobileTarget;
                break;
            case CurriculumPhase.DestroyMobileTarget:
                Debug.Log("Moving to phase 6");
                currentPhase = CurriculumPhase.BattleSelf;
                break;
            case CurriculumPhase.BattleSelf:
                //do nothing
                break;
            default:
                break;
        }

    }
}
