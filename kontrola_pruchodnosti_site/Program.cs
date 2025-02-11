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
        try //načítání inputů od uživatele
        {
            Console.WriteLine("Pro vysílání napište 1, pro naslouchání 2: ");
            string mode = Console.ReadLine();
            while (mode != "1" && mode != "2") //zajistění správného inputu módu
            {
                Console.WriteLine("Neplatný vstup. Zadejte 1 nebo 2: ");
                mode = Console.ReadLine();
            }

            Console.WriteLine("Zvolte protokol (TCP/UDP), pro oba napistě 2: ");
            string protocol = Console.ReadLine().ToUpper();
            while (protocol != "TCP" && protocol != "UDP" && protocol != "2") //zajistění správného inputu protocolu
            {
                Console.WriteLine("Neplatný protokol. Zadejte TCP nebo UDP: ");
                protocol = Console.ReadLine().ToUpper();
            }

            Console.WriteLine("Zadejte port: ");
            int port = GetValidNumber(); //zavolání funkce pro validaci čísla - portu

            if (mode == "1")
            {
                Console.WriteLine("Zadejte IP adresu: ");
                string ip = GetValidIPAddress(); //zavolání funkce pro validaci ip adresy
                Console.WriteLine("Zadejte periodu v sekundách (výchozí 5): ");
                int period = GetValidNumber(5); //zavolání funkce pro validaci čísla - periody
                Sending(protocol, port, ip, period); //zavolání funkce pro vysílání
            }
            else
            {
                Listening(protocol, port); //zavolání funkce pro poslouchání
            }
        }
        catch (Exception ex)
        {
            LogMessage("Neočekávaná chyba: " + ex.Message);
            Console.WriteLine("Neočekávaná chyba: " + ex.Message);
        }
    }

    //procedura určená k vysílání
    static void Sending(string protocol, int port, string ip, int period)
    {
        try
        {   //vysílání pomoci TCP
            if (protocol == "TCP")
            {
                while (true)
                {
                    using (var tcpClient = new TcpClient()) //inicializace TCP clienta za pomoci using kvůli likvidaci instance
                    {
                        try
                        {
                            tcpClient.Connect(ip, port); //připojení k clienta k specifického portu
                            using (NetworkStream stream = tcpClient.GetStream()) //získání streamu na psaní dat
                            {
                                while (true)
                                {
                                    byte[] data = Encoding.UTF8.GetBytes(DateTime.Now.ToString()); // uloží data do bytového pole
                                    stream.Write(data, 0, data.Length); //pošle data
                                    Console.WriteLine("Odeslána zpráva: " + DateTime.Now);
                                    Thread.Sleep(period * 1000); //uspí se na dobu periody
                                }
                            }
                        }
                        catch (IOException ex)
                        {
                            Console.WriteLine("Spojení bylo přerušeno, pokusím se znovu připojit: " + ex.Message);
                            Thread.Sleep(period*1000); 
                        }
                    }
                }
            }
            else if (protocol == "UDP")
            {
            
                using (var udpClient = new UdpClient()) //inicializace UDP clienta za pomoci using kvůli likvidaci instance
                {
                    IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ip), port); //inicializace endpointu
                    while (true)
                    {
                        byte[] data = Encoding.UTF8.GetBytes(DateTime.Now.ToString()); // uloží data do bytového pole
                        udpClient.Send(data, data.Length, endPoint); //pošle data
                        Console.WriteLine("Odeslána zpráva: " + DateTime.Now);
                        Thread.Sleep(period * 1000); //uspí se na dobu periody
                    }
                }
            } else
            {
                while (true)
                {
                    using (var tcpClient = new TcpClient()) //inicializace TCP clienta za pomoci using kvůli likvidaci instance
                    using (var udpClient = new UdpClient()) //inicializace UDP clienta za pomoci using kvůli likvidaci instance
                    {
                        try
                        {
                            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ip), port); //inicializace endpointu
                            tcpClient.Connect(ip, port); //připojení k clienta k specifického portu
                            using (NetworkStream stream = tcpClient.GetStream()) //získání streamu na psaní dat
                            {
                                while (true)
                                {
                                    byte[] data = Encoding.UTF8.GetBytes(DateTime.Now.ToString()); // uloží data do bytového pole
                                    stream.Write(data, 0, data.Length); //pošle data
                                    Console.WriteLine("Odeslána zpráva (TCP): " + DateTime.Now);

                                    byte[] data2 = Encoding.UTF8.GetBytes(DateTime.Now.ToString()); // uloží data do bytového pole
                                    udpClient.Send(data2, data2.Length, endPoint); //pošle data
                                    Console.WriteLine("Odeslána zpráva (UDP): " + DateTime.Now);
                                    Thread.Sleep(period * 1000); //uspí se na dobu periody
                                }
                            }
                        }
                        catch (IOException ex)
                        {
                            Console.WriteLine("Spojení bylo přerušeno, pokusím se znovu připojit: " + ex.Message);
                            Thread.Sleep(period * 1000);
                        }
                    }
                }
            }

        }
        catch (SocketException ex)
        {
            Console.WriteLine("Chyba připojení: " + ex.Message);
        }
    }



    //procedura určená k poslouchání
    static void Listening(string protocol, int port)
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
            }
            else
            {   //poslouchání pomocí UDP
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
}

