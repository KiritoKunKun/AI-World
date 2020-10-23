using UnityEngine;

public interface Gene {
	float size { get; set; }
	float speed { get; set; }
	int lookRadius { get; set; }

	float eatWeight { get; set; }
	float attackWeight { get; set; }
}

public class Creature : MonoBehaviour, Gene {

	public float[] gene;

	public int[] body;
	public Color color { get; set; }

	// Weights
	public float eatWeight { get; set; }
	public float attackWeight { get; set; }

	public int maxLife { get; set; }
	public int currentLife { get; set; }

	public int energyNecessary { get; set; }
	public int currentEnergy { get; set; }

	public float size { get; set; }

	public float speed { get; set; }
	public float currentSpeed { get; set; }

	public int lookRadius { get; set; }

	public string gender { get; set; }

	public Vector3 villagePosition { get; set; }

	void Awake() {
		gene = new float[5];
		body = new int[8];
	}

	void Start() {
		SetGenes();
		SetBody();
	}

	public float GetFitness() {
		return currentEnergy / (float)energyNecessary;
	}

	private void SetGenes() {
		size = gene[0];
		speed = gene[1];
		lookRadius = (int)gene[2];
		eatWeight = gene[3];
		attackWeight = gene[4];

		currentSpeed = speed;

		maxLife = Mathf.RoundToInt(size * speed);
		currentLife = maxLife;

		energyNecessary = (int)(size * speed);
		currentEnergy = 0;
	}

	private void SetBody() {
		Texture[] textures = GameObject.FindGameObjectWithTag("Map").GetComponent<Map>().creaturesTextures;
		transform.GetChild(0).FindChild("000").GetComponent<Renderer>().material.mainTexture = textures[body[0]];
		transform.GetChild(0).FindChild("001").GetComponent<Renderer>().material.mainTexture = textures[body[1]];
		transform.GetChild(0).FindChild("002").GetComponent<Renderer>().material.mainTexture = textures[body[2]];
		transform.GetChild(0).FindChild("003").GetComponent<Renderer>().material.mainTexture = textures[body[3]];
		transform.GetChild(0).FindChild("004").GetComponent<Renderer>().material.mainTexture = textures[body[4]];
		transform.GetChild(0).FindChild("005").GetComponent<Renderer>().material.mainTexture = textures[body[5]];
		transform.GetChild(0).FindChild("006").GetComponent<Renderer>().material.mainTexture = textures[body[6]];
		transform.GetChild(0).FindChild("007").GetComponent<Renderer>().material.mainTexture = textures[body[7]];

		transform.GetChild(0).FindChild("000").GetComponent<Renderer>().material.color = color;
		transform.GetChild(0).FindChild("001").GetComponent<Renderer>().material.color = color;
		transform.GetChild(0).FindChild("002").GetComponent<Renderer>().material.color = color;
		transform.GetChild(0).FindChild("003").GetComponent<Renderer>().material.color = color;
		transform.GetChild(0).FindChild("004").GetComponent<Renderer>().material.color = color;
		transform.GetChild(0).FindChild("005").GetComponent<Renderer>().material.color = color;
		transform.GetChild(0).FindChild("006").GetComponent<Renderer>().material.color = color;

		transform.localScale = new Vector3(size, size, size);
	}

	public void ResetLife() {
		currentLife = maxLife;
	}
}
