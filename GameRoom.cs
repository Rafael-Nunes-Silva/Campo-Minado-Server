using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

public class GameRoom
{
    string name;
    int maxPlayerCount;
    Difficulty difficulty;

    List<Player> players = new List<Player>(0);
    Object playersLock = new Object();
    GameStatus gameStatus = GameStatus.NOT_PLAYING;

    bool shouldClose = false;

    public GameRoom(string name, int playerCount, Difficulty difficulty)
    {
        this.name = name;
        this.maxPlayerCount = playerCount;
        this.difficulty = difficulty;

        Task.Run(RunGame);

        Console.WriteLine($"Sala {name} aberta");
    }

    public string GetName()
    {
        return name;
    }

    public bool HasPlayer(Player player)
    {
        for(int i = 0; i < players.Count; i++)
        {
            if (players[i] == player)
                return true;
        }
        return false;
    }

    public bool AddPlayer(Player player)
    {
        lock (playersLock)
        {
            if (players.Count >= maxPlayerCount)
                return false;

            if (HasPlayer(player))
                return false;

            players.Add(player);
        }

        player.WaitForMsg("LEAVE_ROOM", (content) =>
        {
            lock (playersLock)
            {
                players.Remove(player);
            }
        });
        player.WaitForMsg("GET_PLAYERS", (content) =>
        {
            string retMsg = "Players:\nNome, Pronto\n";
            lock (playersLock)
            {
                players.ForEach((p) => { retMsg += $"{p.name}, {(p.ready ? "Sim" : "Não")}\n"; });
            }
            player.Write("PLAYERS", retMsg);
        }, true);
        player.WaitForMsg("GAMESTATUS", (content) =>
        {
            Console.WriteLine($"{player.name}: {(GameStatus)int.Parse(content[0])}");
        }, true);

        return true;
    }

    public bool GetPlayersBack(out List<Player> players) {
        if (!shouldClose)
        {
            players = new List<Player>(0);
            return false;
        }
        lock (playersLock)
        {
            players = this.players;
        }
        return true;
    }

    public int GetPlayerCount()
    {
        lock (playersLock)
        {
            return players.Count;
        }
    }

    public int GetPlayerLimit()
    {
        return maxPlayerCount;
    }

    public void RunGame()
    {
        while (true)
        {
            switch (gameStatus)
            {
                case GameStatus.NOT_PLAYING:
                    lock (playersLock)
                    {
                        if (players.Count == maxPlayerCount && AllReady())
                        {
                            Console.WriteLine($"Sala {name} iniciando jogo");

                            gameStatus = GameStatus.PLAYING;
                            players.ForEach((player) =>
                            {
                                player.Write("STARTGAME", ((int)difficulty).ToString(), DateTime.Now.Millisecond.ToString());
                            });
                        }
                    }
                    break;
                case GameStatus.PLAYING:

                    break;
                case GameStatus.WON:

                    break;
                case GameStatus.LOST:

                    break;
            }
        }
    }

    bool AllReady()
    {
        lock (playersLock)
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (!players[i].ready)
                    return false;
            }
        }
        return true;
    }
}
