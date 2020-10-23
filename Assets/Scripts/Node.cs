public class Node {

	public int gCost;
	public int hCost;
	public int fCost;
	public Node parent;

	public int x;
	public int y;

	public string type;
	public bool isWalkable;
	public bool hasVillage;

	public Content content;
	public Content food;

	public Node(string type, int x, int y) {
		this.type = type;
		this.x = x;
		this.y = y;

		if (type == TileTypes.WATER || type == TileTypes.OCEAN) {
			isWalkable = false;
		} else {
			isWalkable = true;
		}

		hasVillage = false;
	}
}
