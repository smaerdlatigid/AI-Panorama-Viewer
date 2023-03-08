using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using TMPro;

public class TCPServer : MonoBehaviour
{
	#region private members 	
	/// <summary> 	
	/// TCPListener to listen for incomming TCP connection 	
	/// requests. 	
	/// </summary> 	
	private TcpListener tcpListener;
	/// <summary> 
	/// Background thread for TcpServer workload. 	
	/// </summary> 	
	private Thread tcpListenerThread;
	/// <summary> 	
	/// Create handle to connected tcp client. 	
	/// </summary> 	
	private TcpClient connectedTcpClient;
	#endregion

	public string myIP;
	public int port = 16969;
    public Material imageMaterial;
    public GameObject imagePlane;

    public TMP_Text textMesh;
    Queue<Action> jobs = new Queue<Action>();
	String GetIPAddress()
	{
		IPHostEntry Host = default(IPHostEntry);
		string Hostname = null;
		Hostname = System.Environment.MachineName;
		Host = Dns.GetHostEntry(Hostname);
		foreach (IPAddress IP in Host.AddressList)
		{
			if (IP.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
			{
				return Convert.ToString(IP);
			}
		}
		return "failed";
	}
	// Use this for initialization
	void Start()
	{
		myIP = GetIPAddress();
        print("IP: " + myIP);
        textMesh.text = myIP;

		// Start TcpServer background thread 		
		tcpListenerThread = new Thread(new ThreadStart(ListenForIncommingRequests));
		tcpListenerThread.IsBackground = true;
		tcpListenerThread.Start();
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			SendMessage();
		}

        while (jobs.Count > 0){
            jobs.Dequeue().Invoke();
        }
	}
    internal void AddJob(Action newJob) {
        jobs.Enqueue(newJob);
    }

    void OnDestroy()
    {
        tcpListenerThread.Abort();
    }

    void LoadImage()
    {
        // load the image and set on material
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(message_data);
        
        imagePlane.GetComponent<Renderer>().material.mainTexture = texture;
        // print image size
        print("image size: " + texture.width + "x" + texture.height);
        float aspectRatio = (float)texture.width / (float)texture.height;
        imagePlane.transform.localScale = new Vector3(aspectRatio, 1, 1);
    }
	/// <summary> 	
	/// Runs in background TcpServerThread; Handles incomming TcpClient requests 	
	/// </summary> 	
    Byte[] message_data;
	private void ListenForIncommingRequests()
	{
		try
		{
			// Create listener on port 			
			tcpListener = new TcpListener(IPAddress.Parse(myIP), port);
			tcpListener.Start();
			Debug.Log("Server is listening");
			Byte[] bytes = new Byte[1024];
            // save all the data received from client
            List<Byte[]> data = new List<Byte[]>();
			while (true)
			{
				using (connectedTcpClient = tcpListener.AcceptTcpClient())
				{
					// Get a stream object for reading 					
					using (NetworkStream stream = connectedTcpClient.GetStream())
					{
						int length;
                        data.Clear();
                        int data_size = 0;
						// Read incomming stream into byte arrary. 						
						while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
						{
							var incommingData = new byte[length];
							Array.Copy(bytes, 0, incommingData, 0, length);
                            // if (data_size == 0)
                            // {
                            //     // read four bytes to get the size of the data
                            //     byte[] size_bytes = new byte[4];
                            //     Array.Copy(incommingData, 0, size_bytes, 0, 4);
                            //     data_size = BitConverter.ToInt32(size_bytes, 0);
                            //     Debug.Log("incoming data size: " + data_size);

                            // }
                            if (length != 1024)
                            {
                                message_data = new Byte[data_size+length];
                                for (int i = 0; i < data.Count; i++)
                                {
                                    Array.Copy(data[i], 0, message_data, i * data[i].Length, data[i].Length);
                                }

                                // copy incoming data into message without the last newline
                                Array.Copy(incommingData, 0, message_data, data_size - incommingData.Length, incommingData.Length - 1);
                                
                                AddJob(LoadImage);
                            }
                            else{
                                data.Add(incommingData);
                                data_size += length;
                            }
						}   
					}
				}
			}
		}
		catch (SocketException socketException)
		{
			Debug.Log("SocketException " + socketException.ToString());
		}
	}
	/// <summary> 	
	/// Send message to client using socket connection. 	
	/// </summary> 	
	private void SendMessage()
	{
		if (connectedTcpClient == null)
		{
			return;
		}

		try
		{
			// Get a stream object for writing. 			
			NetworkStream stream = connectedTcpClient.GetStream();
			if (stream.CanWrite)
			{
				string serverMessage = "This is a message from your server.";
				// Convert string message to byte array.                 
				byte[] serverMessageAsByteArray = Encoding.ASCII.GetBytes(serverMessage);
				// Write byte array to socketConnection stream.               
				stream.Write(serverMessageAsByteArray, 0, serverMessageAsByteArray.Length);
				Debug.Log("Server sent his message - should be received by client");
			}
		}
		catch (SocketException socketException)
		{
			Debug.Log("Socket exception: " + socketException);
		}

        // ON CLOSE

	}
}