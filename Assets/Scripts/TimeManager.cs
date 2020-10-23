using UnityEngine;
using UnityEngine.UI;

public class TimeManager : MonoBehaviour {

	public Text dayText;
	public Text timeText;

	// Update is called once per frame
	void Update() {
		dayText.text = "Dia " + GameManager.day;
		timeText.text = ((int)Time.time).ToString();
	}
}
