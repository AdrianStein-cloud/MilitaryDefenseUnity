using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class WaveSpawner : MonoBehaviour {

	public enum SpawnState { SPAWNING, WAITING, PAUSE};

	[System.Serializable]
	public class Wave
	{
		public Transform enemy;
		public int count;
		public float rate;
		public float timeToNextWave;
		public int repeat;
	}

	[System.Serializable]
	public class Round{
		public string name;
		public Wave[] waves;
	}

	public Round[] rounds;
	public TextMeshProUGUI roundTextField;
	public Button startRoundButton;
	public GameObject roundInputField;
	private int nextRound = 0;

	public Button speedUpButton;
	public Sprite speedOn;
	public Sprite speedOff;
	private float maxSpeed = 2f;

	private GameMasterScript gameMasterScript;

	public int NextRound
	{
		get { return nextRound + 1; }
	}

	public Transform[] spawnPoints;
	private float searchCountdown = 1f;

	private SpawnState state = SpawnState.PAUSE;
	public SpawnState State
	{
		get { return state; }
	}

	void Start()
	{
        startRoundButton.gameObject.SetActive(true);
        speedUpButton.gameObject.SetActive(false);

        gameMasterScript = gameObject.GetComponent<GameMasterScript>();

        #if (UNITY_EDITOR)
        roundInputField.SetActive(true);
        #endif

		if (spawnPoints.Length == 0)
		{
			Debug.LogError("No spawn points referenced.");
		}

		UpdateRoundTextField();
	}

	void Update()
	{
		if (state == SpawnState.WAITING)
		{
			if (!EnemyIsAlive())
			{
				WaveCompleted();
			}
			else
			{
				return;
			}
		}
	}

	public void SpeedToggle()
	{
		if(gameMasterScript.speed == 1)
		{
			gameMasterScript.speed = maxSpeed;
			Time.timeScale = gameMasterScript.speed;
			speedUpButton.GetComponent<Image>().sprite = speedOn;
		}
		else if (gameMasterScript.speed == maxSpeed)
		{
            gameMasterScript.speed = 1;
            Time.timeScale = gameMasterScript.speed;
            speedUpButton.GetComponent<Image>().sprite = speedOff;
        }
	}

	public void StartNextRound(){
		startRoundButton.gameObject.SetActive(false);
		speedUpButton.gameObject.SetActive(true);
		if(int.TryParse(roundInputField.GetComponent<TMP_InputField>().text, out int result))
                nextRound = result - 1;
		StartCoroutine( SpawnRound ( rounds[nextRound] ) );
	}

	void WaveCompleted()
	{
		state = SpawnState.PAUSE;
		Debug.Log("Wave Completed!");
		startRoundButton.gameObject.SetActive(true);
        speedUpButton.gameObject.SetActive(false);
        gameMasterScript.UpdateMoney(100 + (nextRound * 10));

		if (nextRound + 1 > rounds.Length - 1)
		{
			nextRound = 0;
			Debug.Log("ALL WAVES COMPLETE! Looping...");
		}
		else
		{
			nextRound++;
		}

		UpdateRoundTextField();
	}

	bool EnemyIsAlive()
	{
		searchCountdown -= Time.deltaTime;
		if (searchCountdown <= 0f)
		{
			searchCountdown = 1f;
			if (GameObject.FindGameObjectWithTag("Enemy") == null)
			{
				return false;
			}
		}
		return true;
	}

	IEnumerator SpawnWave(Wave _wave)
	{
		for (int i = 0; i < _wave.count; i++)
		{
			SpawnEnemy(_wave.enemy);
			yield return new WaitForSeconds( 1f/_wave.rate );
		}
	}

	IEnumerator SpawnRound(Round _round){
		state = SpawnState.SPAWNING;

		foreach(Wave _wave in _round.waves){
			for(int i = _wave.repeat; i >= 0; i--){
				yield return SpawnWave(_wave);
				yield return new WaitForSeconds( _wave.timeToNextWave );
			}
		}

		state = SpawnState.WAITING;

		yield break;
	}

	void SpawnEnemy(Transform _enemy)
	{
		Debug.Log("Spawning Enemy: " + _enemy.name);

		Transform _sp = spawnPoints[ Random.Range (0, spawnPoints.Length) ];
		Enemy enemy = Instantiate(_enemy, _sp.position, _sp.rotation).gameObject.GetComponent<Enemy>();
		float maxHealth = enemy.maxHealth;
		enemy.maxHealth = maxHealth * (1f + (nextRound/StaticSceneClass.CrossSceneDifficulty));
		print("Enemy health: " + enemy.maxHealth);
		enemy.health = enemy.maxHealth;
	}

	void UpdateRoundTextField(){
		roundTextField.text = (nextRound + 1) + "/" + rounds.Length;
	}
}
