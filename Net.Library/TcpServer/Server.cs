using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SomeProject.Library.Server
{
    public class Server
    {
        private static int NumOfFile = 0;//номер файла
        private static long CurrCnt = 0;//такущее количество подключений
        private static long MaxCon = 1;//Максимальное количество подключений


        TcpListener serverListener;
        public Server()
        {
            serverListener = new TcpListener(IPAddress.Loopback, 8080);
        }

        /// <summary>
        /// Прослушивание подключений от TCP-клиентов сети.
        /// </summary>
        public async Task TurnOnListener()
        {
            try
            {
                if (serverListener != null)
                {
                    serverListener.Start();
                }
                Console.WriteLine("Waiting for connections...");
               // ThreadPool.SetMaxThreads(MaxCon, MaxCon);

                Console.WriteLine("The current number of connections: " + CurrCnt);
                while (true)
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(NewConnection), serverListener.AcceptTcpClient());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot turn on listener: " + e.Message);
            }
        }

        /// <summary>
        /// Обработка нового соединения.
        /// </summary>
        /// <param name="client">tcp Клиент</param>
        static void NewConnection(object client)
        {
            while (Interlocked.Read(ref CurrCnt) >= Interlocked.Read(ref MaxCon))
            { }
            if(Interlocked.Read(ref CurrCnt)< Interlocked.Read(ref MaxCon)) {
                Interlocked.Increment(ref CurrCnt);
                Console.WriteLine("The current number of connections: " + CurrCnt);
                OperationResult result = CheckMessage((TcpClient)client).Result;
                if (result.Result == Result.Fail)
                    Console.WriteLine("Unexpected error: " + result.Message);
                else
                    Console.WriteLine(result.Message);
                Interlocked.Decrement(ref CurrCnt);
            }
        }

        /// <summary>
        /// Завершение прослушиваний подключений от TCP-клиентов сети.
        /// </summary>
        public bool TurnOffListener()
        {
            try
            {
                if (serverListener != null)
                    serverListener.Stop();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot turn off listener: " + e.Message);
                return false;
            }
        }

        /// <summary>
        /// Обработка поступившего сообщениея от клиента.
        /// </summary>
        /// <param name="client">tcp Клиент</param>
        public async static Task<OperationResult> CheckMessage(TcpClient client)
        {
            try
            {
                StringBuilder recievedMessage = new StringBuilder();

                byte[] data = new byte[1];
                using (NetworkStream stream = client.GetStream())
                {
                    await  stream.ReadAsync(data, 0, data.Length);
                    string str = Encoding.UTF8.GetString(data, 0, data.Length);
                    if (str == "1")//файл
                        {
                            recievedMessage = ReceiveFileFromClient(stream);
                            SendMessageToClient(stream, "The server received a file.");

                        }
                        else if (str == "0")//сообщение
                        {
                            recievedMessage = ReceiveMessageFromClient(stream);
                            SendMessageToClient(stream, "The server received a message.");
                        //await Task.Delay(5000);
                        }
                        return new OperationResult(Result.OK, recievedMessage.ToString());
                }            
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.Message);
            }
        }

        /// <summary>
        /// Обработка файла от клиента.
        /// </summary>
        /// <param name="stream">Текущий поток</param>
        public static StringBuilder ReceiveFileFromClient(NetworkStream stream)
        {
            try
            {
                Interlocked.Increment(ref NumOfFile);
                //StringBuilder recievedMessage = new StringBuilder("File: ");
                byte[] data = new byte[4096];
               
                if (!Directory.Exists(DateTime.Now.ToString("yyyy-MM-dd")))
                {
                    Directory.CreateDirectory(DateTime.Now.ToString("yyyy-MM-dd"));
                }

                int bytes = stream.Read(data, 0, data.Length);
                string str = Encoding.UTF8.GetString(data);
                int index = str.IndexOf('$');
                string type = str.Substring(0, index);
                StringBuilder path =new StringBuilder( DateTime.Now.ToString("yyyy-MM-dd") + "\\File" + NumOfFile + "." + type);

                using (FileStream fstream = new FileStream(path.ToString(), FileMode.Create))
                {
                    fstream.Write(data, index + 1, bytes - index - 1);
                    //recievedMessage.Append(Encoding.UTF8.GetString(data, index + 1, bytes - index - 1));////
                    while (stream.DataAvailable)
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        fstream.Write(data, 0, bytes);
                        //recievedMessage.Append(Encoding.UTF8.GetString(data, 0, bytes));
                    }
                }
                // return recievedMessage;
                StringBuilder ex = new StringBuilder("File: ");
                return ex.Append(path);
            }
            catch (Exception e)
            {
                StringBuilder ex = new StringBuilder();
                ex.Append(e.Message);
                return ex;
            }
        }

        /// <summary>
        /// Обработка сообщения от клиента.
        /// </summary>
        /// <param name="stream">Текущий поток</param>
        public static StringBuilder ReceiveMessageFromClient(NetworkStream stream)
        {
            try
            {
                StringBuilder recievedMessage = new StringBuilder("Message: ");
                byte[] data = new byte[256];
                do
                {
                    int bytes = stream.Read(data, 0, data.Length);
                    recievedMessage.Append(Encoding.UTF8.GetString(data, 0, bytes));
                }
                while (stream.DataAvailable);
                return  recievedMessage;
            }
            catch (Exception e)
            {
                StringBuilder ex = new StringBuilder();
                ex.Append(e.Message);
                return  ex;
            }
        }

        /// <summary>
        /// Отправление сообщения клиенту о принятии его сообщения
        /// </summary>
        /// <param name="stream">Текущий поток</param>
        /// <param name="message">Сообщение</param>
        public static OperationResult SendMessageToClient(NetworkStream stream, string message)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.Message);
            }
            return new OperationResult(Result.OK, "");
        }
    }
}