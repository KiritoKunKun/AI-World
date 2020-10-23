using UnityEngine;

public class AnimationsEvents : MonoBehaviour {

	public void ChangeGameTime(GameManager.GameTimes newGameTime) {
		GameManager.gameTime = newGameTime;
	}

	public void SetCanProcreate() {
		GameManager.canProcreate = true;
	}
}
