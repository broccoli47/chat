using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Host : MonoBehaviour {

	public static Host Main;

	void Awake(){
		Main = this;
	}

	public enum InternetState : byte
	{
		Offline, None, Server, Client
	}

	public InternetState state;
	ConnectionConfig config;
	string MyIP;

	ulong NetworkID;
	ushort DstNode;
	byte Error;

	// When server
	int MaxConnections = 10;
	int Unreliable;
	int Reliable;
	int HostID;
	int ServerPort = 8888;

	// When client
	int ConnectionID;
	public string ServerIP;

	// Buffer
	int RecHostID;
	int RecConnectionID;
	int RecChannelID;
	int BufferSize = 1024;
	byte[] Buffer;
	int DataSize;
	byte error;

	public void InitInternet(){
		if (state == InternetState.Offline) {
			Log.Line ("Initializing Network Transport");
			MyIP = Network.player.ipAddress;
			Log.Line ("Allocating network buffer (" + BufferSize.ToString () + " bytes)");
			Buffer = new byte[BufferSize];

			GlobalConfig gconfig = new GlobalConfig ();
			gconfig.MaxPacketSize = 512;

			NetworkTransport.Init (gconfig);

			Log.Line ("Creating Reliable and Unreliable channels");
			config = new ConnectionConfig ();
			Unreliable = config.AddChannel (QosType.Unreliable);
			Reliable = config.AddChannel (QosType.Reliable);

			state = InternetState.None;
		}
	}

	public void InitializeServer(){
		if (state == InternetState.None) {
			state = InternetState.Server;
			HostTopology topology = new HostTopology (config, MaxConnections);
			HostID = NetworkTransport.AddHost (topology, ServerPort);

			Log.Server ("Starting Server");
			Log.Line (" IP address: " + MyIP);
			Log.Line (" Port: " + ServerPort.ToString ());
			Log.Line (" Max Connections: " + MaxConnections.ToString ());
		}
	}

	public void Shutdown(){
		NetworkTransport.Shutdown ();
		Log.Line ("All network activity has been terminated.");
		state = InternetState.Offline;
	}

	public void Disconnect(){
		if (state == InternetState.Client) {
			NetworkTransport.RemoveHost (HostID);
			Log.Line ("You have disconnected from the server.");
		}
	}

	public void ConnectionStatus(){
		switch (state) {
		case InternetState.Server:
			int Connections = 0;
			for (int i = 1; i <= MaxConnections; ++i) {
				NetworkTransport.GetConnectionInfo (HostID, i, out ServerPort, out NetworkID, out DstNode, out Error);

				if (Error != 2) {
					Connections++;
					Log.Server ("Slot " + i.ToString ());
					Log.Line (" Port: " + ServerPort.ToString ());
					Log.Line (" Network ID: " + NetworkID.ToString ());
					Log.Line (" DstNode: " + DstNode.ToString ());
					Log.Line (" Error: " + Error.ToString () + " (" + ((NetworkError)Error).ToString () + ")");
				}

			}
			Log.Server (Connections.ToString () + " connections.");
			break;
		case InternetState.Client:
			GetConnectionInfo ();
			break;
		case InternetState.Offline:
			Log.Error ("Transport layer not initialized.");
			break;
		default:
			Log.Line ("You are not connected.");
			break;
		}
	}

	public void ConnectToServer(string ServerIP){
		if (state == InternetState.None) {

			state = InternetState.Client;

			HostTopology topology = new HostTopology (config, 1);
			HostID = NetworkTransport.AddHost (topology);

			byte error;
			ConnectionID = NetworkTransport.Connect (HostID, ServerIP, ServerPort, 0, out error);

			if (ConnectionID > 0) {
				Log.Server ("Connecting to " + ServerIP);
			}else{
				Log.Error ("Maximum amount of connections exceeded");
			}
		}else{
			Log.Error ("You cannot connect in your current network state.");
		}
	}

	public void GetConnectionInfo(){
		NetworkTransport.GetConnectionInfo (HostID, ConnectionID, out ServerPort, out NetworkID, out DstNode, out Error);
		Log.Server ("Connection Info Report");
		Log.Line (" Port: " + ServerPort.ToString ());
		Log.Line (" Network ID: " + NetworkID.ToString ());
		Log.Line (" DstNode: " + DstNode.ToString ());
		Log.Line (" Error: " + ((NetworkError)Error).ToString ());
	}

	public void Message(string message){
		switch (state) {
		case InternetState.Client:
			ClientMessage (message);
			break;
		case InternetState.Server:
			ServerMessage (message);
			break;
		default:
			Log.Error ("You can't send messages in current network state.");
			break;
		}
	}

	void ClientMessage(string message){
		byte[] bytes = new byte[message.Length * sizeof(char)];
		System.Buffer.BlockCopy(message.ToCharArray(), 0, bytes, 0, bytes.Length);
		NetworkTransport.Send (HostID, ConnectionID, Reliable, bytes, bytes.Length, out Error);
	}

	void ServerMessage(string message){
		byte[] bytes = new byte[message.Length * sizeof(char)];
		System.Buffer.BlockCopy(message.ToCharArray(), 0, bytes, 0, bytes.Length);

		for (int i = 1; i <= MaxConnections; ++i) {
			NetworkTransport.GetConnectionInfo (HostID, i, out ServerPort, out NetworkID, out DstNode, out Error);

			if (Error == 0) {
				NetworkTransport.Send (HostID, i, Reliable, bytes, bytes.Length, out Error);
			}
		}
	}

	public static int RTT;

	void Update()
	{
		if (state > InternetState.None) {
			RTT = NetworkTransport.GetCurrentRtt (HostID, ConnectionID, out error);

			NetworkEventType recData = NetworkTransport.Receive (out RecHostID, out RecConnectionID, out RecChannelID, Buffer, BufferSize, out DataSize, out error);
			switch (recData) {
			case NetworkEventType.Nothing:
				break;
			case NetworkEventType.ConnectEvent:
				if (ConnectionID == RecConnectionID) {
					Log.Server ("Connected to server as " + RecConnectionID.ToString ());
				} else {
					Log.Server (RecConnectionID.ToString() + " Connected");
				}
				break;
			case NetworkEventType.DataEvent:
				
				char[] chars = new char[Buffer.Length / sizeof(char)];
				System.Buffer.BlockCopy (Buffer, 0, chars, 0, DataSize);
				Log.Line (new string (chars), new Color (0.7f, 0.2f, 1f));

				break;
			case NetworkEventType. DisconnectEvent: 
				if (ConnectionID == RecConnectionID) {
					Log.Error ("Connection Failed (" + ((NetworkError)error).ToString () + ")");
				} else {
					Log.Server (RecConnectionID.ToString () + " Disconnected");
				}
				break;
			}
		}
	}
}
