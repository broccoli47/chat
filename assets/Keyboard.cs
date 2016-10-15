using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Keyboard : MonoBehaviour {

	public int CharLimit;
	public Text console;
	System.Text.StringBuilder build;
//	char[] chars;

	public bool Listen;
	int Size;
	int Pos;

	const char ctrlbackspace = (char)127;
	const char enter = (char)13;

	void Start(){
		Size = 0;
		build = new System.Text.StringBuilder (CharLimit, CharLimit);
	}

	void Submit(string line){
		Log.Line (line);

		char[] split = new char[1];
		split [0] = ' ';

		string[] arg = line.Split (split, 2);
		int args = arg.Length;

		switch(arg[0]){
		case "exit":
		case "quit":
			Application.Quit ();
			break;
		case "clear":
			Log.Clear ();
			break;
		case "init":
			Host.Main.InitInternet ();
			break;
		case "status":
			Host.Main.ConnectionStatus ();
			break;
		case "host":
			Host.Main.InitializeServer ();
			break;
		case "connect":
			if (args > 1) {
				Host.Main.ConnectToServer (arg [1]);
			}else{
				MissingParameter ();
			}
			break;
		case "shutdown":
			Host.Main.Shutdown ();
			break;
		case "disconnect":
			Host.Main.Disconnect ();
			break;
		case "send":
		case "message":
			if(args > 1){
				Host.Main.Message (arg [1]);
			}else{
				Log.Error ("Enter message to send");
			}
			break;
		case "cmdlist":

			if (args > 1) {
				switch (arg [1]) {
				case "net":
					Log.Line ("init - initializes network transport layer");
					Log.Line ("host - sets up a server");
					Log.Line ("connect - connect to a server with given ip adress");
					Log.Line ("disconnect - disconnect from current server");
					Log.Line ("shutdown - terminate all network activities");
					Log.Line ("status - displays current network status");
					Log.Line ("message/send - send message to all connections");
					break;
				default:
					Log.Error ("Unrecognized subcategory.");
					break;
				}
			} else {
				Log.Error ("You need to pass a subcategory as a parameter.");
				Log.Line ("Subcategories: net");
			}
			break;
		default:
			Log.Error ("'" + arg [0] + "' is not a recognized command.");
			break;
		}
	}

	void MissingParameter(){
		Log.Error ("Missing parameter");
	}

	void Clear(){
		build.Remove (0, Size);
		Size = 0;
		Pos = 0;
	}

	void Update () {
		if (Listen) {
			if (Input.GetKeyDown (KeyCode.LeftArrow) && Pos > 0) {
				Pos--;
			}
			if (Input.GetKeyDown (KeyCode.RightAlt) && Pos < Size) {
				Pos++;
			}
			if (Input.GetKeyDown (KeyCode.Home)) {
				Pos = 0;
			}
			if (Input.GetKeyDown (KeyCode.End)) {
				Pos = Size;
			}

			string s = Input.inputString;
			int Length = s.Length;

			for (int i = 0; i < Length; ++i) {
				switch (s [i]) {
				case '\b':
					if (Size > 0) {
						build.Remove (--Size, 1);
						Pos = Pos > Size ? Size : Pos;
					}
					break;
				case ctrlbackspace:
					// delete last word
					break;
				case enter:
					Submit (build.ToString ());
					Clear ();
					break;
				default:
					if((s[i] >= 'a' && s[i] <= 'z') ||
						(s[i] >= 'A' && s[i] <= 'Z') ||
						(s[i] >= '0' && s[i] <= '9') ||
						s[i] == ' ' ||
						s[i] == '_' ||
						s[i] == '+' ||
						s[i] == '-' ||
						s[i] == '.' ||
						s[i] == ',' ||
						s[i] == '*' ||
						s[i] == '/' ||
						s[i] == '=' ||
						s[i] == '|' ||
						s[i] == '\\' ||
						s[i] == '\'' ||
						s[i] == '"' ||
						s[i] == '^' ||
						s[i] == '(' ||
						s[i] == ')' ||
						s[i] == '[' ||
						s[i] == ']'){

						if (Pos >= Size) {
							Size++;
							build.Append (s [i]);
						} else {
							build [Pos] = s [i];
						}

						Pos++;
					}else{
						Log.Line ("Fuck off bitch (char: " + ((int)s [i]).ToString () + ")");
					}

					break;
				}
			}

			if (Length > 0) {
				console.text = build.ToString ();
			}
		}
	}
}
