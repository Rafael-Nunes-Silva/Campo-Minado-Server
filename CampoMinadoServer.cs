using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

class Campo_Minado_Server
{
    public static readonly string SUCCESS_MSG = "SUCCESS";
    public static readonly string FAILED_MSG = "FAILED";

    static int maxGameRooms = 4;
    static List<GameRoom> gameRooms = new List<GameRoom>(0);

    static TcpListener listener;

    static void Main(string[] args)
    {
        int port = 6778;

        Console.Write("Insira a porta: ");
        try { port = int.Parse(Console.ReadLine()); }
        catch (Exception e) { }

        Console.Write("Insira o número máximo de salas: ");
        try { maxGameRooms = int.Parse(Console.ReadLine()); }
        catch (Exception e) { }

        listener = new TcpListener(System.Net.IPAddress.Any, port);

        Task.Run(ListenForConnections);

        bool running = true;
        while (running)
        {
            switch (Console.ReadLine().ToLower())
            {
                case "-close_server":
                    running = false;
                    break;
            }
        }

        for(int i=0; i<gameRooms.Count;i++)
            gameRooms[i].Close();
    }

    static void ListenForConnections()
    {
        listener.Start();
        while (gameRooms.Count() < maxGameRooms)
        {
            try
            {
                TcpClient tcpConn = listener.AcceptTcpClient();

                Byte[] buffer = new Byte[128];
                int size = tcpConn.GetStream().Read(buffer, 0, buffer.Length);

                string name = Encoding.UTF8.GetString(buffer).Substring(0, size);

                Task.Run(new Action(() => { ManagePlayer(new Player(name, tcpConn)); }));

                Console.WriteLine($"{name} conectou");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            /* create room
            try
            {
                TcpClient player = listener.AcceptTcpClient();

                Byte[] buffer = new Byte[512];
                int size = player.GetStream().Read(buffer, 0, 512);

                string[] content = Encoding.UTF8.GetString(buffer).Substring(0, size).Split('|');

                gameRooms.Add(new GameRoom(listener, player, content[0], int.Parse(content[1])));

                Console.WriteLine($"Sala criada\nNome: {content[0]}\nMáximo de jogadores: {content[1]}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            */
        }
    }

    static void ManagePlayer(Player player)
    {
        while (player.connection.Connected)
        {
            try
            {
                string[] content = player.Read().Split('|');

                switch (content[0])
                {
                    case "DISCONNECT":
                        player.connection.Close();
                        break;
                    case "CONNECT":
                        for (int i = 0; i < gameRooms.Count; i++)
                        {
                            if (gameRooms[i].GetName() == content[1])
                            {
                                if (!gameRooms[i].AddPlayer(player))
                                {
                                    player.Write(SUCCESS_MSG);
                                    break;
                                }
                                player.Write(FAILED_MSG);
                                break;
                            }
                        }
                        break;
                    case "CREATE_ROOM":
                        GameRoom gameRoom = new GameRoom(listener, player, content[1], int.Parse(content[2]));

                        gameRooms.Add(gameRoom);
                        // gameRooms.Add(new GameRoom(listener, player.connection, content[1], int.Parse(content[2])));

                        Task.Run(new Action(() => { ManageRoom(gameRoom); }));

                        Console.WriteLine($"Sala criada por {player.GetName()}\nNome: {content[1]}\nMáximo de jogadores: {content[2]}");

                        player.Write(SUCCESS_MSG);
                        break;
                    case "GET_ROOMS":
                        string msg = "Salas:\nNome | Espaço\n";
                        for(int i=0; i < gameRooms.Count; i++)
                        {
                            msg += $"{gameRooms[i].GetName()} | {gameRooms[i].GetPlayerCount()}/{gameRooms[i].GetMaxPlayers()}\n";
                        }
                        player.Write(msg);
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{player.GetName()} desconectou");
                break;
            }
        }
    }

    static void ManageRoom(GameRoom gameRoom)
    {
        while (gameRoom.PlayersConnected() > 0)
        {
            for (int i = 0; i < gameRoom.players.Count; i++)
            {
                // gameRoom.players[i].Write($"CONNECTED|{gameRoom.GetName()}");
            }
        }
        Console.WriteLine($"A sala {gameRoom.GetName()} foi fechada");
    }
}
