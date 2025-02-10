using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Numerics;

internal class Program
{
    private static void Main(string[] args)
    {

        //načítání dat od uživatele (naslouchání/vysílání), (TCP/UDP), port
        Console.WriteLine("Pro vysílání napište 1 pro naslouchání 2: ");
        string mode = Console.ReadLine();

        if (mode != "1" && mode != "2")
        {
            Console.WriteLine("Neplatný režim napište 1 nebo 2!");
            return;
        }

        Console.WriteLine("Zvolte protocol TCP/UDP: ");
        string protocol = Console.ReadLine().ToUpper();

        if (protocol != "TCP" && protocol != "UDP")
        {
            Console.WriteLine("Neplatný protocol napište TCP nebo UDP!");
            return;
        }

        Console.WriteLine("Zadejte port: ");
        string port = Console.ReadLine();


        foreach (char c in port) //kontroluje zda uživatel zadal správný formát (pouze čísla)
        {
            if (c < '0' || c > '9')
            {
                Console.WriteLine("Špatný formát periody, zadejte periodu v sekundách jen čísly");
                return;
            }
        }
        int portInt = int.Parse(port);


        if (mode == "1") //dodání dat potřebných pro vysílání
        {
            Console.WriteLine("Zadejte ip addresu: ");
            string ip = Console.ReadLine();

            if (String.IsNullOrWhiteSpace(ip))
            {
                return;
            }

            string[] splitValues = ip.Split('.');
            if (splitValues.Length != 4)
            {
                Console.WriteLine("Špatný formát ip adresy");
                return;
            }


            Console.WriteLine("Zadejte periodu v sekundách, výchozí 5");
            string perioda = Console.ReadLine();

            if (perioda.Equals("0") || perioda.Equals("")) //jestliže uživatel zadá 0 nebo nic automaticky nastaví periodu na 5
            {
                perioda = "5";
            }

            foreach (char c in perioda)
            {
                if (c < '0' || c > '9')
                {
                    Console.WriteLine("Špatný formát periody, zadejte periodu v sekundách a jen čísly!");
                    return;
                }
            }

            int periodInt = int.Parse(perioda);

            Sending(protocol, portInt, ip, periodInt); //spustí metodu vysílání
        }
        else
        {
            Listening(protocol, portInt);
            Console.WriteLine("yes");
        }

        static void Sending(string protocol, int port, string ip, int period)
        {
            if (protocol == "TCP") //použití TCP protocolu
            {
                using (var tcpClient = new TcpClient()) // deklarace Tcp clienta pomocí using, aby jsme zajistili likvidaci instance
                {
                    tcpClient.Connect(ip, port); //připojení k portu 
                    NetworkStream stream = tcpClient.GetStream(); //získání streamu napsaní

                    while (true)
                    {
                        byte[] data = Encoding.UTF8.GetBytes($" {DateTime.Now} "); //uloží data do bytového pole (čas)
                        stream.Write(data, 0, data.Length); //odešle zprávu na tcp server
                        Console.WriteLine($" Odeslána zpráva: {DateTime.Now} ");

                        Thread.Sleep(period * 1000); //uspí vlákno o délce dané periody
                    }
                }
            }
            else //použití UDP protocolu
            {
                using (var udpClient = new UdpClient())
                {
                    IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ip), port);

                    while (true)
                    {
                        byte[] data = Encoding.UTF8.GetBytes($" {DateTime.Now} ");
                        udpClient.Send(data, data.Length, endPoint);
                        Console.WriteLine($"Odeslána zpráva: {DateTime.Now} ");
                        Thread.Sleep(period * 1000);
                    }
                }
            }

        }

        static void Listening(string protocol, int port)
        {
            if (protocol == "TCP") //použití TCP protocolu
            {
                TcpListener listener = new TcpListener(IPAddress.Any, port); //deklarace poslouchání na daném portu a jakékoli ip adresy
                listener.Start();
                Console.WriteLine("Poslouchám (TCP)...");

                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient(); //akceptuje tcp připojení
                    NetworkStream stream = client.GetStream();
                    byte[] buffer = new byte[1024];

                    int bytesRead;
                    while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0) //čte dokud nevyprázdní buffer
                    {
                        string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        Console.WriteLine($"Přijmutá zpráva : {receivedData}"); //vypíše příjmuté data
                    }
                }
            }
            else
            { //pužití UDP protocolu
                using (UdpClient udpListener = new UdpClient(port))
                {
                    Console.WriteLine("Poslouchám (UDP)...");

                    while (true)
                    {
                        IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);
                        byte[] data = udpListener.Receive(ref endPoint);
                        string receivedData = Encoding.UTF8.GetString(data);
                        Console.WriteLine($"Přijmutá zpráva: {receivedData}");
                    }
                }
            }
        }
    }
}
