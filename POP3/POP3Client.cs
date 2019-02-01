using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Timers;
using System.Xml;

namespace POP3
{
    class POP3Client
    {
        private Timer updateTimer;
        private string login;
        private string pass;
        private string address;
        private int port;
        private int refreshTime;
        private Dictionary<string, string> uidlDict;
        private int receivedMails;

        public POP3Client()
        {
            XmlReader xmlReader = XmlReader.Create("App.config");
            xmlReader.MoveToContent();
            while (xmlReader.Read())
            {
                if (xmlReader.NodeType == XmlNodeType.Element)
                {
                    if (xmlReader.Name == "address")
                        address = xmlReader.ReadElementString();
                    else if (xmlReader.Name == "user")
                        login = xmlReader.ReadElementString();
                    else if (xmlReader.Name == "password")
                        pass = xmlReader.ReadElementString();
                    else if (xmlReader.Name == "port")
                        port = Int32.Parse(xmlReader.ReadElementString());
                    else if (xmlReader.Name == "refreshTime")
                        refreshTime = Int32.Parse(xmlReader.ReadElementString()) * 1000;
                }
            }

            updateTimer = new Timer(refreshTime);
            updateTimer.Elapsed += updateMails;
            updateTimer.AutoReset = true;
            updateTimer.Enabled = false;
            uidlDict = new Dictionary<string, string>();
            receivedMails = 0;
        }

        private void updateMails(object sender, ElapsedEventArgs e)
        {
            using (TcpClient tcpClient = new TcpClient())
            {
                tcpClient.Connect(address, port);
                using (StreamReader sr = new StreamReader(tcpClient.GetStream()))
                {
                    using (StreamWriter sw = new StreamWriter(tcpClient.GetStream()))
                    {
                        string responseLine = string.Empty;
                        responseLine = sr.ReadLine();
                        sw.WriteLine("USER " + login);
                        sw.Flush();
                        responseLine = sr.ReadLine();
                        if (responseLine[0] != '+')
                            throw new Exception("Wystapil blad przy ustanawianiu polaczenia.\n");
                        sw.WriteLine("PASS " + pass);
                        sw.Flush();
                        responseLine = sr.ReadLine();
                        if (responseLine[0] != '+')
                            throw new Exception("Wystapil blad przy ustanawianiu polaczenia.\n");

                        sw.WriteLine("UIDL");
                        sw.Flush();

                        string[] nr_uidl;
                        Dictionary<string, string> tempUidlDict = new Dictionary<string, string>();
                        while ((responseLine = sr.ReadLine()) != ".")
                        {
                            if (responseLine == "." || responseLine.IndexOf("-ERR") != -1)
                                break;

                            if (responseLine[0] != '+')
                            {
                                nr_uidl = responseLine.Split(' ');
                                tempUidlDict[nr_uidl[1]] = nr_uidl[0];
                            }
                        }


                        foreach (string uidl in tempUidlDict.Keys)
                        {
                            if (!uidlDict.Keys.Contains(uidl))
                            {
                                receivedMails++;
                                Console.WriteLine("Dostales nowa wiadomosc!");
                                string msg = "RETR " + tempUidlDict[uidl];
                                sw.WriteLine(msg);
                                sw.Flush();
                                while ((responseLine = sr.ReadLine()) != null)
                                {
                                    if (responseLine == "." || responseLine.IndexOf("-ERR") != -1)
                                        break;

                                    if (responseLine.Contains("Subject"))
                                        Console.WriteLine("Tytul: " + responseLine.Split(' ')[1] + "\n");
                                }
                            }
                        }
                        uidlDict = tempUidlDict;

                        sw.WriteLine("QUIT");
                        sw.Flush();
                    }
                }
            }
        }

        public void runClient()
        {
            using (TcpClient tcpClient = new TcpClient(address, port))
            {
                using (StreamReader sr = new StreamReader(tcpClient.GetStream()))
                {
                    using (StreamWriter sw = new StreamWriter(tcpClient.GetStream()))
                    {
                        string responseLine = string.Empty;
                        responseLine = sr.ReadLine();

                        sw.WriteLine("USER " + login);
                        sw.Flush();
                        responseLine = sr.ReadLine();
                        if (responseLine[0] != '+')
                            throw new Exception("Wystapil blad przy ustanawianiu polaczenia.\n");
                        sw.WriteLine("PASS " + pass);
                        sw.Flush();
                        responseLine = sr.ReadLine();
                        if (responseLine[0] != '+')
                            throw new Exception("Wystapil blad przy ustanawianiu polaczenia.\n");
                        sw.WriteLine("UIDL");
                        sw.Flush();

                        string[] nr_uidl;
                        while ((responseLine = sr.ReadLine()) != null)
                        {
                            if (responseLine == "." || responseLine.IndexOf("-ERR") != -1)
                                break;

                            if (responseLine[0] != '+')
                            {
                                nr_uidl = responseLine.Split(' ');
                                uidlDict[nr_uidl[1]] = nr_uidl[0];
                            }
                        }
                        sw.WriteLine("QUIT");
                        sw.Flush();
                    }
                }
            }

            updateTimer.Start();
        }

        public void stopClient()
        {
            updateTimer.Stop();
            updateTimer.Dispose();

            Console.WriteLine("Podczas dzialania klienta otrzymales " + receivedMails + " maili.\n");
        }
    }
}
