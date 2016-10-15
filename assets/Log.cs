using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Log : MonoBehaviour {

	// static shit
	public static Color DefaultBackground = new Color (0, 0, 0);
	public static Color DefaultForeground = new Color (1, 1, 1);
	public static Color ErrorBackground = new Color (0.1f, 0, 0);
	public static Color ErrorForeground = new Color (1, 0, 0);
	public static Color ServerForeground = new Color (1, 1, 0);

	public static Log Main;

	public static void Error(string line){
		Main.colortext (ErrorForeground);
		Main.colorbackground (ErrorBackground);
		Main.line (line);
	}

	public static void Server(string line){
		Main.colortext (ServerForeground);
		Main.colorbackground (DefaultBackground);
		Main.line (line);
	}

	public static void Line(string line, Color color){
		Main.colortext (color);
		Main.line (line);
	}

	public static void Line(string line){
		Main.colortext (DefaultForeground);
		Main.colorbackground (DefaultBackground);
		Main.line (line);
	}

	public static void Clear(){
		Main.clear ();
	}

	void Awake(){
		Main = this;
	}
	//

	public int History;
	public float LineHeight;
	public Vector2 Padding;
	public GameObject template;
	public RectTransform parentTo;
	public Scrollbar scroll;

	RectTransform[] transforms;
	Image[] images;
	Text[] texts;
//	float[] Heights;
	int Next;
	int Used;
	bool LineBackground;

	void Start () {
		transforms = new RectTransform[History];
		images = new Image[History];
		texts = new Text[History];

		float Y = 0f;

		LineBackground = template.GetComponentInChildren <Image> () != null;

		RectTransform rekt;
		for (int i = 0; i < History; ++i) {
			rekt = Instantiate <GameObject> (template).GetComponent <RectTransform> ();
			rekt.SetParent (parentTo, false);

			rekt.offsetMax = new Vector2 (Padding.y, Y);
			Y += LineHeight;
			rekt.offsetMin = new Vector2 (Padding.x, Y);

			transforms [i] = rekt;
			texts [i] = rekt.GetComponentInChildren<Text> ();

			if (LineBackground) {
				images [i] = rekt.GetComponentInChildren<Image> ();
			}
		}
	}

	void show(int i, bool show){
		if (LineBackground) {
			images [i].enabled = show;
		}

		texts [i].enabled = show;
	}

	void clear(){
		for (int i = 0; i < Used; ++i) {
			show (i, false);
		}

		Next = 0;
		Used = 0;
	}

	void colortext(Color color){
		texts [Next].color = color;
	}

	void colorbackground(Color color){
		if (LineBackground) {
			images [Next].color = color;
		}
	}

	void line(string line){
		show (Next, true);
		texts [Next].text = line;

		if (Used < History) {
			Used++;

			bool autoscroll = scroll.value == 0f;
			float scrolled = parentTo.offsetMax.y;
			parentTo.offsetMax = new Vector2 (0f, scrolled);
			parentTo.offsetMin = new Vector2 (0f, Used * LineHeight + scrolled);

			if (autoscroll) {
				scroll.value = 1f;
				scroll.value = 0f;
			}

		} else {
			float Y = 0f;

			int a = Next;
			for (int i = 0; i < History; ++i) {
				if (++a >= History) a = 0;

				transforms [a].offsetMax = new Vector2 (Padding.y, Y);
				Y += LineHeight;
				transforms [a].offsetMin = new Vector2 (Padding.x, Y);
			}
		}

		if (++Next >= History) {
			Next = 0;
		}
	}
}
