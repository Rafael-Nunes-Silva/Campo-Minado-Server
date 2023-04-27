using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

public class GameRoom
{
    string name = "Default room name";
    int maxPlayers = 2;

    public List<Player> players = new List<Player>(0);
    TcpListener listener;

    public GameRoom(TcpListener listener, Player player, string name, int maxPlayers)
    {
        this.listener = listener;
        this.name = name;
        this.maxPlayers = maxPlayers;

        players.Add(player);
    }

    public void Close()
    {
        for (int i = 0; i < players.Count; i++)
            players[i].connection.Close();
        players.Clear();
    }

    public string GetName()
    {
        return name;
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
