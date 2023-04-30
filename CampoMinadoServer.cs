using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

class Campo_Minado_Server
{
    static int maxGameRooms = 4;
    static Dictionary<string, GameRoom> gameRooms = new Dictionary<string, GameRoom>(0);
    static List<Player> players = new List<Player>(0);

    static TcpListener listener;
    static bool running = true;

    static void Main(string[] args)
    {
        int port = 6778;

        /*
        Console.Write("Insira a porta: ");
        try { port = int.Parse(Console.ReadLine()); }
        catch (Exception e) { }

        Console.Write("Insira o número máximo de salas: ");
        try { maxGameRooms = int.Parse(Console.ReadLine()); }
        catch (Exception e) { }
        */

        listener = new TcpListener(System.Net.IPAddress.Any, port);

        Task.Run(ListenForConnections);

        while (running)
        {
            // Console.Clear();

            Console.WriteLine("0 - Fechar servidor");
            Console.WriteLine("1 - Mostrar salas");
            Console.WriteLine("2 - Fechar sala");
            Console.WriteLine("3 - Mostrar jogadores");
            Console.WriteLine("4 - Desconectar jogador");
            Console.Write(": ");

            try
            {
                int input = int.Parse(Console.ReadLine());
                string str = "";

                switch (input)
                {
                    case 0:
                        running = false;
                        break;
                    case 1:
                        ShowRooms();
                        break;
                    case 2:
                        Console.Write("Insira o nome da sala: ");
                        str = Console.ReadLine();

                        CloseRoom(str);
                        break;
                    case 3:
                        ShowPlayers();
                        break;
                    case 4:
                        Console.Write("Insira o nome do jogador: ");
                        str = Console.ReadLine();

                        DisconnectPlayer(str);
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                continue;
            }
        }

        CloseRooms();
        DisconnectPlayers();
    }

    static void ListenForConnections()
    {
        listener.Start();
        while (running)
        {
            Player newPlayer;
            try { newPlayer = new Player(listener.AcceptTcpClient()); }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }

            players.Add(newPlayer);

            newPlayer.Connector().WaitForMsg("DISCONNECT", (disconnect) =>
            {
                newPlayer.Disconnect();
                players.Remove(newPlayer);
            });
            newPlayer.Connector().WaitForMsg("CREATE_ROOM", (roomData) =>
            {
                if (CreateRoom(roomData[0], int.Parse(roomData[1]), (Difficulty)int.Parse(roomData[2])))
                    newPlayer.Connector().Write("CREATE_ROOM_SUCCESS");
                UpdatePlayers();
            }, false);
            newPlayer.Connector().WaitForMsg("ENTER_ROOM", (roomName) =>
            {
                if (gameRooms.ContainsKey(roomName[0]))
                {
                    if (gameRooms[roomName[0]].AddPlayer(newPlayer))
                        newPlayer.Connector().Write("ENTER_ROOM_SUCCESS");
                }
            }, false);
            newPlayer.Connector().WaitForMsg("LEAVE_ROOM", (roomName) =>
            {
                foreach(var room in gameRooms)
                {
                    if (room.Value.HasPlayer(newPlayer))
                        room.Value.RemovePlayer(newPlayer);
                }
            }, false);
        }
    }

    static void UpdatePlayers()
    {
        string msg = "Salas:\nNome, Vagas\n";
        foreach (var room in gameRooms)
            msg += $"{room.Key}, {room.Value.GetPlayerCount()}/{room.Value.GetMaxPlayerCount()}\n";

        players.ForEach((player) => { player.Connector().Write("ROOMS", msg); });
    }

    static bool CreateRoom(string name, int playerCount, Difficulty difficulty)
    {
        if (gameRooms.ContainsKey(name))
            return false;

        gameRooms.Add(name, new GameRoom(playerCount, difficulty));

        UpdatePlayers();
        return true;
    }

    static void ShowRooms()
    {
        Console.WriteLine("Salas:");
        foreach (var room in gameRooms)
            Console.WriteLine(room.Key);
    }

    static void CloseRoom(string name)
    {
        gameRooms[name].Close();
        gameRooms.Remove(name);
    }

    static void CloseRooms()
    {
        foreach (var room in gameRooms)
            room.Value.Close();
        gameRooms.Clear();
    }

    static void ShowPlayers()
    {
        Console.WriteLine("Jogadores:");
        foreach (var player in players)
            Console.WriteLine(player.GetName());
    }

    static void DisconnectPlayer(string name)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].GetName() == name)
            {
                players[i].Disconnect();
                players.RemoveAt(i);
            }
        }
    }

    static void DisconnectPlayers()
    {
        players.ForEach((player) => { player.Disconnect(); });
        players.Clear();
    }
}
