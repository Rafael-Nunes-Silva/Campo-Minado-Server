using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

public class Player
{
    string name;
    TcpClient connection;

    public Player(string name, TcpClient connection)
    {
        this.name = name;
        this.connection = connection;
    }


}

public class GameRoom
{
    string name = "Default room name";
    int maxPlayers = 2;

    List<Player> players = new List<Player>(0);
    TcpListener listener;

    public GameRoom(TcpListener listener, TcpClient player, string name, int maxPlayers)
    {
        this.listener = listener;
        this.name = name;
        this.maxPlayers = maxPlayers;
    }

    public string GetName()
    {
        return name;
    }

    public int GetMaxPlayers()
    {
        return maxPlayers;
    }
}
