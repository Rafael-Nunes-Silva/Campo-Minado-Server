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
        { "1", (p, c) => { Console.WriteLine("1"); } },
        { "a", (p, c) => {

            Console.WriteLine("2");
        } },
        { "a", (p, c) => {
            Console.WriteLine("3");
        } },
        { "4", (p, c) => { Console.WriteLine("4"); } }
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

/*
public class GameRoom
{
    int maxPlayerCount;
    Difficulty difficulty;

    public List<Player> players = new List<Player>(0);

    public GameRoom(int playerCount, Difficulty difficulty)
    {
        this.maxPlayerCount = playerCount;
        this.difficulty = difficulty;

        Task.Run(ManageGame);
    }

    public void Close()
    {
        for (int i = 0; i < players.Count; i++)
            players[i].Disconnect();
        players.Clear();
    }

    public bool Full()
    {
        return players.Count == maxPlayerCount;
    }

    public int GetPlayerCount()
    {
        return players.Count;
    }

    public int GetMaxPlayerCount()
    {
        return maxPlayerCount;
    }

    public int PlayersConnected()
    {
        return players.Count;
    }

    public bool HasPlayer(Player player)
    {
        return players.Contains(player);
    }

    public bool AddPlayer(Player player)
    {
        if (players.Count >= maxPlayerCount || HasPlayer(player))
            return false;

        players.Add(player);

        player.Connector().WaitForMsg("GAMESTATUS", (gameStatus) =>
        {
            switch ((GameStatus)int.Parse(gameStatus[0]))
            {
                case GameStatus.WON:
                    Console.WriteLine($"{player.GetName()} venceu");
                    break;
                case GameStatus.LOST:
                    Console.WriteLine($"{player.GetName()} perdeu");
                    break;
            }
        }, false);
        player.Connector().WaitForMsg("GET_PLAYERS", (playersData) =>
        {
            string msg = "Jogadores:\nNome, Pronto\n";
            players.ForEach((p) => { msg += $"{p.GetName()}, {(p.IsReady() ? "Sim" : "Não")}\n"; });

            player.Connector().Write("PLAYERS", msg);
        }, false);

        return true;
    }

    public void RemovePlayer(Player player)
    {
        players.Remove(player);
    }

    bool AllReady()
    {
        foreach (Player player in players)
        {
            if (!player.IsReady())
                return false;
        }
        return true;
    }

    void StartGame()
    {
        players.ForEach((player) =>
        {
            player.Connector().Write("STARTGAME");
            player.Connector().Write("DIFFICULTY", ((int)difficulty).ToString());

            player.Connector().WaitForMsg("GAMESTATE", (status) => { Console.WriteLine((GameStatus)int.Parse(status[0])); });
        });
    }

    void ManageGame()
    {
        while (players.Count > 0)
        {
            Console.WriteLine("Sala esperando");
            while (!AllReady()) { }
            Console.WriteLine("Todos prontos, iniciando jogo");

            StartGame();
        }
        Close();
    }
}
*/