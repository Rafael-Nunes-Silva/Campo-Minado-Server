using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

public class Player
{
    string name;
    TcpClient connection;

    public Player(string name, TcpClient connection)
    {
        this.name = name;
        this.connection = connection;
    }

    public void Disconnect()
    {
        connection.Close();
    }

    public bool IsConnected()
    {
        return connection.Connected;
    }

    public string GetName()
    {
        return name;
    }

    public bool Write(string msg)
    {
        try
        {
            Byte[] buffer = Encoding.UTF8.GetBytes(msg);
            connection.GetStream().Write(buffer, 0, buffer.Length);
        }
        catch (Exception e) { return false; }
        return true;
    }
    
    public string Read()
    {
        Byte[] buffer = new Byte[256];
        int size = connection.GetStream().Read(buffer, 0, buffer.Length);

        return Encoding.UTF8.GetString(buffer).Substring(0, size);
    }
}
