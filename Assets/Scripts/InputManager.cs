using UnityEngine;

public class InputManager : MonoBehaviour {

	private Map map;

	void Awake() {
		map = GameObject.FindGameObjectWithTag("Map").GetComponent<Map>();
	}

	void Update() {
		if (Input.GetMouseButtonDown(1)) {
			map.GetTileClick();
		}

		if (GameManager.isFollowing &&
			(Input.GetKeyDown(KeyCode.Escape) ||
			Input.GetAxisRaw("Horizontal") != 0 ||
			Input.GetAxisRaw("Vertical") != 0)) {
			GameManager.isFollowing = false;
			GameManager.basePanel.Play("Out");
		}
	}
}
