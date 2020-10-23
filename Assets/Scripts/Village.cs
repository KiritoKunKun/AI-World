using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Village {

	public int x;
	public int y;

	public List<Creature> creatures;

	public Village(int x, int y) {
		this.x = x;
		this.y = y;

		creatures = new List<Creature>();
	}
}
