using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

public enum GameStatus
{
    NOT_PLAYING,
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

class Campo_Minado_Server
{
    static readonly Dictionary<string, Action<Player, string[]>> expectedMessages = new Dictionary<string, Action<Player, string[]>>
    {
        { "NAME", (p, c) => { p.name = c[0]; } },
        { "GET_ROOMS", (p, c) =>
        {
            string retMsg = "Salas:\nnome, lugares\n";
            lock(gameRoomsLock)
            {
                gameRooms.ForEach((gameRoom) => { retMsg += $"{gameRoom.GetName()}, {gameRoom.GetPlayerCount()}/{gameRoom.GetPlayerLimit()}\n"; });
            }
            p.Write("ROOMS", retMsg);
        } },
        { "CREATE_ROOM", (p, c) => {
            Console.WriteLine("Criando sala");

            if(RoomExists(c[0]))
                return;

            Console.WriteLine("Sala não existe");

            Console.WriteLine(gameRooms.Count);
            // lock(gameRoomsLock)
            {
                gameRooms.Add(new GameRoom(c[0], int.Parse(c[1]), (Difficulty)int.Parse(c[2])));
                Console.WriteLine("AQUI");
            }
            Console.WriteLine("CREATE_ROOM_SUCCESS");
            p.Write("CREATE_ROOM_SUCCESS");
        } },
        { "ENTER_ROOM", (p, c) => {
            if(!RoomExists(c[0]))
                return;

            if(FindRoomByName(c[0]).AddPlayer(p))
                p.Write("ENTER_ROOM_SUCCESS");
        } }
    }; // new Dictionary<string, Action<Player>>(0);

    static int maxGameRooms = 4;
    static List<GameRoom> gameRooms = new List<GameRoom>(0);
    static List<Player> players = new List<Player>(0);

    static Object gameRoomsLock = new Object(),
        playersLock = new Object();

    static TcpListener listener;

    static void Main(string[] args)
    {
        StartUp();
        Console.WriteLine("Servidor iniciou");

        Task.Run(ReceivePlayers);
        Console.WriteLine("Escutando por clientes");

        Task.Run(ManagePlayers);
        Console.WriteLine("Gerenciando jogadores");

        while (StayOpen())
        {

        }
    }

    static void StartUp()
    {
        int port = 6778;

        Console.Write("Insira a porta: ");
        try { port = int.Parse(Console.ReadLine()); }
        catch (Exception e) { }

        Console.Write("Insira o número máximo de salas: ");
        try { maxGameRooms = int.Parse(Console.ReadLine()); }
        catch (Exception e) { }

        listener = new TcpListener(System.Net.IPAddress.Any, port);
        listener.Start();
    }

    static bool StayOpen()
    {
        return true;
    }

    static void ReceivePlayers()
    {
        while (StayOpen())
        {
            try { players.Add(new Player(listener.AcceptTcpClient())); }
            catch (Exception e) { Console.WriteLine(e); }
            Console.WriteLine("Cliente recebido");
        }
    }

    static void ManagePlayers()
    {
        while (StayOpen())
        {
            lock (playersLock)
            {
                try
                {
                    players.ForEach((player) =>
                    {
                        player.Receive();

                        foreach (var expectedMsg in expectedMessages)
                        {
                            if (player.TryGetMessage(expectedMsg.Key, out string[] content))
                                expectedMsg.Value(player, content);
                        }
                    });
                }
                catch (Exception e) { }
            }
        }
    }

    static bool RoomExists(string roomName)
    {
        lock (gameRoomsLock)
        {
            for (int i = 0; i < gameRooms.Count; i++)
            {
                if (gameRooms[i].GetName() == roomName)
                    return true;
            }
        }
        return false;
    }

    static GameRoom FindRoomByName(string roomName)
    {
        lock (gameRoomsLock)
        {
            for (int i = 0; i < gameRooms.Count; i++)
            {
                if (gameRooms[i].GetName() == roomName)
                    return gameRooms[i];
            }
        }
        return null;
    }
}
