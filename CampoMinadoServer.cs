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
    }

    static void ListenForConnections()
    {
        while (gameRooms.Count() < maxGameRooms)
        {
            try
            {
                TcpClient player = listener.AcceptTcpClient();

                Byte[] buffer = new Byte[512];
                int size = player.GetStream().Read(buffer, 0, 512);

                string[] content = Encoding.UTF8.GetString(buffer).Substring(0, size).Split('|');

                gameRooms.Add(new GameRoom(listener, player, content[0], int.Parse(content[1])));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
