using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using UnityEngine.Windows;
using UnityEditor;

public class BattleManager : MonoBehaviour
{
    [SerializeField]
    public List<BattlePlayer> players;

    public List<BattlePlayer> MatchParticipants { get; private set; }

    [SerializeField]
    private List<PlayerSpawner> playerSpawners = new List<PlayerSpawner>();

    [SerializeField]
    public int maxScore; //max score

    [SerializeField]
    private ObstacleManager largeObstacles;

    [HideInInspector]
    public int playerCount;
    public int MatchParticipantsNr;

    public int MaxWins;
    public int MaxTimeouts;

    float CurrentSpeed = 1;
    float initialTimeScale = 1.0f;

    [SerializeField]
    bool obstacles;

    private void Awake()
    {

        if (MatchParticipants == null) MatchParticipants = new List<BattlePlayer>();

        playerCount = players.Count;
        if (playerCount == 0)
        {
            Debug.Log("Match participants list was empty!");
            throw new System.Exception("Match participants list was empty!");
        }



        Debug.Log($"Starting, match participants found: {playerCount}");
    }


    // Start is called before the first frame update
    void Start()
    {
        LoadResultsFile();

        foreach (var p in players)
        {
            p.SetToSleep();
        }

        Setupbattle();




    }


    // Update is called once per frame
    void Update()
    {
        if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (CurrentSpeed != 0.5f) CurrentSpeed = 0.5f;
            SetTimeScale(CurrentSpeed);
        }
        if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (CurrentSpeed != 1f) CurrentSpeed = 1f;
            SetTimeScale(CurrentSpeed);
        }
        if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (CurrentSpeed != 2f) CurrentSpeed = 2f;
            SetTimeScale(CurrentSpeed);
        }
        if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha4))
        {
            if (CurrentSpeed != 3f) CurrentSpeed = 3f;
            SetTimeScale(CurrentSpeed);
        }

    }

    public void SetTimeScale(float scale)
    {
        Time.timeScale = scale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }

    private void FixedUpdate()
    {
        MatchParticipantsNr = MatchParticipants.Count;
    }

    private BattlePlayer[] GetNextParticipants()
    {
        BattlePlayer[] participants;
        participants = new BattlePlayer[2];

        bool foundPossibleMatch = false;
        int PlayerId = 0;

        //Debug.Log("Getting First player...");
        foreach (var p in players) //get first Player
        {           
            int opponentID = p.BattleData.GetNextOpponent(players, MaxWins, MaxTimeouts);

            if (opponentID == 0) //there was no available Opponent FOR this player
            {                
                continue;
            }
            else //found and opponent, Go to Battle
            {
                participants[0] = p;

                participants[1] = GetPlayerFromID(opponentID);

                if (participants[0] == null) //error Happened
                {
                    EditorApplication.isPaused = true;
                    throw new System.Exception($"System failed to find an opponent with ID {p.BattleData.myID}");
                    EditorApplication.isPlaying = false;

                }
                if (participants[1] == null) //error Happened
                {
                    EditorApplication.isPaused = true;
                    throw new System.Exception($"System failed to find an opponent with ID {opponentID}");
                    EditorApplication.isPlaying = false;

                }
                foundPossibleMatch = true;

                break;
            }
        }

        if (!foundPossibleMatch)
        {
            Debug.Log("System failed to find an opponent, Ending Game");
            EditorApplication.isPaused = true;
            EditorApplication.isPlaying = false;
            Application.Quit();
            return null;
        }

        return participants;
    }

    public void Setupbattle()
    {
        Debug.Log("Setting up next battle...");
        MatchParticipants.Clear();

        BattlePlayer[] participants = GetNextParticipants();

        if (participants != null)
        {
            Debug.Log($"Player '{participants[0].BattleData.Name}' (ID: '{participants[0].BattleData.myID}') vs Player {participants[1].BattleData.Name} (ID: '{participants[1].BattleData.myID}')");

            foreach (var i in participants)
            {
                MatchParticipants.Add(i);

                i.BattleData.AddTotalGames();
            }

            StartCoroutine(EndBattleByTimeOut());

            Battle();
        }
        else
        {
            Debug.Log("Error, No participants Founds");
        }
    }

    void Battle()
    {      
        BattlePlayer player1 = MatchParticipants[0];
        BattlePlayer player2 = MatchParticipants[1];

        //1. Set Obstacles on
        if (obstacles) largeObstacles.Activate();
        else largeObstacles.DeActive();
        //largeObstacles.DeActive();

        //2. Spawn first player in a random pre place spawner

        PlayerSpawner pSpawner = SpawnInRandomSpawner(player1);

        //3. Spawn second player in furthest away spawner

        PlayerSpawner ESpawner = SpawnInFurthestSpawner(pSpawner, player2);

        Debug.Log("Battle is Starting");
    }

    //Continue episode in case the score isn't maxed yet
    public void ContinueBattle(BattlePlayer winner)
    {
        //Debug.Log("Continuing episode");
        //opponent is null
        //if (Opponent == null) Debug.Log("Opponent was null");
        
        StopAllCoroutines(); //stop the battleTimer

        BattlePlayer loser = GetLoser(winner); //get the loser

        Debug.Log($"a round has ended, {winner.BattleData.Name} killed {loser.BattleData.Name}");

        SpawnInRandomSpawner(loser); //Respawn the loser

        StartCoroutine(EndBattleByTimeOut()); //restart battle timer
    }

    public void EndBattle(BattlePlayer winner) 
    { 
        StopAllCoroutines();

        BattlePlayer loser = GetLoser(winner);

        Debug.Log($"battle over: winner: {winner.BattleData.Name} (ID: {winner.BattleData.myID}) Loser: {loser.BattleData.Name} (ID: {loser.BattleData.myID})");

        winner.BattleData.AddTotalBattleWins();
        loser.BattleData.AddTotalBattleLosses();
        
        winner.BattleData.AddTotalSuccessfulBattles();
        loser.BattleData.AddTotalSuccessfulBattles();

        int totalWinnerGames = winner.BattleData.AddBattleRecord(loser.BattleData.myID);
        int totalLoserGames = loser.BattleData.AddBattleRecord(winner.BattleData.myID);

        Debug.Log($"{winner.BattleData.Name} now has {totalWinnerGames} games played agaisnt {loser.BattleData.Name}");
        Debug.Log($"{loser.BattleData.Name} now has {totalLoserGames} games played agaisnt {winner.BattleData.Name}");

        foreach (var p in MatchParticipants)
        {
            p.ReSetRoundScore();
            p.SetToSleep();
            p.Store();
        }

        MatchParticipants.Clear();

        SaveResults();

        Setupbattle();
    }

    private IEnumerator EndBattleByTimeOut()
    {
        yield return new WaitForSeconds(60);

        StopAllCoroutines();

        Debug.Log($"Battle is ending due to time out! {MatchParticipants[0].BattleData.Name} vs {MatchParticipants[1].BattleData.Name}");

        int totalPlayer1TimeOuts = MatchParticipants[0].BattleData.AddTimeOutRecord(MatchParticipants[1].BattleData.myID);
        int totalPlayer2TimeOuts = MatchParticipants[1].BattleData.AddTimeOutRecord(MatchParticipants[0].BattleData.myID);

        Debug.Log($"{MatchParticipants[0].BattleData.Name} now has {totalPlayer1TimeOuts} timeouts agaisnt {MatchParticipants[1].BattleData.Name}");
        Debug.Log($"{MatchParticipants[1].BattleData.Name} now has {totalPlayer2TimeOuts} timeouts agaisnt {MatchParticipants[0].BattleData.Name}");

        foreach (var p in MatchParticipants)
        {
            p.BattleData.AddTotalTimedOutBattle();

            p.ReSetRoundScore();
            p.EndEpisode();
            p.SetToSleep();
            p.Store();
            
        }

        SaveResults();

        MatchParticipants.Clear();



        Setupbattle();
    }


    private BattlePlayer GetLoser(BattlePlayer winner)
    {
        if (MatchParticipants.Count > 2) throw new System.Exception("match had more than 2 participants");

        BattlePlayer loser = MatchParticipants[0];

        if (winner == loser) loser = MatchParticipants[1];

        return loser;
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
                if ((p as Icreature) != info.emitter)
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
                if ((p as Icreature) != info.emitter)
                {
                    p.ReceiveLocation(info);
                }
            }
        }
    }

    private PlayerSpawner SpawnInRandomSpawner(BattlePlayer p)
    {
        PlayerSpawner playerSpawner = playerSpawners[0];
        playerSpawner = playerSpawners[Random.Range(0, playerSpawners.Count)];
        p.WakeUp();
        playerSpawner.SpawnEntity(p);

        return playerSpawner;
    }

    private PlayerSpawner SpawnInFurthestSpawner(PlayerSpawner pSpawner, BattlePlayer opponent)
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

        opponent.WakeUp();
        furthest.SpawnEntity(opponent);

        return furthest;
    }

    private PlayerSpawner SpawnInFurthestSpawner(Vector3 location, BattlePlayer opponent)
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
    internal void HitNotice(BattlePlayer p, HitInfo info)
    {
       
    }
    internal void KillNotice(BattlePlayer p, HitInfo info)
    {
        BattlePlayer opponent = (info.HitEnemy as BattlePlayer);

    

        if (p.roundScore >= maxScore)
        {
            //end battle

            EndBattle(p);
        }

        else
        {
            //continue battle


            
            ContinueBattle(p);
        }


    }


    private void LoadResultsFile()
    {
        Debug.Log("Loading results file");
        // The target file path e.g.
        var folder = Application.streamingAssetsPath;

        if (!Directory.Exists(folder))
        {
            Debug.Log($"folder directory didn't exist, creating a new folder @ {folder}");
            Directory.CreateDirectory(folder);
        }

        var filePath = System.IO.Path.Combine(folder, "results.csv");


        if (!Directory.Exists(filePath))
        {
            CreateResultsFile();
        }
        else
        {
            Debug.Log("Results file found, Loading file...");
            using (var reader = new System.IO.StreamReader(filePath))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    string[] values = line.Split(';');
                    int[] intValues = new int[values.Length];

                    BattlePlayer currentPlayer = players.First(s => s.BattleData.myID == System.Int32.Parse(values[0]));

                    intValues[0] = currentPlayer.BattleData.myID;
                    intValues[1] = 0;
                    intValues[2] = currentPlayer.BattleData.TeamId;
                    for (int i = 3; i < values.Length; i++)
                    {
                        intValues[i] = System.Int32.Parse(values[i]);
                    }

                    currentPlayer.BattleData.UpdateAllValues(intValues[3], intValues[4], intValues[5], intValues[6], intValues[7], intValues[8], intValues[9], intValues[10], intValues[11]);
                }
            }
            
            Debug.Log("Results file loaded");
            EditorApplication.isPaused = true;
        }
    }

    private void CreateResultsFile()
    {
        Debug.Log("results file didn't exist, creating a new results file");
        // The target file path e.g.
        var folder = Application.streamingAssetsPath;

        if (!Directory.Exists(folder))
        {
            Debug.Log($"folder directory didn't exist, creating a new folder @ {folder}");
            Directory.CreateDirectory(folder);
        } 

        var cvs = new StringBuilder();

        foreach (BattlePlayer p in players)
        {
            string ID = p.BattleData.myID.ToString();
            string Name = p.BattleData.Name;
            string TeamID = p.BattleData.TeamId.ToString();
            string TotalBattleWins = 0.ToString();
            string TotalBattleLosses = 0.ToString();
            string TotalSuccessfulBattles = 0.ToString();
            string TotalTimedOutBattles = 0.ToString();
            string TotalGames = 0.ToString();
            string TotalMisses = 0.ToString();
            string TotalHits = 0.ToString(); ;
            string TotalKills = 0.ToString();
            string TotalDeaths = 0.ToString();

            string newLine = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}, {11}", ID, Name, TeamID, TotalBattleWins, TotalBattleLosses, TotalSuccessfulBattles, TotalTimedOutBattles, TotalGames, TotalMisses, TotalHits, TotalKills, TotalDeaths);

            cvs.AppendLine(newLine);
        }

        var filePath = System.IO.Path.Combine(folder, "results.csv");

        using (var writer = new System.IO.StreamWriter(filePath, false))
        {
            writer.Write(cvs.ToString());
        }
        Debug.Log($"Finished Creating results file, can be found @ {filePath}");
        EditorApplication.isPaused = true;
    }

    private void SaveResults()
    {
        Debug.Log("Saving Results to file...");
        // The target file path e.g.
        var folder = Application.streamingAssetsPath;

        if (!Directory.Exists(folder))
        {
            Debug.Log($"folder directory didn't exist, creating a new folder @ {folder}");
            Directory.CreateDirectory(folder);
        }

        var cvs = new StringBuilder();

        foreach (var p in players)
        {
            string ID = p.BattleData.myID.ToString();
            string Name = p.BattleData.Name;
            string TeamID = p.BattleData.TeamId.ToString();
            string TotalBattleWins = p.BattleData.TotalBattleWins.ToString();
            string TotalBattleLosses = p.BattleData.TotalBattleLosses.ToString();
            string TotalSuccessfulBattles = p.BattleData.TotalSuccessfulBattles.ToString();
            string TotalTimedOutBattles = p.BattleData.TotalTimedOutBattles.ToString();
            string TotalGames = p.BattleData.TotalGames.ToString();
            string TotalMisses = p.BattleData.TotalMisses.ToString();
            string TotalHits = p.BattleData.TotalHits.ToString();
            string TotalKills = p.BattleData.TotalKills.ToString();
            string TotalDeaths = p.BattleData.TotalDeaths.ToString();

            string newLine = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}, {11}", ID, Name, TeamID, TotalBattleWins, TotalBattleLosses, TotalSuccessfulBattles, TotalTimedOutBattles, TotalGames, TotalMisses, TotalHits, TotalKills, TotalDeaths);

            cvs.AppendLine(newLine);
        }

        var filePath = System.IO.Path.Combine(folder, "results.csv");

        using (var writer = new System.IO.StreamWriter(filePath, false))
        {
            writer.Write(cvs.ToString());
        }

        Debug.Log("Finished saving results!");
    }

    private BattlePlayer GetPlayerFromID(int ID)
    {
        BattlePlayer player = null;

        foreach (var t in players)
        {
            if (t.BattleData.myID == ID) { player = t; break; }       //works
        }

        if (player == null) Debug.Log($"Failed to find a player with id {ID}");

        return player;
    }
}

