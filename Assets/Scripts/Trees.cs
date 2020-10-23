using UnityEngine;

public class Trees : MonoBehaviour {

	private float spawnFruitTime;
	private float timer;

	private Map map;

	void Awake() {
		spawnFruitTime = 5f;
		timer = 0f;

		map = GameObject.FindGameObjectWithTag("Map").GetComponent<Map>();
	}

	// Update is called once per frame
	void Update() {
		timer += Time.deltaTime;
		if (timer > spawnFruitTime) {
			SpawnFruit();

			timer = 0f;
		}

		Render();
	}

	private void SpawnFruit() {
		Vector2Int pos = new Vector2Int((int)transform.localPosition.x, (int)transform.localPosition.y);

		int randomX = Random.Range(-1, 2);
		int randomY = Random.Range(0, 2);
		int randomFruit = Random.Range(0, 4);

		bool hasEmptyTile = false;

		for (int i = -1; i < 2; i++) {
			for (int j = 0; j < 2; j++) {
				if (map.map[pos.x - i, pos.y - j].isWalkable && !map.map[pos.x - i, pos.y - j].hasVillage) {
					hasEmptyTile = true;
				}
			}
		}

		if (!hasEmptyTile) {
			return;
		}

		while (!map.map[pos.x - randomX, pos.y - randomY].isWalkable || 
				map.map[pos.x - randomX, pos.y - randomY].hasVillage) {
			randomX = Random.Range(-1, 2);
			randomY = Random.Range(0, 2);
		}

		map.CreateFood(map.map[pos.x - randomX, pos.y - randomY], FoodTypes.FRUIT, randomFruit.ToString());
	}

	private void Render() {
		if (Vector2.Distance(transform.localPosition, Camera.main.transform.position) > 35) {
			transform.GetChild(0).gameObject.SetActive(false);
		} else {
			transform.GetChild(0).gameObject.SetActive(true);
		}
	}
}
