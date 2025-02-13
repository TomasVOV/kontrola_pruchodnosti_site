using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Serilog;


internal class Program
{
    private static void Main()
    {
        // Konfigurace logování
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        
        try
        {   // Získání vstupních hodnot
            string selectedMode = GetMode();
            string selectedProtocol = GetProtocol();
            int[] selectedPorts = GetValidPort();

            // Zpracování vstupních hodnot
            if (selectedMode == "1")
            {   
                string targetIp = GetValidIPAddress();
                int sendInterval = GetValidPeriod("Zadejte periodu v sekundách (výchozí 5): ", 5);

                // Spuštění daného protokolu
                if (selectedProtocol == "TCP")
                {
                    TcpSending(selectedPorts, targetIp, sendInterval);
                }
                else if (selectedProtocol == "UDP")
                {
                    UdpSending(selectedPorts, targetIp, sendInterval);
                }
                else
                {
                    // Spuštění obou protokolů za pomoci dvou vláken
                    Thread tcpThread = new Thread(() => TcpSending(selectedPorts, targetIp, sendInterval));
                    Thread udpThread = new Thread(() => UdpSending(selectedPorts, targetIp, sendInterval));
                    tcpThread.Start();
                    udpThread.Start();
                }
            }
            else
            {
                // Spuštění naslouchání na daných portech
                if (selectedProtocol == "TCP")
                {
                    TcpListening(selectedPorts);
                }
                else if (selectedProtocol == "UDP")
                {
                    UdpListening(selectedPorts);
                }
                else
                {
                    // Spuštění obou protokolů za pomoci dvou vláken
                    Thread tcpThread = new Thread(() => TcpListening(selectedPorts));
                    Thread udpThread = new Thread(() => UdpListening(selectedPorts));
                    tcpThread.Start();
                    udpThread.Start();
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Neočekávaná chyba");
            Console.WriteLine("Neočekávaná chyba: " + ex.Message);
        }
    }

    // Metoda pro získání režimu
    static string GetMode()
    {
        Console.WriteLine("Pro vysílání napište 1, pro naslouchání 2: ");
        string userInput = Console.ReadLine();

        while (userInput != "1" && userInput != "2")
        {
            Console.WriteLine("Neplatný vstup. Zadejte 1 nebo 2: ");
            userInput = Console.ReadLine();
        }
        return userInput;
    }

    //Metoda pro získání protokolu
    static string GetProtocol()
    {
        Console.WriteLine("Zvolte protokol (TCP/UDP), pro oba napište 2: ");
        string userInput = Console.ReadLine().ToUpper();

        while (userInput != "TCP" && userInput != "UDP" && userInput != "2")
        {
            Console.WriteLine("Neplatný protokol. Zadejte TCP, UDP nebo 2: ");
            userInput = Console.ReadLine().ToUpper();
        }
        return userInput;
    }

    // Metoda pro získání platného periody
    static int GetValidPeriod(string message, int defaultValue = -1)
    {
        Console.WriteLine(message);
        string userInput = Console.ReadLine();

        while (true)
        {
            // Pokud je vstup prázdný a je nastavena výchozí hodnota, vrátí se výchozí hodnota
            if (string.IsNullOrWhiteSpace(userInput) && defaultValue != -1)
            {
                return defaultValue;
            }
            // Pokud je vstup číslo větší než 0, vrátí se toto číslo
            if (int.TryParse(userInput, out int validNumber) && validNumber > 0)
            {
                return validNumber;
            }

            Console.WriteLine("Neplatné číslo, zkuste to znovu.");
            userInput = Console.ReadLine();
        }
    }

    // Metoda pro získání platného portu nebo intervalu portů
    static int[] GetValidPort()
    {
        Console.WriteLine("Zadejte port nebo interval (a-b): ");
        string userInput = Console.ReadLine();

        while (true)
        {
            string[] inputParts = userInput.Split('-');

            // Pokud je vstup jedno číslo, vrátí se pole s jedním prvkem
            if (inputParts.Length == 1 && int.TryParse(inputParts[0], out int singlePort))
            {
                return new[] { singlePort };
            }

            // Pokud jsou vstup dvě čísla, vrátí se pole s porty od-do
            if (inputParts.Length == 2 && int.TryParse(inputParts[0], out int startPort) && int.TryParse(inputParts[1], out int endPort))
            {
                // Pokud je začáteční port větší než koncový, vypíše se chyba
                if (startPort > endPort)
                {
                    Console.WriteLine("Začáteční port musí být menší nebo roven koncovému.");
                    userInput = Console.ReadLine();
                    continue;
                }
                // 
                int[] portArray = new int[endPort - startPort + 1];
                for (int i = 0; i < portArray.Length; i++)
                {
                    portArray[i] = startPort + i;
                }
                return portArray;
            }

            Console.WriteLine("Neplatný port, zkuste to znovu.");
            userInput = Console.ReadLine();
        }
    }

    // Metoda pro získání platné IP adresy
    static string GetValidIPAddress()
    {
        Console.WriteLine("Zadejte IP adresu: ");
        string userInput = Console.ReadLine();

        // Kontrola, zda je vstup platná IP adresa
        while (!IPAddress.TryParse(userInput, out _))
        {
            Console.WriteLine("Neplatná IP adresa, zkuste to znovu.");
            userInput = Console.ReadLine();
        }
        return userInput;
    }

    // Metoda pro vysílání TCP zpráv
    static void TcpSending(int[] ports, string ipAddress, int intervalSeconds)
    {
        while (true)
        {
            // Pro každý port se vytvoří nové TCP spojení a odešle se zpráva
            foreach (int port in ports)
            {
                try
                {   // Vytvoření TCP spojení a odeslání zprávy
                    using (var tcpClient = new TcpClient(ipAddress, port))
                    using (NetworkStream stream = tcpClient.GetStream())
                    {
                        byte[] messageData = Encoding.UTF8.GetBytes(DateTime.Now.ToString());
                        stream.Write(messageData, 0, messageData.Length);
                        Log.Information("(TCP) Odeslána zpráva na {Port}", port);
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "(TCP) Chyba na portu {Port}", port);
                }
            }
            Thread.Sleep(intervalSeconds * 1000);
        }
    }

    // Metoda pro vysílání UDP zpráv
    static void UdpSending(int[] ports, string ipAddress, int intervalSeconds)
    {
        // Vytvoření UDP klienta
        using (var udpClient = new UdpClient())
        {
            while (true)
            {
                // Pro každý port se odešle zpráva
                foreach (int port in ports)
                {
                    IPEndPoint destinationEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
                    byte[] messageData = Encoding.UTF8.GetBytes(DateTime.Now.ToString());
                    udpClient.Send(messageData, messageData.Length, destinationEndPoint);
                    Log.Information("(UDP) Odeslána zpráva na {Port}", port);
                }
                Thread.Sleep(intervalSeconds * 1000);
            }
        }
    }

    // Metoda pro naslouchání TCP zpráv
    private static void TcpListening(int[] ports)
    {
        // Pro každý port se vytvoří nové TCP naslouchání
        foreach (int port in ports)
        {
            // Vytvoření vlákna pro naslouchání
            Thread listenerThread = new Thread(() =>
            {
                try
                {
                    TcpListener listener = new TcpListener(IPAddress.Any, port);
                    listener.Start();
                    Log.Information("(TCP) Naslouchání na portu {Port}...", port);

                    while (true)
                    {
                        TcpClient client = listener.AcceptTcpClient();
                        using (NetworkStream stream = client.GetStream())
                        {
                            byte[] buffer = new byte[1024];
                            int bytesRead = stream.Read(buffer, 0, buffer.Length);
                            string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                            Log.Information("(TCP) Přijata zpráva na portu {Port}: {Message}", port, receivedMessage);
                        }
                        client.Close();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "(TCP) Chyba při naslouchání na portu {Port}", port);
                }
            });

            listenerThread.Start();
        }
    }

    // Metoda pro naslouchání UDP zpráv
    private static void UdpListening(int[] ports)
    {
        // Pro každý port se vytvoří nové UDP naslouchání
        Console.WriteLine("yes");
        foreach (int port in ports)
        {
            // Vytvoření vlákna pro naslouchání
            Thread listenerThread = new Thread(() =>
            {
                try
                {
                    using (UdpClient udpListener = new UdpClient(port))
                    {
                        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, port);
                        Log.Information("(UDP) Naslouchání na portu {Port}...", port);

                        while (true)
                        {
                            byte[] receivedData = udpListener.Receive(ref remoteEndPoint);
                            string receivedMessage = Encoding.UTF8.GetString(receivedData);
                            Log.Information("(UDP) Přijata zpráva na portu {Port}: {Message}", port, receivedMessage);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "(UDP) Chyba při naslouchání na portu {Port}", port);
                }
            });
            listenerThread.Start();
        }
    }
}