using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;

public class Connector
{
    TcpClient tcpConn;
    static Dictionary<string, string[]> messageQueue = new Dictionary<string, string[]>(0);
    Object messageQueueLock = new Object();

    public Connector(TcpClient tcpConn)
    {
        this.tcpConn = tcpConn;

        Task.Run(Listen);
    }

    public void WaitForMsg(string msg, Action<string[]> receiveCallback, bool once=true)
    {
        Task.Run(() =>
        {
            while (tcpConn.Connected)
            {
                Thread.Sleep(1000);
                if (Read(out string[] strArr, msg))
                {
                    receiveCallback(strArr);
                    if (once)
                        break;
                }
            }
        });
    }

    public bool Write(params string[] msgParts)
    {
        string msg = "";
        if (msgParts.Length > 1)
        {
            msg = $"|{msgParts[0]}?";
            for (int i = 1; i < msgParts.Length; i++)
                msg += $"{msgParts[i]}{(i < msgParts.Length - 1 ? "&" : "")}";
            msg += "|";
        }
        else msg = $"|{msgParts[0]}|";

        try
        {
            Byte[] buffer = Encoding.UTF8.GetBytes(msg);
            tcpConn.GetStream().Write(buffer, 0, buffer.Length);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    bool Read(out string[] msg, string msgName, int waitTime = 1000)
    {
        DateTime start = DateTime.Now;
        while ((DateTime.Now - start).TotalMilliseconds <= waitTime) { }

        lock (messageQueueLock)
        {
            if (messageQueue.ContainsKey(msgName))
            {
                msg = messageQueue[msgName];
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
            Byte[] buffer = new Byte[512];
            int size;
            try
            {
                size = tcpConn.GetStream().Read(buffer, 0, buffer.Length);
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                break;
            }

            foreach (string receivedMsg in Encoding.UTF8.GetString(buffer).Substring(0, size).Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] msg = receivedMsg.Split('?');

                lock (messageQueueLock)
                {
                    if (messageQueue.ContainsKey(msg[0]))
                        messageQueue[msg[0]] = (msg.Length > 1 ? msg[1].Split('&') : new string[] { "" });
                    else messageQueue.Add(msg[0], (msg.Length > 1 ? msg[1].Split('&') : new string[] { "" }));
                }
            }
        }
    }
}
