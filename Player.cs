using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;

public class Player
{
    TcpClient tcpConn;
    Dictionary<string, string[]> messageQueue = new Dictionary<string, string[]>(0);
    Object messageQueueLock = new Object();

    public string name = "UNKNOWN";
    public bool ready = false;

    public Player(TcpClient tcpConn)
    {
        this.tcpConn = tcpConn;

        Task.Run(Listen);

        WaitForMsg("NAME", (content) => { name = content[0]; });
        WaitForMsg("READY", (content) => { ready = bool.Parse(content[0]); }, true);
    }

    public void Disconnect()
    {
        tcpConn.Close();
    }

    public bool IsConnected()
    {
        return tcpConn.Connected;
    }

    public void WaitForMsg(string msgName, Action<string[]> receiveCallback, bool repeat = false)
    {
        Task.Run(() =>
        {
            while (tcpConn.Connected)
            {
                if (Read(msgName, out string[] strArr))
                {
                    receiveCallback(strArr);
                    if (!repeat)
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

    public bool Read(string msgName, int maxWaitTime = 10000)
    {
        DateTime start = DateTime.Now;
        do
        {
            lock (messageQueueLock)
            {
                if (messageQueue.ContainsKey(msgName))
                {
                    messageQueue.Remove(msgName);
                    return true;
                }
            }
            Thread.Sleep(100);
        } while ((DateTime.Now - start).TotalMilliseconds < maxWaitTime);
        return false;
    }

    public bool Read(string msgName, out string[] content, int maxWaitTime = 10000)
    {
        DateTime start = DateTime.Now;
        do
        {
            lock (messageQueueLock)
            {
                if (messageQueue.ContainsKey(msgName))
                {
                    content = messageQueue[msgName];
                    messageQueue.Remove(msgName);
                    return true;
                }
            }
            Thread.Sleep(100);
        } while ((DateTime.Now - start).TotalMilliseconds < maxWaitTime);
        content = new string[0];
        return false;
    }

    void Listen()
    {
        while (tcpConn.Connected)
        {
            Byte[] buffer = new Byte[4096];
            int size;
            try { size = tcpConn.GetStream().Read(buffer, 0, buffer.Length); }
            catch (Exception e)
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

            // Thread.Sleep(100);
        }
    }
}
