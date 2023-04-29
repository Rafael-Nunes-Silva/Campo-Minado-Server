using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

class Campo_Minado_Server
{
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

                Player newPlayer = new Player(tcpConn);

                Task.Run(new Action(() => { ManagePlayer(newPlayer); }));

                Console.WriteLine($"{newPlayer.GetName()} conectou");
            }
            catch (Exception e) { Console.WriteLine(e); }
        }
    }

    static void ManagePlayer(Player player)
    {
        while (player.IsConnected())
        {
            try
            {
                if(player.Read("DISCONNECT"))
                {
                    player.Disconnect();
                    return;
                }

                string[] msgArr;
                if (player.Read(out msgArr, "ENTER_ROOM"))
                {
                    for (int i = 0; i < gameRooms.Count; i++)
                    {
                        if (gameRooms[i].GetName() == msgArr[0])
                        {
                            if (!gameRooms[i].AddPlayer(player))
                            {
                                player.Write("SUCCESS");
                                return;
                            }
                        }
                    }
                    player.Write("FAILED");
                }
                else if (player.Read(out msgArr, "CREATE_ROOM"))
                {
                    GameRoom gameRoom = new GameRoom(msgArr[0], int.Parse(msgArr[1]), (Difficulty)int.Parse(msgArr[2]));

                    gameRooms.Add(gameRoom);
                    // gameRooms.Add(new GameRoom(listener, player.connection, content[1], int.Parse(content[2])));

                    Task.Run(new Action(() => { ManageRoom(gameRoom); }));

                    Console.WriteLine($"Sala criada por {player.GetName()}\nNome: {msgArr[0]}\nMáximo de jogadores: {msgArr[1]}\nDificuldade: {(Difficulty)int.Parse(msgArr[2])}");

                    player.Write("SUCCESS");
                }
                else if (player.Read("GET_ROOMS"))
                {
                    string msg = "Salas:\nNome, Espaço\n|";
                    for (int i = 0; i < gameRooms.Count; i++)
                        msg += $"{gameRooms[i].GetName()}, {gameRooms[i].GetPlayerCount()}/{gameRooms[i].GetMaxPlayers()}\n";
                    player.Write(msg);
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
        Console.WriteLine($"Sala {gameRoom.GetName()} aguardando jogadores...");
        while (!gameRoom.Full()) { }
        Console.WriteLine($"Sala {gameRoom.GetName()} está cheia, aguardando jogadores");

        while (!gameRoom.AllReady()) { }

        Console.WriteLine($"Sala {gameRoom.GetName()} iniciou o jogo");
        gameRoom.StartGame();
        /*
        while (gameRoom.PlayersConnected() > 0)
        {
            for (int i = 0; i < gameRoom.players.Count; i++)
            {
                // gameRoom.players[i].Write($"CONNECTED|{gameRoom.GetName()}");
            }
        }
        */
        Console.WriteLine($"A sala {gameRoom.GetName()} foi fechada");
    }
}
