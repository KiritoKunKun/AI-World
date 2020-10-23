using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class Map : MonoBehaviour {

	public int mapSizeX;
	public int mapSizeY;
	public float scale;
	public int offsetX;
	public int offsetY;

	private bool canInstantiateCreatures;

	public Node[,] map;

	private Tilemap tilemap;
	private Tilemap secondmap;

	private Transform contentsParent;
	public GameObject treePrefab;
	public GameObject foodPrefab;

	private Transform creaturesParent;
	public GameObject creaturePrefab;
	public static List<GameObject> creatures;
	public Texture[] creaturesTextures;

	public static List<Village> villages;

	void Awake() {
		tilemap = GameObject.FindGameObjectWithTag("Tilemap").GetComponent<Tilemap>();
		secondmap = GameObject.FindGameObjectWithTag("Secondmap").GetComponent<Tilemap>();

		contentsParent = GameObject.FindGameObjectWithTag("ContentsParent").transform;
		creaturesParent = GameObject.FindGameObjectWithTag("CreaturesParent").transform;

		creatures = new List<GameObject>();
		villages = new List<Village>();
		canInstantiateCreatures = true;
	}

	void Update() {
		if (canInstantiateCreatures) {
			InstantiateCreatures();
			canInstantiateCreatures = false;
		}
	}

	public void CreateMap() {
		CreateTiles();
		CreateVillages();

		DrawMap();
	}

	private void CreateTiles() {
		map = new Node[mapSizeX, mapSizeY];

		offsetX = Random.Range(0, 99999);
		offsetY = Random.Range(0, 99999);

		int chunkOffsetX = 0;
		int chunkOffsetY = 0;

		for (int i = 0; i < 4; i++) {
			for (int j = 0; j < 4; j++) {
				CreateChunk(chunkOffsetX, chunkOffsetY);
				chunkOffsetX += 50;
			}

			chunkOffsetX = 0;
			chunkOffsetY += 50;
		}
	}

	private void CreateChunk(int chunkOffsetX, int chunkOffsetY) {
		int chunkSize = 50;

		for (int i = chunkOffsetX; i < chunkSize + chunkOffsetX; i++) {
			for (int j = chunkOffsetY; j < chunkSize + chunkOffsetY; j++) {
				map[i, j] = CalculateNoise(i, j);
			}
		}
	}

	private Node CalculateNoise(int x, int y) {
		float xValue = (float)x / mapSizeX * 2f - 1f;
		float yValue = (float)y / mapSizeY * 2f - 1f;

		float xCoord = (float)x / mapSizeX * scale + offsetX;
		float yCoord = (float)y / mapSizeY * scale + offsetY;

		float value = Evaluate(Mathf.Max(Mathf.Abs(xValue), Mathf.Abs(yValue)));
		float perlinNoise = Mathf.PerlinNoise(xCoord, yCoord);
		float noise = Mathf.Clamp01(perlinNoise - value);

		string tileType;

		bool createTree = false;

		if (noise < 0.1f) {
			tileType = TileTypes.OCEAN;
		} else if (noise < 0.2f) {
			tileType = TileTypes.SAND;
		} else if (noise < 0.45f) {
			tileType = TileTypes.DIRT;
		} else if (noise < 0.65f) {
			tileType = TileTypes.GRASS;

			if (noise > 0.54f && noise < 0.5408f) {
				createTree = true;
			}
		} else {
			tileType = TileTypes.WATER;
		}

		Node node = new Node(tileType, x, y);

		if (createTree) {
			node.content = new Content(node, ContentTypes.TREE, false);
		}

		return node;
	}

	private float Evaluate(float value) {
		float a = 3f;
		float b = 2.2f;

		return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
	}

	private void DrawMap() {
		for (int i = 0; i < mapSizeX; i++) {
			for (int j = 0; j < mapSizeY; j++) {
				DrawTile(map[i, j]);

				if (map[i, j].content != null && !map[i, j].hasVillage) {
					CreateTree(map[i, j]);
					//ReplicateTrees(i, j);
				}
			}
		}
	}

	private void DrawTile(Node tile) {
		Tile tileSprite = GetTileSprite("tiles", tile.type, tile.type);
		tilemap.SetTile(new Vector3Int(tile.x, tile.y, 0), tileSprite);
	}

	private Tile GetTileSprite(string root, string type, string sprite) {
		string path = root + "/" + type + "/" + sprite;
		Tile tileSprite = Resources.Load(path) as Tile;

		return tileSprite;
	}

	private void SetTile(Node tile, string type, bool isWalkable = true) {
		tile.type = type;
		tile.isWalkable = isWalkable;
		DrawTile(tile);
	}

	private void CreateTree(Node node) {
		InstantiateTree(node.x, node.y);
	}

	private void InstantiateTree(int x, int y) {
		GameObject go = Instantiate(treePrefab, contentsParent);
		go.transform.localPosition = new Vector2(x, y);
	}

	public void CreateFood(Node node, string type, string sprite) {
		Tile foodSprite = GetTileSprite("food", type, sprite);
		secondmap.SetTile(new Vector3Int(node.x, node.y, 0), foodSprite);
		node.food = new Content(node, type);
	}

	public void RemoveFood(Node node) {
		secondmap.SetTile(new Vector3Int(node.x, node.y, 0), null);
		node.food = null;
	}

	private void ReplicateTrees(int x, int y) {
		int weight = 100;

		for (int mod = 1; mod < 5; mod++) {
			for (int i = -1; i < 2; i++) {
				for (int j = -1; j < 2; j++) {
					if (i == 0 && j == 0) {
						continue;
					}

					int random = Random.Range(0, 100);

					if (random < weight
						&& map[x + (i * mod), y + (j * mod)].content == null
						&& map[x + (i * mod), y + (j * mod)].type == TileTypes.GRASS) {
						CreateTree(map[x + (i * mod), y + (j * mod)]);
						weight -= 12;
					}
				}
			}
		}
	}

	private void CreateVillages() {
		int chunkOffsetX = 0;
		int chunkOffsetY = 0;

		for (int i = 0; i < 3; i++) {
			for (int j = 0; j < 3; j++) {
				int chunkSize = 65;
				bool hasAvailableTile = false;

				for (int x = chunkOffsetX; x < chunkSize + chunkOffsetX; x++) {
					for (int y = chunkOffsetY; y < chunkSize + chunkOffsetY; y++) {
						if (map[x, y].isWalkable &&
							!map[x, y].hasVillage &&
							map[x, y].type == TileTypes.DIRT) {
							hasAvailableTile = true;
						}
					}
				}

				if (hasAvailableTile) {
					int randomX = Random.Range(chunkOffsetX, chunkSize + chunkOffsetX);
					int randomY = Random.Range(chunkOffsetY, chunkSize + chunkOffsetY);

					while (!map[randomX, randomY].isWalkable && map[randomX, randomY].type != TileTypes.DIRT) {
						randomX = Random.Range(chunkOffsetX, chunkSize + chunkOffsetX);
						randomY = Random.Range(chunkOffsetY, chunkSize + chunkOffsetY);
					}

					map[randomX, randomY].hasVillage = true;

					villages.Add(new Village(randomX, randomY));

					Village village = new Village(randomX, randomY);

					int randomSprite = Random.Range(1, 4);

					Tile villageSprite = GetTileSprite("villages", "villages", randomSprite.ToString());
					secondmap.SetTile(new Vector3Int(village.x, village.y, 0), villageSprite);
				}

				chunkOffsetX += 50;
			}

			chunkOffsetX = 0;
			chunkOffsetY += 50;
		}
	}

	private void InstantiateCreatures() {
		for (int i = 0; i < villages.Count; i++) {
			Color color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);

			for (int j = 0; j < 4; j++) {
				Village village = villages[i];

				GameObject go = Instantiate(creaturePrefab, creaturesParent);
				go.transform.localPosition = new Vector2(village.x, village.y);

				Creature creature = go.GetComponent<Creature>();
				creature.gender = j % 2 == 0 ? "male" : "female";
				creature.villagePosition = new Vector3(village.x, village.y);

				creature.size = Random.Range(1f, 2f);
				creature.speed = Random.Range(1f, 5f);
				creature.energyNecessary = (int)(creature.size * creature.speed);
				creature.currentEnergy = 0;
				creature.lookRadius = Random.Range(7, 15);
				creature.eatWeight = Random.Range(40f, 100f);
				creature.attackWeight = Random.Range(5f, 60f);

				creature.gene[0] = creature.size;
				creature.gene[1] = creature.speed;
				creature.gene[2] = creature.lookRadius;
				creature.gene[3] = creature.eatWeight;
				creature.gene[4] = creature.attackWeight;

				creature.body[0] = Random.Range(0, 21);
				creature.body[1] = Random.Range(0, 21);
				creature.body[2] = Random.Range(0, 21);
				creature.body[3] = Random.Range(0, 21);
				creature.body[4] = Random.Range(0, 21);
				creature.body[5] = Random.Range(0, 21);
				creature.body[6] = Random.Range(0, 21);
				creature.body[7]= Random.Range(0, 21);

				creature.color = color;

				villages[i].creatures.Add(creature);

				creatures.Add(go);
			}
		}
	}

	public void GetTileClick() {
		if (!EventSystem.current.IsPointerOverGameObject()) {
			Vector3 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);

			if (point.x >= 0 && point.x < mapSizeX && point.y >= 0 && point.y < mapSizeY) {
				//SetTile(map[(int)point.x, (int)point.y], TileTypes.WATER, false);
				if (creatures == null) {
					return;
				}

				for (int i = 0; i < creatures.Count; i++) {
					if (creatures[i].GetComponent<StateMachine>().currentNode == map[(int)point.x, (int)point.y] ||
						creatures[i].GetComponent<StateMachine>().currentNode == map[(int)point.x, (int)point.y - 1] ||
						creatures[i].GetComponent<StateMachine>().currentNode == map[(int)point.x, (int)point.y - 2] ||
						creatures[i].GetComponent<StateMachine>().currentNode == map[(int)point.x, (int)point.y - 3]) {
						GameManager.targetCreature = creatures[i].transform;
						GameManager.isFollowing = true;

						GameManager.basePanel.Play("In");

						Creature creature = creatures[i].GetComponent<Creature>();
						Transform basePanel = GameManager.basePanel.transform.GetChild(0);
						basePanel.GetChild(0).gameObject.SetActive(true);

						basePanel.GetChild(0).GetChild(0).GetChild(1).GetComponent<Slider>().maxValue = GameManager.maxGenes[0] * GameManager.maxGenes[1];
						basePanel.GetChild(0).GetChild(0).GetChild(1).GetComponent<Slider>().value = creature.energyNecessary;

						basePanel.GetChild(0).GetChild(1).GetChild(1).GetComponent<Slider>().value = creature.size;
						basePanel.GetChild(0).GetChild(2).GetChild(1).GetComponent<Slider>().value = creature.speed;
						basePanel.GetChild(0).GetChild(3).GetChild(1).GetComponent<Slider>().value = creature.lookRadius;

						basePanel.GetChild(0).GetChild(1).GetChild(1).GetComponent<Slider>().maxValue = creature.maxLife;
						basePanel.GetChild(1).GetChild(1).GetChild(1).GetComponent<Slider>().value = creature.currentLife;

						basePanel.GetChild(1).GetChild(1).GetChild(1).GetComponent<Slider>().value = creature.size;
						basePanel.GetChild(1).GetChild(2).GetChild(1).GetComponent<Slider>().value = creature.speed;

						return;
					}
				}
			}
		}
	}

	public List<Node> GetPath(Node currentPosition, Node destination) {
		Node startNode = map[currentPosition.x, currentPosition.y];

		List<Node> openList = new List<Node>();
		List<Node> closedList = new List<Node>();

		openList.Add(startNode);

		while (openList.Count > 0) {
			Node node = openList[0];

			for (int i = 1; i < openList.Count; i++) {
				if (openList[i].fCost <= node.fCost) {
					if (openList[i].hCost < node.hCost) {
						node = openList[i];
					}
				}
			}

			openList.Remove(node);
			closedList.Add(node);

			if (node.x == destination.x && node.y == destination.y) {
				// Finish path
				destination.parent = node.parent;
				return RetracePath(startNode, destination);
			}

			foreach (Node neighbor in GetNeighbors(node)) {
				if (neighbor.isWalkable && !closedList.Contains(neighbor)) {
					int newCostToNeighbor = node.gCost + GetDistance(node, neighbor);

					if (newCostToNeighbor < neighbor.gCost || !openList.Contains(neighbor)) {
						neighbor.gCost = newCostToNeighbor;
						neighbor.hCost = GetDistance(neighbor, destination);
						neighbor.parent = node;

						if (!openList.Contains(neighbor)) {
							openList.Add(neighbor);
						}
					}
				}
			}
		}

		return null;
	}

	private List<Node> GetNeighbors(Node node) {
		List<Node> neighbors = new List<Node>();

		if (node.x - 1 >= 0) {
			neighbors.Add(map[node.x - 1, node.y]);
		}

		if (node.x + 1 < mapSizeX) {
			neighbors.Add(map[node.x + 1, node.y]);
		}

		if (node.y - 1 >= 0) {
			neighbors.Add(map[node.x, node.y - 1]);
		}

		if (node.y + 1 < mapSizeY) {
			neighbors.Add(map[node.x, node.y + 1]);
		}

		return neighbors;
	}

	private int GetDistance(Node a, Node b) =>
		(Mathf.Abs(b.x - a.x) + Mathf.Abs(b.y - a.y)) * 10;

	private List<Node> RetracePath(Node startNode, Node endNode) {
		List<Node> path = new List<Node>();
		Node currentNode = endNode;

		while (currentNode != startNode) {
			path.Add(currentNode);
			currentNode = currentNode.parent;
		}

		path.Reverse();

		return path;
	}
}