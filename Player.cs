using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

public class Msg
{
    string ownerName, msgName;
    string[] content;

    public Msg(string ownerName, string msgName, string[] content)
    {
        this.ownerName = ownerName;
        this.msgName = msgName;
        this.content = content;
    }

    public string GetOwner()
    {
        return ownerName;
    }

    public string GetName()
    {
        return msgName;
    }

    public void SetContent(string[] content)
    {
        this.content = content;
    }
    public string[] GetContent()
    {
        return content;
    }
}

public class Player
{
    TcpClient tcpConn;
    List<Msg> messageQueue = new List<Msg>(0);
    // Dictionary<string, string[]> messageQueue = new Dictionary<string, string[]>(0);

    public string name = "UNKNOWN";
    public bool ready = false;

    public Player(TcpClient tcpConn)
    {
        this.tcpConn = tcpConn;


    }

    public void Disconnect()
    {
        tcpConn.Close();
    }

    bool QueueHasMessage(string msgName)
    {
        for (int i = 0; i < messageQueue.Count; i++)
        {
            if (messageQueue[i].GetName() == msgName)
                return true;
        }

        return false;
    }

    Msg FindMessageByName(string msgName)
    {
        for (int i = 0; i < messageQueue.Count; i++)
        {
            if (messageQueue[i].GetName() == msgName)
                return messageQueue[i];
        }

        return null;
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
        else msg = $"|{name}?{msgParts[0]}|";

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

    public void Receive()
    {
        string receivedStr = "";
        try
        {
            Byte[] buffer = new Byte[4096];
            int len = tcpConn.GetStream().Read(buffer, 0, buffer.Length);
            receivedStr = Encoding.UTF8.GetString(buffer).Substring(0, len);
        }
        catch (Exception e) { Console.WriteLine(e); }

        foreach (string msg in receivedStr.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
        {
            string[] splitMsg = msg.Split('?');

            string senderName = splitMsg[0],
                msgName = splitMsg[1];
            string[] msgContent = (splitMsg.Length > 2 ? splitMsg[2].Split('&') : new string[] { "" });

            if (QueueHasMessage(msgName))
                FindMessageByName(msgName).SetContent(msgContent);
            else messageQueue.Add(new Msg(senderName, msgName, msgContent));

            /*
            if (messageQueue.ContainsKey(msgName))
                messageQueue[msgName] = msgContent;
            else messageQueue.Add(msgName, msgContent);
            */
        }
    }

    public bool TryGetMessage(string msgName, out string[] content)
    {
        if (!QueueHasMessage(msgName))
        {
            content = new string[] { "" };
            return false;
        }

        Msg msg = FindMessageByName(msgName);
        content = msg.GetContent();
        messageQueue.Remove(msg);
        return true;
    }
}
