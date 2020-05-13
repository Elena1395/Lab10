using System;
using System.Text;
using System.Net.Sockets;
using System.IO;

namespace SomeProject.Library.Client
{
    public class Client
    {
        public TcpClient tcpClient;

        /// <summary>
        /// Получение сообщения с сервера.
        /// </summary>
        /// <param name="stream">Текущий поток</param>
        public OperationResult ReceiveMessageFromServer(NetworkStream stream)
        {
            try
            {
                StringBuilder recievedMessage = new StringBuilder();
                byte[] data = new byte[256];
                do
                {
                    int bytes = stream.Read(data, 0, data.Length);
                    recievedMessage.Append(Encoding.UTF8.GetString(data, 0, bytes));
                }
                while (stream.DataAvailable);
                stream.Close();
                return new OperationResult(Result.OK, recievedMessage.ToString());
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.ToString());
            }
        }

        /// <summary>
        /// Отправка сообщения на сервер.
        /// </summary>
        /// <param name="message">Сообщение для отправки</param>
        public OperationResult SendMessageToServer(string message)
        {
            try
            {
                using (tcpClient = new TcpClient("127.0.0.1", 8080))
                {
                    using (NetworkStream stream = tcpClient.GetStream())
                    {
                        byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
                        stream.Write(data, 0, data.Length);
                        return ReceiveMessageFromServer(stream);
                    }
                }
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.Message);
            }
        }

        /// <summary>
        /// Отправка файла на сервер.
        /// </summary>
        /// <param name="message">Сообщение для отправки</param>
        public OperationResult SendFileToServer(string fileName)
        {
            try
            {
                using (tcpClient = new TcpClient("127.0.0.1", 8080))
                {
                    using (NetworkStream stream = tcpClient.GetStream())
                    {
                        byte[] fname = Encoding.UTF8.GetBytes("1" + fileName.Substring(fileName.LastIndexOf('.') + 1) + "$");
                        stream.Write(fname, 0, fname.Length);
                        using (FileStream fstream = new FileStream(fileName, FileMode.Open))
                        {
                            byte[] buffer = new byte[4096];
                            int len = 0;
                            do
                            {
                                len = fstream.Read(buffer, 0, buffer.Length);
                                stream.Write(buffer, 0, len);
                            } while (len != 0);
                        }
                        //return new OperationResult(Result.OK,"");
                        return ReceiveMessageFromServer(stream);
                    }
                }
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.Message);
            }
        }

    }
}

