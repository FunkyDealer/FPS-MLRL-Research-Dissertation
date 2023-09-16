using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public enum CurriculumPhase
    {
        Phase1_ImmobileTarget1,     //Phase 1 - Destroy Target directly in front
        Phase2_ImmobileTarget2,     //Phase 2 - Destroy Target that is slightly off centered from the front of the agent
        Phase3_ImmobileTarget3,     //Phase 3 - Destroy target that spawns randomly arround agent
        Phase4_WanderingTarget1,    //phase 4 - Destroy target that spawn randomly arround agent and moves around
        Phase5_Obstacles,           //phase 5 - Find and destroy target in final arena while spawning in middle
        Phase6_Arena1,              //Phase 6 - Find and Destroy target in the final arena
        Phase7_Arena2,              //Phase 7 - Find and destroy target that moves in the arena
        Phase8_BattleSelf           //Phase 8 - Agent Fights agaisnt itself
    }

    public CurriculumPhase currentPhase;

    [SerializeField]
    public Player player;

    public List<Icreature> MatchParticipants { get; private set; }

    Icreature Opponent;

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
    GameObject WanderingTarget;
    [SerializeField]
    GameObject MovingTarget;
    [SerializeField]
    GameObject AgentClone;

    public int maxScore { get; private set; } //max score in curriculum phase

    [SerializeField]
    private ObstacleManager largeObstacles;

    public int MatchParticipantsNr;

    public bool ReadyForPhaseUp { get; private set; }

    private void Awake()
    {

        if (MatchParticipants == null) MatchParticipants = new List<Icreature>();

        MatchParticipantsNr = MatchParticipants.Count;

        ReadyForPhaseUp = false;
    }


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {
        MatchParticipantsNr = MatchParticipants.Count;
    }

    public void StartMatch()
    {
        //Debug.Log("Episode Start");

        switch (currentPhase)
        {
            case CurriculumPhase.Phase1_ImmobileTarget1: //Max score: 1
            case CurriculumPhase.Phase2_ImmobileTarget2: //max score: 1
            case CurriculumPhase.Phase3_ImmobileTarget3: //max score: 1
                maxScore = 1;

                FetchOpponent(Immobiletarget);
                StartBasicTraining();

                break;
            case CurriculumPhase.Phase4_WanderingTarget1:

                maxScore = 1;

                FetchOpponent(WanderingTarget);
                StartBasicTraining();

                break;

            case CurriculumPhase.Phase5_Obstacles: //Max score: 1
                maxScore = 1;

                FetchOpponent(WanderingTarget);
                ObstacleTraining();

                break;


            case CurriculumPhase.Phase6_Arena1: //max score: 2
                maxScore = 2;

                FetchOpponent(WanderingTarget);
                IntermediateTraining();
                break;
            case CurriculumPhase.Phase7_Arena2: //max score: 2
                maxScore = 2;

                FetchOpponent(MovingTarget);
                IntermediateTraining();

                break;
            case CurriculumPhase.Phase8_BattleSelf: //max score: 2
                maxScore = 2;

                FetchOpponent(AgentClone);
                MasterTraining();

                break;
            default:
                break;
        }
    }

    void StartBasicTraining()
    {
        //1. Set Obstacles off
        largeObstacles.DeActive();

        //2. Spawn Agent in middle
        PlayerTrainingSpawner.SpawnEntity(player);
        player.RotateToRandom();

        MatchParticipants.Clear();
        MatchParticipants.Add(player);
        MatchParticipants.Add(Opponent);

        //3. Spawn Target 
        if (currentPhase == CurriculumPhase.Phase1_ImmobileTarget1)
        {
            //directly in front of agent
            SpawnImmobileAgent1();
        }
        else if (currentPhase == CurriculumPhase.Phase2_ImmobileTarget2)
        {
            //In front of agent
            SpawnImmobileAgent2();
        }
        else if (currentPhase == CurriculumPhase.Phase3_ImmobileTarget3 || currentPhase == CurriculumPhase.Phase4_WanderingTarget1)
        {
            //in a random spawner around the middle
            SpawnPraticeTargetAroundMiddle();
        }

    }

    void ObstacleTraining()
    {
        //1. Set Obstacles on
        largeObstacles.Activate();

        //2. Spawn Agent in middle
        PlayerTrainingSpawner.SpawnEntity(player);
        player.RotateToRandom();

        MatchParticipants.Clear();
        MatchParticipants.Add(player);
        MatchParticipants.Add(Opponent);

        //3. Spawn Target in random spawner
        SpawnInRandomSpawner(Opponent);


    }

    void IntermediateTraining()
    {

        MatchParticipants.Clear();
        MatchParticipants.Add(player);
        MatchParticipants.Add(Opponent);

        //1. Set Obstacles on
        largeObstacles.Activate();

        //2. Spawn Agent in a random pre place spawner

        PlayerSpawner pSpawner = SpawnInRandomSpawner(player);

        //3. Spawn Target in furthest away spawner

        PlayerSpawner ESpawner = SpawnInFurthestSpawner(pSpawner, Opponent);


    }

    void MasterTraining()
    {
        MatchParticipants.Clear();
        MatchParticipants.Add(player);
        MatchParticipants.Add(Opponent);

        //1. Set Obstacles on
        largeObstacles.Activate();
        //largeObstacles.DeActive();

        //2. Spawn Agent in a random pre place spawner

        PlayerSpawner pSpawner = SpawnInRandomSpawner(player);

        //3. Spawn Target in furthest away spawner

        PlayerSpawner ESpawner = SpawnInFurthestSpawner(pSpawner, Opponent);


       
    }

    //Continue episode in case the score isn't maxed yet
    public void ContinueEpisode(Icreature winner)
    {
        //Debug.Log("Continuing episode");
        //opponent is null
        //if (Opponent == null) Debug.Log("Opponent was null");

        switch (currentPhase)
        {
            case CurriculumPhase.Phase2_ImmobileTarget2:

                SpawnImmobileAgent2();

                break;
            case CurriculumPhase.Phase3_ImmobileTarget3:
                SpawnPraticeTargetAroundMiddle();

                break;

            case CurriculumPhase.Phase5_Obstacles:

                SpawnInRandomSpawner(Opponent);

                break;
            case CurriculumPhase.Phase6_Arena1:

               // SpawnInFurthestSpawner(player.transform.localPosition, Opponent);
                SpawnInRandomSpawner(Opponent);

                break;

            case CurriculumPhase.Phase7_Arena2:

                //SpawnInFurthestSpawner(player.transform.localPosition, Opponent);
                SpawnInRandomSpawner(Opponent);

                break;
            case CurriculumPhase.Phase8_BattleSelf:

                if (winner == (Icreature)player)
                {
                    //SpawnInFurthestSpawner(player.transform.position, Opponent);
                    SpawnInRandomSpawner(Opponent);
                }
                else if (winner == Opponent)
                {
                    //SpawnInFurthestSpawner(player.transform.position, player);
                    SpawnInRandomSpawner(player);
                }

                break;
            default:
                break;
        }
    }

    public void EndEpisode()
    {
        //Debug.Log("Ending Episode");

        foreach (var p in MatchParticipants)
        {
            p.SetToSleep();
        }

        if (currentPhase == CurriculumPhase.Phase8_BattleSelf)
        {
            player.EndEpisode();
            (Opponent as Player).EndEpisode();
        }
        else {
            player.EndEpisode();
            
        }

        CleanUpOpponent();
    }

    private void FetchOpponent(GameObject OpponentObject)
    {
        Icreature l = OpponentObject.GetComponent<Icreature>();
        Opponent = l;
        l.SetToSleep();
    }

    private void CleanUpOpponent()
    {
        Opponent.Store();
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

        Opponent.GetGameObject().SetActive(true);
        Opponent.Respawn(EnemySpawnPos, Quaternion.identity);
    }

    private void SpawnImmobileAgent2()
    {
        Vector3 EnemySpawnPos = player.transform.position + player.transform.forward * 12;

        float y = EnemySpawnPos.y;
        EnemySpawnPos += Random.insideUnitSphere * 10;
        EnemySpawnPos.y = y;

        Opponent.GetGameObject().SetActive(true);
        Opponent.Respawn(EnemySpawnPos, Quaternion.identity);
    }

    private void SpawnPraticeTargetAroundMiddle()
    {
        //3. Spawn Opponent randomly in one of the object spawners
        PlayerSpawner OpponentSpawn = TrainingObjectSpawner[0];
        OpponentSpawn = TrainingObjectSpawner[Random.Range(0, TrainingObjectSpawner.Count)];

        Opponent.GetGameObject().SetActive(true);
        OpponentSpawn.SpawnEntity(Opponent);
    }

    private PlayerSpawner SpawnInRandomSpawner(Icreature p)
    {
        PlayerSpawner playerSpawner = playerSpawners[0];
        playerSpawner = playerSpawners[Random.Range(0, playerSpawners.Count)];
        p.GetGameObject().SetActive(true);
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

        opponent.GetGameObject().SetActive(true);
        furthest.SpawnEntity(opponent);

        return furthest;
    }

    private PlayerSpawner SpawnInFurthestSpawner(Vector3 location, Icreature opponent)
    {
        PlayerSpawner furthest = playerSpawners[0];
        float furthestDist = Vector3.Distance(location, furthest.transform.position);

        foreach (var s in playerSpawners)
        {
            float dist = Vector3.Distance(location, s.transform.position);

            if (dist > furthestDist)
            {
                furthestDist = dist;
                furthest = s;
            }
        }

        opponent.GetGameObject().SetActive(true);
        furthest.SpawnEntity(opponent);

        return furthest;
    }

    public void MoveToNextPhase()
    {
        //Opponent.SetToSleep();

        ReadyForPhaseUp = true;

        TrainingManager.inst.CheckForPhaseUp((int)currentPhase);
       

    }

    public void GoToNextPhase()
    {
        Opponent.SetToSleep();
        ReadyForPhaseUp = false;

        switch (currentPhase)
        {
            case CurriculumPhase.Phase1_ImmobileTarget1://Move to phase 2

                currentPhase = CurriculumPhase.Phase2_ImmobileTarget2;
                break;

            case CurriculumPhase.Phase2_ImmobileTarget2://Move to phase 3

                currentPhase = CurriculumPhase.Phase3_ImmobileTarget3;
                break;

            case CurriculumPhase.Phase3_ImmobileTarget3://Move to phase 4

                currentPhase = CurriculumPhase.Phase4_WanderingTarget1;
                break;

            case CurriculumPhase.Phase4_WanderingTarget1: //Move to phase 5

                currentPhase = CurriculumPhase.Phase5_Obstacles;
                break;

            case CurriculumPhase.Phase5_Obstacles: //Move to phase 6

                currentPhase = CurriculumPhase.Phase6_Arena1;
                break;

            case CurriculumPhase.Phase6_Arena1: //Move to phase 7

                currentPhase = CurriculumPhase.Phase7_Arena2;
                break;

            case CurriculumPhase.Phase7_Arena2: //Move to phase 8

                currentPhase = CurriculumPhase.Phase8_BattleSelf;
                break;

            case CurriculumPhase.Phase8_BattleSelf: //do nothing

                //do nothing

                break;
            default:
                break;
        }
    }

    public void IncreaseEpisodeLimit(int ammount)
    {
        foreach (var p in MatchParticipants)
        {
            if (p is Player)
            {
                (p as Player).IncreaseMaxStep(ammount);
            }
        }
    }

}
