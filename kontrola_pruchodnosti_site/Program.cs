using System;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;

internal class Program
{
    private static void Main(string[] args)
    {
        try //načítání vstupů od uživatele
        {
            Console.WriteLine("Pro vysílání napište 1, pro naslouchání 2: ");
            string mode = Console.ReadLine();
            while (mode != "1" && mode != "2") //zajistění správného vstupu módu
            {
                Console.WriteLine("Neplatný vstup. Zadejte 1 nebo 2: ");
                mode = Console.ReadLine();
            }

            Console.WriteLine("Zvolte protokol (TCP/UDP), pro oba napistě 2: ");
            string protocol = Console.ReadLine().ToUpper();
            while (protocol != "TCP" && protocol != "UDP" && protocol != "2") //zajistění správného vstupu u protocolu
            {
                Console.WriteLine("Neplatný protokol. Zadejte TCP nebo UDP: ");
                protocol = Console.ReadLine().ToUpper();
            }

            Console.WriteLine("Zadejte port nebo interval (a-b): ");
            int[] port = GetValidPort(); //zavolání metody pro validaci portu

            if (mode == "1")
            {
                Console.WriteLine("Zadejte IP adresu: ");
                string ip = GetValidIPAddress(); //zavolání metody pro validaci ip adresy
                Console.WriteLine("Zadejte periodu v sekundách (výchozí 5): ");
                int period = GetValidNumber(5); //zavolání metody pro validaci čísla - periody
                Sending(protocol, port, ip, period); //zavolání procedury pro vysílání
            }
            else
            {
                Listening(protocol, port); //zavolání procedury pro poslouchání
            }
        }
        catch (Exception ex)
        {
            LogMessage("Neočekávaná chyba: " + ex.Message);
            Console.WriteLine("Neočekávaná chyba: " + ex.Message);
        }
    }

    //procedura určená k vysílání
    static void Sending(string protocol, int[] port, string ip, int period)
    {
        try
        {   //vysílání pomoci TCP
            if (protocol == "TCP")
            {
                while (true)
                {
                    TcpSending(port, ip, period);   
                }
            }
            else if (protocol == "UDP")
            {
                while (true)
                {
                    UdpSending(port, ip, period);
                }
            } 
            else
            {
                while (true)
                {
                    TcpSending(port, ip, period);
                    UdpSending(port, ip, period);
                }
            }

        }
        catch (SocketException ex)
        {
            Console.WriteLine("Chyba připojení: " + ex.Message);
        }
    }



    //procedura určená k poslouchání
    static void Listening(string protocol, int[] port) 
    {
        try
        {   //poslouchání pomocí TCP
            if (protocol == "TCP")
            {
                TcpListener listener = new TcpListener(IPAddress.Any, port); //inicializuje TCP listener, který poslouchá jakoukoli IP adresu na daném portu
                listener.Start();
                Console.WriteLine("Poslouchám (TCP)...");
                LogMessage("Poslouchám (TCP)...");

                while (true)
                {
                    using (TcpClient client = listener.AcceptTcpClient()) //akceptuje žádost o připojení
                    using (NetworkStream stream = client.GetStream())
                    {
                        byte[] buffer = new byte[1024];
                        int bytesRead = stream.Read(buffer, 0, buffer.Length); //čte příjmutá data
                        string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead); //převede data do stringu
                        string message = "Přijatá zpráva: " + receivedData;
                        Console.WriteLine(message);
                        LogMessage(message);
                    }
                }
            } //poslouchání pomocí UDP
            else if (protocol == "UDP")
            {   
                using (UdpClient udpListener = new UdpClient(port)) //inicializuje UDP listener, který poslouchá na daném portu
                {
                    Console.WriteLine("Poslouchám (UDP)...");
                    LogMessage("Poslouchám (UDP)...");
                    while (true)
                    {
                        IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port); //inicializace endpointu
                        byte[] data = udpListener.Receive(ref endPoint); //vrací příjmutá data
                        string receivedData = Encoding.UTF8.GetString(data); //převede data do stringu
                        string message = "Přijatá zpráva: " + receivedData;
                        Console.WriteLine(message);
                        LogMessage(message);
                    }
                }
            }
            else
            {
                TcpListener listener = new TcpListener(IPAddress.Any, port); //inicializuje TCP listener, který poslouchá jakoukoli IP adresu na daném portu
                listener.Start();
                Console.WriteLine("Poslouchám (TCP/UDP)...");
                LogMessage("Poslouchám (TCP/UDP)...");

                while (true)
                {
                    using (UdpClient udpListener = new UdpClient(port)) //inicializuje UDP listener, který poslouchá na daném portu
                    using (TcpClient client = listener.AcceptTcpClient()) //akceptuje žádost o připojení
                    using (NetworkStream stream = client.GetStream())
                    {
                        byte[] buffer = new byte[1024];
                        int bytesRead = stream.Read(buffer, 0, buffer.Length); //čte příjmutá data
                        string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead); //převede data do stringu
                        string message = "Přijatá zpráva (TCP): " + receivedData;
                        Console.WriteLine(message);
                        LogMessage(message);

                        IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port); //inicializace endpointu
                        byte[] data = udpListener.Receive(ref endPoint); //vrací příjmutá data
                        string receivedData1 = Encoding.UTF8.GetString(data); //převede data do stringu
                        string message1 = "Přijatá zpráva (UDP): " + receivedData1;
                        Console.WriteLine(message1);
                        LogMessage(message1);

                    }
                }
            }
        }
        catch (SocketException ex)
        {
            LogMessage("Chyba sítě: " + ex.Message);
            Console.WriteLine("Chyba sítě: " + ex.Message);
        }
    }
    //validace čísel - inputu
    static int GetValidNumber(int defaultValue = -1)
    {
        int result;
        while (true)
        {
            string input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input) && defaultValue != -1) //jestli uživatel nezadá vstup, tak bude vrácena výchozí hodnota
            {
                return defaultValue;
            }
            try    //kontrola zda se jedná o číslo větší než 0
            {
                result = int.Parse(input);
                if (result > 0)
                {
                    return result;
                }
            }
            catch
            {
                Console.WriteLine("Neplatné číslo, zkuste to znovu.");
            }
        }
    }

    static int[] GetValidPort()
    {
        int[] result = new int[2];

        while (true)
        {
            string[] input = Console.ReadLine().Split("-");

            if (input.Length == 1)
            {
                try
                {
                    result[0] = int.Parse(input[0]);
                    return result;

                }
                catch
                {
                    Console.WriteLine("Neplatné číslo, zkuste to znovu.");
                }
            }
            else if (input.Length == 2)
            {
                try
                {
                    result[0] = int.Parse(input[0]);
                    result[1] = int.Parse(input[1]);

                    return result;

                }
                catch
                {
                    Console.WriteLine("Neplatné číslo, zkuste to znovu.");
                }

            }
        }
    }
    //validace IP adresy
    static string GetValidIPAddress()
    {
        while (true)
        {
            string input = Console.ReadLine();
            try
            {
                if (IPAddress.TryParse(input, out _)) //zjistění jeslti se jedná o validní IP adresu
                {
                    return input;
                }
            }
            catch
            {
                Console.WriteLine("Neplatná IP adresa, zkuste to znovu.");
            }
        }
    }
    //logování výstupů
    static void LogMessage(string message)
    {
        string logFilePath = "log.txt";
        using (StreamWriter writer = new StreamWriter(logFilePath, true))
        {
           writer.WriteLine(message);
        }
    }

    static void TcpSending(int[] port, string ip, int period)
    {
        using (var tcpClient = new TcpClient()) //inicializace TCP clienta za pomoci using kvůli likvidaci instance
        {
            try
            {
                if (port[1] != 0)
                {
                    while (true)
                    {
                        for (int i = port[0]; i <= port[1]; i++)
                        {
                            tcpClient.Connect(ip, i); //připojení clienta ke specifickému portu
                            using (NetworkStream stream = tcpClient.GetStream()) //získání streamu na psaní dat
                            {
                                byte[] data = Encoding.UTF8.GetBytes(DateTime.Now.ToString()); // uloží data do bytového pole
                                stream.Write(data, 0, data.Length); //pošle data
                                Console.WriteLine("(TCP) Odeslána zpráva " + i + ":" + DateTime.Now);
                            }
                        }
                        Thread.Sleep(period * 1000); //uspí se na dobu periody
                    }
                }
                else
                {
                    tcpClient.Connect(ip, port[0]); //připojení clienta ke specifickému portu
                    using (NetworkStream stream = tcpClient.GetStream()) //získání streamu na psaní dat
                    {
                        while (true)
                        {
                            byte[] data = Encoding.UTF8.GetBytes(DateTime.Now.ToString()); // uloží data do bytového pole
                            stream.Write(data, 0, data.Length); //pošle data
                            Console.WriteLine("(TCP) Odeslána zpráva: " + DateTime.Now);
                            Thread.Sleep(period * 1000); //uspí se na dobu periody
                        }
                    }
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine("Spojení bylo přerušeno, pokusím se znovu připojit: " + ex.Message);
                LogMessage("Spojení bylo přerušeno, pokusím se znovu připojit: " + ex.Message);
                Thread.Sleep(period * 1000);
            }
        }
    }

    static void UdpSending(int[] port, string ip, int period)
    {
        if (port[1] != 0)
        {
            using (var udpClient = new UdpClient()) //inicializace UDP clienta za pomoci using kvůli likvidaci instance
            {
                while (true)
                {
                    for (int i = port[0]; i <= port[1]; i++)
                    {
                        IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ip), i); //inicializace endpoint

                        byte[] data = Encoding.UTF8.GetBytes(DateTime.Now.ToString()); // uloží data do bytového pole
                        udpClient.Send(data, data.Length, endPoint); //pošle data
                        Console.WriteLine("Odeslána zpráva: " + DateTime.Now);
                        Thread.Sleep(period * 1000); //uspí se na dobu periody   

                    }
                    Thread.Sleep(period * 1000); //uspí se na dobu periody
                }
            }
        }
        else
        {
            using (var udpClient = new UdpClient()) //inicializace UDP clienta za pomoci using kvůli likvidaci instance
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ip), port[0]); //inicializace endpoint
                while (true)
                {
                    byte[] data = Encoding.UTF8.GetBytes(DateTime.Now.ToString()); // uloží data do bytového pole
                    udpClient.Send(data, data.Length, endPoint); //pošle data
                    Console.WriteLine("Odeslána zpráva: " + DateTime.Now);
                    Thread.Sleep(period * 1000); //uspí se na dobu periody
                }
            }
        }
    }
}