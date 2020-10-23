using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

	public enum GameTimes { DAY, NIGHT }
	public static GameTimes gameTime;

	private static bool canPlayDayAlertAnimation;
	public static bool canProcreate;

	public float cameraSpeed;

	public static Map map;

	public GameObject dayControl;
	public static int day;

	public static float[] minGenes;
	public static float[] maxGenes;

	public static Transform targetCreature;
	public static bool isFollowing;
	public static Animator basePanel;

	public static Animator dayAlert;

	void Awake() {
		map = GameObject.FindGameObjectWithTag("Map").GetComponent<Map>();

		gameTime = GameTimes.DAY;

		canPlayDayAlertAnimation = false;
		canProcreate = false;

		day = 1;

		minGenes = new float[5];
		minGenes[0] = 1;
		minGenes[1] = 1;
		minGenes[2] = 7;
		minGenes[3] = 0.1f;
		minGenes[4] = 0.1f;

		maxGenes = new float[5];
		maxGenes[0] = 2;
		maxGenes[1] = 5;
		maxGenes[2] = 14;
		maxGenes[3] = 100;
		maxGenes[4] = 100;

		isFollowing = false;
		basePanel = GameObject.FindGameObjectWithTag("BasePanel").GetComponent<Animator>();

		dayAlert = GameObject.FindGameObjectWithTag("DayAlert").GetComponent<Animator>();
	}

	void Start() {
		map.CreateMap();
		Camera.main.transform.position = new Vector3(100, 100, -10);
	}

	void Update() {
		//MoveCameraFromBorders();
		MoveCameraFromKeyboard();
		ZoomCamera();

		if (isFollowing && targetCreature != null) {
			FollowCreature();
		} else {
			isFollowing = false;
		}

		if (canProcreate) {
			GetComponent<GeneticAlgorithm>().CheckProcreations();
			canProcreate = false;

			dayControl.GetComponent<Animator>().Play("DayControlAnimation");
		}

		if (gameTime == GameTimes.NIGHT) {
			Debug.Log(Map.creatures.Count);
			if (Map.creatures == null || Map.creatures.Count == 0) {
				return;
			}

			int creaturesCount = 0;

			for (int i = 0; i < Map.creatures.Count; i++) {
				if (new Vector3(
					(int)Map.creatures[i].transform.localPosition.x,
					(int)Map.creatures[i].transform.localPosition.y
					) == new Vector3(
						(int)Map.creatures[i].GetComponent<Creature>().villagePosition.x,
						(int)Map.creatures[i].GetComponent<Creature>().villagePosition.y
					)) {
					creaturesCount++;
				}
			}

			if (creaturesCount == Map.creatures.Count) {
				if (canPlayDayAlertAnimation) {
					day++;
					dayAlert.Play("DayAlertAnim");
					dayAlert.transform.GetChild(0).GetComponent<Text>().text = "Dia " + day;

					canPlayDayAlertAnimation = false;
				}
			}
		} else {
			canPlayDayAlertAnimation = true;
		}
	}

	private void MoveCameraFromBorders() {
		Vector3 point = Camera.main.ScreenToViewportPoint(Input.mousePosition);

		// TODO - Adicionar um limite

		if (point.x >= 0f && point.x < 0.1f) {
			Camera.main.transform.position -= new Vector3(cameraSpeed * Time.deltaTime, 0, 0);
		}

		if (point.x > 0.9f && point.x <= 1f) {
			Camera.main.transform.position += new Vector3(cameraSpeed * Time.deltaTime, 0, 0);
		}

		if (point.y >= 0f && point.y < 0.1f) {
			Camera.main.transform.position -= new Vector3(0, cameraSpeed * Time.deltaTime, 0);
		}

		if (point.y > 0.9f && point.y <= 1f) {
			Camera.main.transform.position += new Vector3(0, cameraSpeed * Time.deltaTime, 0);
		}
	}

	private void ZoomCamera() {
		Camera.main.orthographicSize -= Input.mouseScrollDelta.y;

		if (Camera.main.orthographicSize < 1) {
			Camera.main.orthographicSize = 1;
		}

		if (Camera.main.orthographicSize > 18) {
			Camera.main.orthographicSize = 18;
		}
	}

	private void MoveCameraFromKeyboard() {
		Camera.main.transform.position +=
			new Vector3(
				Input.GetAxisRaw("Horizontal") * cameraSpeed * Camera.main.orthographicSize * Time.deltaTime,
				Input.GetAxisRaw("Vertical") * cameraSpeed * Camera.main.orthographicSize * Time.deltaTime
			);
	}

	private void FollowCreature() {
		Camera.main.transform.position = new Vector3(targetCreature.position.x, targetCreature.position.y, -10);
	}
}
