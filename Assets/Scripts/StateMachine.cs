using System.Collections.Generic;
using UnityEngine;
using DragonBones;

public class StateMachine : MonoBehaviour {

	private enum States { Idle, Eat, Attack, BackToVillage, Sleep, Dying }
	private States currentState;

	private bool isMoving;
	private bool canFind;

	public bool isBeingAttacked;
	private float attackTimer;
	private float attackSpeed;
	private Creature targetCreature;

	private float sleepTimer;

	private Map map;

	public Node currentNode {
		get => map.map[
			(int)transform.localPosition.x,
			(int)transform.localPosition.y
		];
	}

	private Vector2 destination;
	private List<Node> path;

	void Awake() {
		map = GameObject.FindGameObjectWithTag("Map").GetComponent<Map>();

		isMoving = false;
		canFind = false;
		isBeingAttacked = false;
		attackTimer = 0f;
		sleepTimer = 0f;
		currentState = States.Idle;
		path = new List<Node>();
	}

	void Start() {
		attackSpeed = 3f / GetComponent<Creature>().speed;
	}

	void Update() {
		CheckStateAndAct();

		Render();
	}

	private void CheckStateAndAct() {
		if (GameManager.gameTime == GameManager.GameTimes.NIGHT &&
			currentState != States.Sleep && currentState != States.Dying) {
			currentState = States.Sleep;
			isMoving = false;
			path.Clear();
		}

		if (currentState == States.Idle) {
			Find();
		}

		if (isMoving) {
			MoveToNextTile();

			if (transform.GetChild(0).gameObject.activeSelf &&
				GetComponentInChildren<UnityArmatureComponent>().animationName != "walk" &&
				GetComponentInChildren<UnityArmatureComponent>().animation.isCompleted) {
				SetAnimation("walk", -1);
			}
		}

		if (currentState == States.Idle) {
			Idle();
		} else if (currentState == States.Eat) {
			Eat();
		} else if (currentState == States.Attack) {
			Attack();
		} else if (currentState == States.BackToVillage) {
			BackToVillage();
		} else if (currentState == States.Sleep) {
			Sleep();
		}
	}

	private void Idle() {
		if (isMoving) {
			return;
		}

		int posX = (int)GetComponent<Creature>().transform.localPosition.x;
		int posY = (int)GetComponent<Creature>().transform.localPosition.y;
		int lookRadius = GetComponent<Creature>().lookRadius;

		int randomX = Random.Range(posX - lookRadius, posX + lookRadius + 1);
		int randomY = Random.Range(posY - lookRadius, posY + lookRadius + 1);

		while (randomX < 0 || randomX >= map.mapSizeX || randomY < 0 || randomY >= map.mapSizeY ||
			!map.map[randomX, randomY].isWalkable) {
			randomX = Random.Range(posX - lookRadius, posX + lookRadius + 1);
			randomY = Random.Range(posY - lookRadius, posY + lookRadius + 1);
		}

		MoveTo(map.map[randomX, randomY]);
	}

	private void Eat() {
		if (map.map[(int)destination.x, (int)destination.y].food == null) {
			currentState = States.Idle;

			return;
		}

		if (transform.localPosition == new Vector3(destination.x, destination.y)) {
			map.RemoveFood(map.map[(int)destination.x, (int)destination.y]);

			currentState = States.BackToVillage;
		}
	}

	private void Attack() {
		if (targetCreature == null || !targetCreature.transform.GetChild(0).gameObject.activeSelf) {
			currentState = States.Idle;
			return;
		}

		if (Mathf.Abs(transform.localPosition.x - targetCreature.transform.position.x) <= 1f &&
			Mathf.Abs(transform.localPosition.y - targetCreature.transform.position.y) <= 1f) {
			attackTimer += Time.deltaTime;

			if (attackTimer > attackSpeed) {
				if (targetCreature != null) {
					SetAnimation("Attack", 1);
					attackTimer = 0f;

					targetCreature.currentLife -= 3;
					targetCreature.GetComponent<StateMachine>().targetCreature = GetComponent<Creature>();

					if (targetCreature.GetComponent<StateMachine>().currentState != States.Attack) {
						targetCreature.GetComponent<StateMachine>().currentState = States.Attack;
					}

					if (targetCreature.currentLife <= 0) {
						int index = Map.creatures.IndexOf(targetCreature.gameObject);
						Map.creatures[index].GetComponent<StateMachine>().SetAnimation("Dying", 1);
						Map.creatures[index].GetComponent<StateMachine>().isMoving = false;
						Map.creatures[index].GetComponent<StateMachine>().path.Clear();
						Map.creatures[index].GetComponent<StateMachine>().currentState = States.Dying;

						Destroy(Map.creatures[index], 1f);
						Map.creatures.RemoveAt(index);

						isBeingAttacked = false;
						currentState = States.Idle;
						GetComponent<Creature>().currentEnergy += 10;
					}
				}
			}
		} else {
			if (!isMoving) {
				MoveTo(map.map[
					(int)targetCreature.transform.localPosition.x,
					(int)targetCreature.transform.localPosition.y
				]);
			}
		}
	}

	private bool CheckPosition(Vector3 position) => transform.localPosition == position;

	private void BackToVillage() {
		if (transform.localPosition == GetComponent<Creature>().villagePosition) {
			if (GameManager.gameTime == GameManager.GameTimes.DAY) {
				if (sleepTimer == 0f) {
					GetComponent<Creature>().currentEnergy += 15;
				}

				sleepTimer += Time.deltaTime;
				if (sleepTimer > 3f) {
					currentState = States.Idle;

					sleepTimer = 0f;
				}
			}
		} else {
			if (!isMoving) {
				MoveTo(map.map[
					(int)GetComponent<Creature>().villagePosition.x,
					(int)GetComponent<Creature>().villagePosition.y
				]);
			}
		}
	}

	private void Sleep() {
		if (GameManager.gameTime == GameManager.GameTimes.DAY) {
			currentState = States.Idle;
			GetComponent<Creature>().currentSpeed = GetComponent<Creature>().speed;

			return;
		}

		if (transform.localPosition != GetComponent<Creature>().villagePosition) {
			if (!isMoving) {
				MoveTo(map.map[
					(int)GetComponent<Creature>().villagePosition.x,
					(int)GetComponent<Creature>().villagePosition.y
				]);

				GetComponent<Creature>().currentSpeed = 25f;
			}
		}
	}

	private void Find() {
		if (!canFind) {
			return;
		}

		canFind = false;

		Creature creature = GetComponent<Creature>();
		int posX = (int)creature.transform.localPosition.x;
		int posY = (int)creature.transform.localPosition.y;
		int lookRadius = creature.lookRadius;

		for (int mod = 1; mod <= lookRadius; mod++) {
			for (int i = -1; i < 2; i++) {
				for (int j = -1; j < 2; j++) {
					if (i != 0 || j != 0) {
						if (posX + (i * mod) >= 0 && posX + (i * mod) < map.mapSizeX
						&& posY + (j * mod) >= 0 && posY + (j * mod) < map.mapSizeY) {
							if (currentState != States.BackToVillage) {
								// Eat
								if (map.map[posX + (i * mod), posY + (j * mod)].isWalkable &&
									map.map[posX + (i * mod), posY + (j * mod)].food != null) {
									if (Random.Range(0f, 100f) < GetComponent<Creature>().eatWeight) {
										if (!CheckPosition(new Vector3(posX + (i * mod), posY + (j * mod)))) {
											MoveTo(map.map[posX + (i * mod), posY + (j * mod)]);
										}

										currentState = States.Eat;

										return;
									}
								}

								// Attack
								for (int creatureIndex = 0; creatureIndex < Map.creatures.Count; creatureIndex++) {
									if (Map.creatures[creatureIndex].GetComponent<StateMachine>().currentNode.x == posX + (i * mod) &&
										Map.creatures[creatureIndex].GetComponent<StateMachine>().currentNode.y == posY + (j * mod)) {
										if (Map.creatures[creatureIndex].GetComponent<Creature>().villagePosition != GetComponent<Creature>().villagePosition) {
											if (Random.Range(0f, 100f) < GetComponent<Creature>().attackWeight) {
												targetCreature = Map.creatures[creatureIndex].GetComponent<Creature>();
												currentState = States.Attack;
												targetCreature.GetComponent<StateMachine>().isBeingAttacked = true;
												isMoving = false;
											}
										}

										return;
									}
								}
							}
						}
					}
				}
			}
		}
	}

	private void MoveTo(Node destinationNode) {
		path = map.GetPath(currentNode, destinationNode);

		if (path == null || path.Count == 0) {
			return;
		}

		destination = new Vector2(
			path[path.Count - 1].x,
			path[path.Count - 1].y
		);

		isMoving = true;
	}

	private void MoveToNextTile() {
		if (transform.localPosition == new Vector3(destination.x, destination.y) || path.Count == 0) {
			isMoving = false;
			return;
		}

		if (transform.GetChild(0).gameObject.activeSelf) {
			GetComponentInChildren<UnityArmatureComponent>().armature.flipX = transform.localPosition.x > path[0].x;
		}

		transform.localPosition = Vector3.MoveTowards(
			transform.localPosition,
			new Vector2(path[0].x, path[0].y),
			GetComponent<Creature>().currentSpeed * Time.deltaTime
		);

		if (transform.localPosition == new Vector3(path[0].x, path[0].y)) {
			path.RemoveAt(0);

			canFind = true;
		}
	}

	private void Render() {
		if (Vector2.Distance(transform.localPosition, Camera.main.transform.position) > 35) {
			transform.GetChild(0).gameObject.SetActive(false);
		} else {
			if (transform.localPosition == new Vector3(
					GetComponent<Creature>().villagePosition.x,
					GetComponent<Creature>().villagePosition.y
				)) {
				transform.GetChild(0).gameObject.SetActive(false);

				return;
			}

			transform.GetChild(0).gameObject.SetActive(true);
		}
	}

	public void SetAnimation(string anim, int playTimes) {
		if (transform.GetChild(0).gameObject.activeSelf &&
			GetComponentInChildren<UnityArmatureComponent>().animationName != anim) {
			GetComponentInChildren<UnityArmatureComponent>().animation.Play(anim, playTimes);
			GetComponentInChildren<UnityArmatureComponent>().animation.timeScale = GetComponent<Creature>().currentSpeed;
		}
	}
}
