using System.Collections.Generic;
using UnityEngine;

public class GeneticAlgorithm : MonoBehaviour {

	public GameObject creaturePrefab;
	public Transform creaturesParent;

	List<Creature> maleCreatures = new List<Creature>();
	List<Creature> femaleCreatures = new List<Creature>();

	List<Creature> newCreatures = new List<Creature>();

	private void SortByFitness(Village village) {
		maleCreatures.Clear();
		femaleCreatures.Clear();

		for (int i = 0; i < Map.creatures.Count; i++) {
			if (Map.creatures[i].GetComponent<Creature>().villagePosition == new Vector3(village.x, village.y)) {
				if (Map.creatures[i].GetComponent<Creature>().gender == "male") {
					maleCreatures.Add(Map.creatures[i].GetComponent<Creature>());
				} else {
					femaleCreatures.Add(Map.creatures[i].GetComponent<Creature>());
				}
			}
		}

		maleCreatures.Sort((a, b) => a.GetFitness().CompareTo(b.GetFitness()));
		femaleCreatures.Sort((a, b) => a.GetFitness().CompareTo(b.GetFitness()));
	}

	public void CheckProcreations() {
		for (int i = 0; i < Map.villages.Count; i++) {
			SortByFitness(Map.villages[i]);

			int maxIndex = maleCreatures.Count > femaleCreatures.Count ? femaleCreatures.Count : maleCreatures.Count;
			float modifier = 100f / maxIndex;

			for (int index = 0; index < maxIndex; index++) {
				float chance = 100f;

				if (index > 0) {
					chance = 100f - (modifier * index) - ((maleCreatures[index].lookRadius + femaleCreatures[index].lookRadius) / 2f);
				}

				if (Random.Range(0f, 100f) <= chance) {
					for (int procreationChance = 0; procreationChance < Random.Range(1, 3); procreationChance++) {
						Procreate(maleCreatures[index], femaleCreatures[index]);
					}
				}
			}
		}

		for (int i = 0; i < maleCreatures.Count; i++) {
			if (maleCreatures[i].GetFitness() < 1f) {
				int index = Map.creatures.IndexOf(maleCreatures[i].gameObject);

				Destroy(Map.creatures[index]);
				Map.creatures.RemoveAt(index);

				maleCreatures.RemoveAt(i);
				i--;
			}
		}

		for (int i = 0; i < maleCreatures.Count; i++) {
			if (femaleCreatures[i].GetFitness() < 1f) {
				int index = Map.creatures.IndexOf(femaleCreatures[i].gameObject);

				Destroy(Map.creatures[index]);
				Map.creatures.RemoveAt(index);

				femaleCreatures.RemoveAt(i);
				i--;
			}
		}

		InstantiateNewCreatures();
	}

	private void Procreate(Creature a, Creature b) {
		Creature creature = a.GetFitness() > b.GetFitness() ? a : b;

		if (Random.Range(0f, 100f) < 95f) {
			CrossOver(creature, b);
		} else {
			Mutation(creature);
		}

		for (int i = 0; i < a.body.Length; i++) {
			if (Random.Range(0f, 100f) < 50f) {
				a.body[i] = b.body[i];
			}
		}

		creature.currentEnergy = 0;
		creature.gender = Random.Range(0f, 100f) < 50f ? "male" : "female";

		newCreatures.Add(creature);
	}

	private void CrossOver(Creature a, Creature b) {
		int randomGene = Random.Range(0, a.gene.Length);
		for (int i = 0; i <= randomGene; i++) {
			a.gene[i] = b.gene[i];
		}
	}

	private void Mutation(Creature a) {
		int random = Random.Range(0, a.gene.Length);

		a.gene[random] += Random.Range(-5f, 5f);

		if (a.gene[random] < GameManager.minGenes[random]) {
			a.gene[random] = GameManager.minGenes[random];
		} else if (a.gene[random] > GameManager.maxGenes[random]) {
			a.gene[random] = GameManager.maxGenes[random];
		}
	}

	private void InstantiateNewCreatures() {
		for (int i = 0; i < Map.creatures.Count; i++) {
			Map.creatures[i].GetComponent<Creature>().ResetLife();
		}

		for (int i = 0; i < newCreatures.Count; i++) {
			GameObject go = Instantiate(creaturePrefab, creaturesParent);

			go.GetComponent<Creature>().gender = newCreatures[i].gender;
			go.GetComponent<Creature>().villagePosition = newCreatures[i].villagePosition;

			go.GetComponent<Creature>().size = newCreatures[i].size;
			go.GetComponent<Creature>().speed = newCreatures[i].speed;
			go.GetComponent<Creature>().lookRadius = newCreatures[i].lookRadius;
			go.GetComponent<Creature>().eatWeight = newCreatures[i].eatWeight;
			go.GetComponent<Creature>().gene = newCreatures[i].gene;
			go.GetComponent<Creature>().energyNecessary =
				Mathf.RoundToInt(go.GetComponent<Creature>().size * go.GetComponent<Creature>().speed);
			go.GetComponent<Creature>().currentEnergy = 0;

			go.GetComponent<Creature>().color = newCreatures[i].color;

			go.transform.localPosition = go.GetComponent<Creature>().villagePosition;

			Map.creatures.Add(go);
		}
	}
}
