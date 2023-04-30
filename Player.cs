using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

public class Player
{
    string name;
    TcpClient tcpConn;
    public bool ready = false;

    List<KeyValuePair<string, string[]>> messageQueue = new List<KeyValuePair<string, string[]>>(0);

    public Player(TcpClient tcpConn)
    {
        this.tcpConn = tcpConn;
        Task.Run(Listen);
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
        if (Read(out string[] nameMsg, "NAME"))
            name = nameMsg[0];
        return name;
    }

    public bool Write(params string[] msgParts)
    {
        try
        {
            string msg = "";
            if (msgParts.Length > 1)
            {
                msg = $"|{msgParts[0]}?";
                for (int i = 1; i < msgParts.Length; i++)
                    msg += $"{msgParts[i]}{(i < msgParts.Length-1 ? "&" : "")}";
                msg += "|";
            }
            else msg = $"|{msgParts[0]}|";

            Byte[] buffer = Encoding.UTF8.GetBytes(msg);
            tcpConn.GetStream().Write(buffer, 0, buffer.Length);
        }
        catch (Exception e) { return false; }
        return true;
    }

    public bool Read(string msgName)
    {
        for (int i = 0; i < messageQueue.Count; i++)
        {
            if (messageQueue[i].Key == msgName)
            {
                Console.WriteLine("*** " + msgName + " ***");
                messageQueue.RemoveAt(i);
                return true;
            }
        }
        return false;
    }

    public bool Read(out string[] msg, string msgName)
    {
        for (int i = 0; i < messageQueue.Count; i++)
        {
            if (messageQueue[i].Key == msgName)
            {
                msg = new string[messageQueue[i].Value.Length];
                for (int j = 0; j < messageQueue[i].Value.Length; j++)
                    msg[j] = messageQueue[i].Value[j];
                messageQueue.RemoveAt(i);
                return true;
            }
        }
        msg = new string[0];
        return false;
    }

    void Listen()
    {
        while (tcpConn.Connected)
        {
            try
            {
                Byte[] buffer = new Byte[512];
                int size = tcpConn.GetStream().Read(buffer, 0, buffer.Length);

                foreach (string receivedMsg in Encoding.UTF8.GetString(buffer).Substring(0, size).Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string[] msg = receivedMsg.Split('?');

                    if (msg.Length > 1)
                        messageQueue.Add(new KeyValuePair<string, string[]>(msg[0], msg[1].Split('&')));
                    else messageQueue.Add(new KeyValuePair<string, string[]>(msg[0], new string[] { "" }));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                break;
            }
        }
    }
}
