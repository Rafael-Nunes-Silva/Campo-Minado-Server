using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

public class Player
{
    TcpClient tcpConn;
    Connector connector;

    string name = "UNKNOWN";
    bool ready = false;

    public Player(TcpClient tcpConn)
    {
        this.tcpConn = tcpConn;
        connector = new Connector(tcpConn);

        connector.WaitForMsg("NAME", (name) => { this.name = name[0]; });
        connector.WaitForMsg("READY", (ready) => { this.ready = bool.Parse(ready[0]); }, false);
    }

    public Connector Connector()
    {
        return connector;
    }

    public void Disconnect()
    {
        tcpConn.Close();
    }

    public bool IsConnected()
    {
        return tcpConn.Connected;
    }

    public string GetName()
    {
        return name;
    }

    public bool IsReady()
    {
        return ready;
    }
}
