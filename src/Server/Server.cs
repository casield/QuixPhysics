using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ContentBuilder;
using ContentLoader;

namespace QuixPhysics
{
    public class ConnectionState : IDisposable
    {
        // Size of receive buffer.  
        public const int BufferSize = 2048*4;

        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];

        // Received data string.
        public StringBuilder sb = new StringBuilder();

        // Client socket.
        public Socket workSocket = null;
        public Simulator simulator = null;

        public void Dispose()
        {
            simulator = null;
            sb.Clear();
        }
    }
    public class Server
    {

        public static ManualResetEvent allDone = new ManualResetEvent(false);
        public DataBase dataBase;
        public bool isRunning = true;

        public Dictionary<String, MeshContent> meshes = new Dictionary<string, MeshContent>();
        public Server()
        {
            dataBase = new DataBase();

            LoadMeshes();

            StartListening();
        }

        private void LoadMeshes(){
     
           LoadMesh("Stadiums/CrocoLoco/piedra1");
            LoadMesh("Stadiums/CrocoLoco/piedra2");
            LoadMesh("Stadiums/CrocoLoco/stand_croco");
            LoadMesh("Stadiums/CrocoLoco/start_position");
            LoadMesh("newt");
            QuixConsole.Log("Reload meshes");

        }

        public void ReloadMeshes(){
            meshes.Clear();
            LoadMeshes();
        }

        public MeshContent GetMesh(string name){
            return meshes[name];
        }

        private void LoadMesh(string name){
            meshes.Add(name,LoadObjFile(@"Content/"+name+".obj"));
        }

        private MeshContent LoadObjFile(string path){
            var npath = Path.Combine("",path);
            using (FileStream fs = File.OpenRead(path))
            {
                return MeshBuilder.Build(fs);
            }
        }

        public void StartListening()
        {


            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 1337);

            // Create a TCP/IP socket.  
            Socket listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);


            //Simulator simulator = new Simulator(new ConnectionState(), this);
            // Bind the socket to the local endpoint and listen for incoming connections.  
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen();
                QuixConsole.Log("QuixPhysicsServer is","running");

                while (isRunning)
                {
                    // Set the event to nonsignaled state.  
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.  
                    QuixConsole.WriteLine("Waiting for a connection...");
                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        listener);

                    // Wait until a connection is made before continuing.  
                    allDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                QuixConsole.WriteLine(e.ToString());

            }

            QuixConsole.WriteLine("\nPress ENTER to continue...");
        }


        public void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.  
            allDone.Set();

            // Get the socket that handles the client request.  
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Create the state object.  
            ConnectionState state = new ConnectionState();
            state.workSocket = handler;

            CreateSimulator(state);

            handler.BeginReceive(state.buffer, 0, ConnectionState.BufferSize, 0,
            new AsyncCallback(ReadCallback), state);


        }

        private void CreateSimulator(ConnectionState socket)
        {
            QuixConsole.WriteLine("Create simulator");
            Simulator simulator = new Simulator(socket, this);
            socket.simulator = simulator;
        }
        public void ReadCallback(IAsyncResult ar)
        {


            ConnectionState state = (ConnectionState)ar.AsyncState;
            String content = String.Empty;

            // Retrieve the state object and the handler socket  
            // from the asynchronous state object.  

            Socket handler = state.workSocket;


            // Read data from the client socket.
            if (handler.Connected)
            {
                try{
                    int bytesRead = handler.EndReceive(ar);
              
                



                if (bytesRead > 0)
                {
                    // There  might be more data, so store the data received so far.  
                    state.sb.Append(Encoding.ASCII.GetString(
                        state.buffer, 0, bytesRead));

                    // Check for end-of-file tag. If it is not there, read
                    // more data.  
                    content = state.sb.ToString();

                    var splited = content.Split("<L@>");
                
                    for (int a = 0; a < splited.Length; a++)
                    {
                        if (state.simulator != null)
                        {
                            if (splited[a] != "")
                            {
                                state.simulator.commandReader.AddCommandToBeRead(splited[a]);
                            }

                        }

                    }
                    state.sb.Clear();
                    handler.BeginReceive(state.buffer, 0, ConnectionState.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                }
                }catch(SocketException e){
                    QuixConsole.Log("Socket exception",e);
                }
            }
              


        }
        public void Send(Socket handler, String data)
        {
            try{
                
            // Convert the string data to byte data using ASCII encoding.  
            data += "<L@>";
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.  
            if (handler.Connected)
            {
                handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);

            }else{
                handler.Close();
            }
            }catch(SocketException e){
                QuixConsole.Log("Error in Send",e);
            }



        }
        private void SendCallback(IAsyncResult ar)
        {

            // Retrieve the socket from the state object.  
            Socket handler = (Socket)ar.AsyncState;

            // Complete sending the data to the remote device.  
            int bytesSent = handler.EndSend(ar);
            //  Console.WriteLine("Sent {0} bytes to client.", bytesSent);

            // handler.Shutdown(SocketShutdown.Both);
            // handler.Close();

        }
    }
}