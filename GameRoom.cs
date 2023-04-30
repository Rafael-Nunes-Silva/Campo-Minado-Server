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
        if (players.Count >= maxPlayerCount)
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

        UpdatePlayers();

        return true;
    }

    public void RemovePlayer(Player player)
    {
        players.Remove(player);
        UpdatePlayers();
    }

    void UpdatePlayers()
    {
        string msg = "Jogadores:\nNome, Pronto\n";
        players.ForEach((player) => { msg += $"{player.GetName()}, {player.IsReady()}\n"; });

        players.ForEach((player) => { player.Connector().Write("PLAYERS", msg); });
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
            while (!AllReady()) { UpdatePlayers(); }
            Console.WriteLine("Todos prontos, iniciando jogo");

            StartGame();
        }
        Close();
    }
}
