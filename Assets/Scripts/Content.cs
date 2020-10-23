using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Content {

	public int x;
	public int y;

	public Node parent;

	public string type;

	public Content(Node parent, string type, bool isWalkable = true) {
		x = parent.x;
		y = parent.y;
		this.parent = parent;
		this.type = type;

		parent.isWalkable = isWalkable;
	}
}