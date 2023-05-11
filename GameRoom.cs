using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

public class GameRoom
{
    static readonly Dictionary<string, Action<Player, string[]>> expectedMessages = new Dictionary<string, Action<Player, string[]>>
    {
        { "GET_PLAYERS", (p, c) => {
            string retMsg = "Players:\nNome, Pronto\n";
            players.ForEach((player) => { retMsg += $"{player.name}, {(player.ready ? "Sim" : "Não")}\n"; });
            p.Write("PLAYERS", retMsg);
        } },
        { "GAMESTATUS", (p, c) => {
            Console.WriteLine($"{p.name}: {(GameStatus)int.Parse(c[0])}");
        } }
    }; // new Dictionary<string, Action<Player>>(0);

    string name;
    int maxPlayerCount;
    Difficulty difficulty;

    public List<Player> players = new List<Player>(0);
    GameStatus gameStatus = GameStatus.NOT_PLAYING;

    bool shouldClose = false;

    public GameRoom(string name, int playerCount, Difficulty difficulty)
    {
        this.name = name;
        this.maxPlayerCount = playerCount;
        this.difficulty = difficulty;
    }

    public string GetName()
    {
        return name;
    }

    public bool AddPlayer(Player player)
    {
        if (players.Count >= maxPlayerCount)
            return false;

        players.Add(player);

        return true;
    }

    public bool GetPlayersBack(out List<Player> players) {
        if (!shouldClose)
        {
            players = new List<Player>(0);
            return false;
        }

        players = this.players;
        return true;
    }

    public int GetPlayerCount()
    {
        return players.Count;
    }

    public int GetPlayerLimit()
    {
        return maxPlayerCount;
    }

    public void RunGame()
    {
        players.ForEach((player) => { ManagePlayer(player); });
        switch (gameStatus)
        {
            case GameStatus.NOT_PLAYING:
                if (AllReady())
                {
                    gameStatus = GameStatus.PLAYING;
                    Console.WriteLine($"Sala {name} iniciando jogo");
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

    bool AllReady()
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (!players[i].ready)
                return false;
        }

        return true;
    }

    void ManagePlayer(Player player)
    {
        player.Receive();

        foreach (var expectedMsg in expectedMessages)
        {
            if (player.TryGetMessage(expectedMsg.Key, out string[] content))
                expectedMsg.Value(player, content);
        }
    }
}
