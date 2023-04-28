using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

/*
public struct GameData {
    int flags;
    public GameData(int flags)
    {
        this.flags = flags;
    }
}
*/

public class GameRoom
{
    string name = "Default room name";
    int maxPlayers = 2;

    public List<Player> players = new List<Player>(0);
    // public Dictionary<Player, GameData> players = new Dictionary<Player, GameData>(0);

    public GameRoom(string name, int maxPlayers)
    {
        this.name = name;
        this.maxPlayers = maxPlayers;
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
}
