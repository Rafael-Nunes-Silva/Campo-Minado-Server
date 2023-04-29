using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

public enum GameStatus
{
    PLAYING,
    WON,
    LOST
}

public enum Difficulty
{
    EASY = 0,
    NORMAL = 1,
    HARD = 2
}

public class GameRoom
{
    string name;
    int maxPlayers;
    Difficulty difficulty;

    public List<Player> players = new List<Player>(0);
    // public Dictionary<Player, GameData> players = new Dictionary<Player, GameData>(0);

    public GameRoom(string name, int maxPlayers, Difficulty difficulty)
    {
        this.name = name;
        this.maxPlayers = maxPlayers;
        this.difficulty = difficulty;
    }

    public void Close()
    {
        for (int i = 0; i < players.Count; i++)
            players[i].Disconnect();
        players.Clear();
    }

    public string GetName()
    {
        return name;
    }

    public bool Full()
    {
        return players.Count == maxPlayers;
    }

    public int GetPlayerCount()
    {
        return players.Count;
    }

    public int GetMaxPlayers()
    {
        return maxPlayers;
    }

    public int PlayersConnected()
    {
        return players.Count;
    }

    public bool AddPlayer(Player player)
    {
        if (players.Count >= maxPlayers)
            return false;
        players.Add(player);
        Console.WriteLine($"{player.GetName()} conectou a sala {name}");
        return true;
    }

    void ManagePlayer(Player player)
    {
        while (player.IsConnected())
        {
            try
            {
                if (player.Read("LEAVE_ROOM"))
                {
                    player.Disconnect();
                    return;
                }
                
                string[] msgArr;
                if (player.Read(out msgArr, "READY"))
                {
                    player.ready = bool.Parse(msgArr[0]);
                }
                else if (player.Read(out msgArr, "GAMESTATUS"))
                {
                    switch ((GameStatus)int.Parse(msgArr[0]))
                    {
                        case GameStatus.WON:
                            Console.WriteLine($"{player.GetName()} venceu");
                            break;
                        case GameStatus.LOST:
                            Console.WriteLine($"{player.GetName()} perdeu");
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{player.GetName()} desconectou");
                break;
            }
        }
    }

    public bool AllReady()
    {
        foreach (Player player in players)
        {
            if (!player.ready)
                return false;
        }
        return true;
    }

    public void StartGame()
    {

    }
}
