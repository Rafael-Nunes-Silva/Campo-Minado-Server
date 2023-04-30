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
    List<KeyValuePair<string, string[]>> messageQueue = new List<KeyValuePair<string, string[]>>(0);
    Mutex messageQueueMut = new Mutex();

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
                if (Read(out string[] strArr, msg))
                {
                    receiveCallback(strArr);
                    if (once)
                        return;
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

    bool Read(out string[] msg, string msgName)
    {
        if (messageQueueMut.WaitOne())
        {
            for (int i = 0; i < messageQueue.Count; i++)
            {
                try
                {
                    if (messageQueue[i].Key == msgName)
                    {
                        msg = new string[messageQueue[i].Value.Length];
                        for (int j = 0; j < messageQueue[i].Value.Length; j++)
                            msg[j] = messageQueue[i].Value[j];
                        messageQueue.RemoveAt(i);
                        messageQueueMut.ReleaseMutex();
                        return true;
                    }
                }
                catch (Exception e) { break; }
            }
            messageQueueMut.ReleaseMutex();
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

                if (messageQueueMut.WaitOne())
                {
                    if (msg.Length > 1)
                        messageQueue.Add(new KeyValuePair<string, string[]>(msg[0], msg[1].Split('&')));
                    else messageQueue.Add(new KeyValuePair<string, string[]>(msg[0], new string[] { "" }));
                    messageQueueMut.ReleaseMutex();
                }
            }
            Thread.Sleep(1000);
        }
    }
}
