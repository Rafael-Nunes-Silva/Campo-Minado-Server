using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

        while (StayOpen())
        {
            lock (gameRoomsLock)
            {
                for (int i = 0; i < gameRooms.Count; i++)
                {
                    if (gameRooms[i].shouldClose)
                    {
                        Console.WriteLine($"A sala {gameRooms[i].GetName()} foi fechada");
                        gameRooms.Remove(gameRooms[i]);
                        i--;
                    }
                }
            }
            Thread.Sleep(10000);
        }

        listener.Stop();
    }

    static void StartUp()
    {
        int port = 6778;
        
        Console.Write("Insira a porta: ");
        try { port = int.Parse(Console.ReadLine()); }
        catch (Exception e) { Console.WriteLine(e); }

        Console.Write("Insira o número máximo de salas: ");
        try { maxGameRooms = int.Parse(Console.ReadLine()); }
        catch (Exception e) { Console.WriteLine(e); }

        listener = new TcpListener(System.Net.IPAddress.Any, port);
        listener.Start();
        Task.Run(ReceivePlayer);
    }

    static bool StayOpen()
    {
        return true;
    }

    static void ReceivePlayer()
    {
        while (true)
        {
            Player newPlayer = null;
            try { newPlayer = new Player(listener.AcceptTcpClient()); }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }

            lock (playersLock)
            {
                players.Add(newPlayer);
            }

            newPlayer.WaitForMsg("GET_ROOMS", (content) =>
            {
                string retMsg = "Salas:\nnome, lugares\n";
                lock (gameRoomsLock)
                {
                    gameRooms.ForEach((gameRoom) => { retMsg += $"{gameRoom.GetName()}, {gameRoom.GetPlayerCount()}/{gameRoom.GetPlayerLimit()}\n"; });
                }
                newPlayer.Write("ROOMS", retMsg);
            }, true);
            newPlayer.WaitForMsg("CREATE_ROOM", (content) =>
            {
                if (RoomExists(content[0]))
                    return;

                lock (gameRoomsLock)
                {
                    gameRooms.Add(new GameRoom(content[0], int.Parse(content[1]), (Difficulty)int.Parse(content[2])));
                }
                newPlayer.Write("CREATE_ROOM_SUCCESS");
            }, true);
            newPlayer.WaitForMsg("ENTER_ROOM", (content) =>
            {
                if (!RoomExists(content[0]))
                    return;

                if (FindRoomByName(content[0]).AddPlayer(newPlayer))
                    newPlayer.Write("ENTER_ROOM_SUCCESS");
            }, true);
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
