using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentBattleData
{
    public int myID { get; private set; }
    public string Name { get; private set; }
    public int TeamId { get; private set; }
    public int TotalBattleWins { get; private set; }    
    public int TotalBattleLosses { get; private set; }
    public int TotalSuccessfulBattles { get; private set; }
    public int TotalTimedOutBattles { get; private set; }    
    public int TotalGames { get; private set; }    
    public int TotalMisses { get; private set; }    
    public int TotalHits { get; private set; }    
    public int TotalKills { get; private set; }    
    public int TotalDeaths { get; private set; }

    List<Tuple<int, int, int>> BattleRecord; //ID, Sucessuful battles, Timeouts

    public AgentBattleData(int id, string name, int teamId)
    {
        this.myID = id;
        this.Name = name;
        this.TeamId = teamId;
        TotalBattleWins = 0;
        TotalBattleLosses = 0;
        TotalSuccessfulBattles = 0;
        TotalTimedOutBattles = 0;
        TotalGames = 0;
        TotalMisses = 0;
        TotalHits = 0;
        TotalKills = 0;
        TotalDeaths = 0;

        BattleRecord = new List<Tuple<int, int, int>>();
    }

    public AgentBattleData(int id, string name, int teamId, int totalBattleWins, int totalBattleLosses,int totalSuccessfulBattles, int totalTimedOutBattles, int totalGames, int totalMisses, int totalHits, int totalKills, int totalDeaths)
    {
        myID = id;
        Name = name;
        TeamId = teamId;

        this.TotalBattleWins = totalBattleWins;
        this.TotalBattleLosses = totalBattleLosses;
        this.TotalSuccessfulBattles = totalSuccessfulBattles;
        this.TotalTimedOutBattles = totalTimedOutBattles;
        this.TotalGames = totalGames;
        this.TotalMisses = totalMisses;
        this.TotalHits = totalHits;
        this.TotalKills = totalKills;
        this.TotalDeaths = totalDeaths;

        BattleRecord = new List<Tuple<int, int, int>>();
    }

    public void UpdateAllValues(int totalBattleWins, int totalBattleLosses, int totalSuccessfulBattles, int totalTimedOutBattles, int totalGames, int totalMisses, int totalHits, int totalKills, int totalDeaths)
    {
        this.TotalBattleWins = totalBattleWins;
        this.TotalBattleLosses = totalBattleLosses;
        this.TotalSuccessfulBattles = totalSuccessfulBattles;
        this.TotalTimedOutBattles = totalTimedOutBattles;
        this.TotalGames = totalGames;
        this.TotalMisses = totalMisses;
        this.TotalHits = totalHits;
        this.TotalKills = totalKills;
        this.TotalDeaths = totalDeaths;

        BattleRecord = new List<Tuple<int, int, int>>();
    }

    public void AddTotalBattleWins() => TotalBattleWins++;
    public void AddTotalBattleLosses() => TotalBattleLosses++;
    public void AddTotalSuccessfulBattles() => TotalSuccessfulBattles++;
    public void AddTotalTimedOutBattle() => TotalTimedOutBattles++;
    public void AddTotalGames() => TotalGames++;
    public void AddTotalMisses() => TotalMisses++;
    public void AddTotalHits() => TotalHits++;
    public void AddTotalKills() => TotalKills++;
    public void AddTotalDeaths() => TotalDeaths++;


    public int AddBattleRecord(int opponentID)
    {
        if (BattleRecord == null) BattleRecord = new List<Tuple<int, int, int>>();

        if (BattleRecord.Count == 0) //there are no battle records
        {
            BattleRecord.Add(new Tuple<int, int, int>(opponentID, 1 , 0)); //create a new one where the first game was sucessful     
            return 1;
        }
        else //there are battle records
        {
            Tuple<int, int, int> record = GetBattleRecordFromID(opponentID); 

            if (record == null) //the battle record for that Id was not found
            {
                BattleRecord.Add(new Tuple<int, int, int>(opponentID, 1, 0));  //create a new one where the first game was sucessful     
                return 1;
            }
            else //a battle record for that ID was found
            {
                int index = BattleRecord.FindIndex(t => t.Item1 == opponentID);
                int gamesNr = record.Item2; //number of games
                int TimeoutsNr = record.Item3; //number timeouts
                //Debug.Log($"overwriting number of games in {this.myID} vs {opponentID}: was {i}, now is {i + 1}");

                gamesNr++;

                BattleRecord[index] = Tuple.Create(opponentID, gamesNr, TimeoutsNr);

                return gamesNr;
            }
        }
        return 0;
    }

    public int AddTimeOutRecord(int opponentID)
    {
        if (BattleRecord == null) BattleRecord = new List<Tuple<int, int, int>>();

        if (BattleRecord.Count == 0) //there are no battle records
        {
            BattleRecord.Add(new Tuple<int, int, int>(opponentID, 0, 1)); //create a new one where the first game was a timeOut    
            return 1;
        }
        else //there are battle records
        {
            Tuple<int, int, int> record = GetBattleRecordFromID(opponentID);

            if (record == null) //the battle record for that Id was not found
            {
                BattleRecord.Add(new Tuple<int, int, int>(opponentID, 0, 1));  //create a new one where the first game was a timeout     
                return 1;
            }
            else //a battle record for that ID was found
            {
                int index = BattleRecord.FindIndex(t => t.Item1 == opponentID);
                int gamesNr = record.Item2; //number of games
                int TimeoutsNr = record.Item3; //number of timeouts
                //Debug.Log($"overwriting number of games in {this.myID} vs {opponentID}: was {i}, now is {i + 1}");

                TimeoutsNr++; //increase timeouts

                BattleRecord[index] = Tuple.Create(opponentID, gamesNr, TimeoutsNr); //replace old battleRecord with the new

                return TimeoutsNr;
            }
        }
        return 0;


    }

    private Tuple<int,int, int> GetBattleRecordFromID(int ID)
    {
        foreach (var t in BattleRecord)
        {
            if (t.Item1 == ID) return t;
        }

        return null;

    }

    public int GetSucessGamesRecord(int ID)
    {
        if (BattleRecord == null) BattleRecord = new List<Tuple<int, int, int>>();

        if (BattleRecord.Count == 0)
        {
            return 0;
        }
        else
        {
            Tuple<int, int, int> t = GetBattleRecordFromID(ID);

            if (t == null) return 0;
            else return t.Item2;

        }
    }

    public int GetTimeOutRecord(int ID)
    {
        if (BattleRecord == null) BattleRecord = new List<Tuple<int, int, int>>();

        if (BattleRecord.Count == 0)
        {
            return 0;
        }
        else
        {
            Tuple<int, int, int> t = GetBattleRecordFromID(ID);

            if (t == null) return 0;
            else return t.Item3;
        }
    }



    public int GetNextOpponent(List<BattlePlayer> participants, int maxWins, int maxTimeouts)
    {
       // Debug.Log($"Checking {Name}...");

        foreach (var p in participants)
        {
            //Debug.Log($"trying to match {this.Name} to {p.name}");
            if (p.BattleData.TeamId == this.TeamId) continue;
            else
            {                
                if ((p.BattleData.GetSucessGamesRecord(this.myID) < maxWins) && (p.BattleData.GetTimeOutRecord(this.myID) < maxTimeouts)) return p.BattleData.myID;
            }
        }

        return 0;
    }

}
