using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Tile
{
	public int x { get; set; }
	public int y { get; set; }
	public GameObject go { get; set; }
	public Sprite sprite { get; set; }
}

public class Board : MonoBehaviour
{
	GameObject[,] squares = new GameObject[10, 10];
	GameObject[,] dinosaurs = new GameObject[10, 10];
	GameObject[] types = new GameObject[8];

	GameObject currentDino = new GameObject();

	bool attemptSwap = false;

	int rowIdx = 0;
	int colIdx = 0;

	float gameOverTimer;

	GameObject gameOverPanel;
	Text timerText;

	int score;
	GameObject scoreBoard;
	Text scoreText;
	AudioSource music;
	AudioSource match3;
	AudioSource match4;
	AudioSource match5;
	AudioSource boom;

	// Start is called before the first frame update
	void Start()
	{
		gameOverPanel = GameObject.Find("game_over_panel");
		timerText = GameObject.Find("timer_text").GetComponent<Text>();

		scoreBoard = GameObject.Find("score_panel");
		scoreText = GameObject.Find("score_text").GetComponent<Text>();

		music = GameObject.Find("Music").GetComponent<AudioSource>();
		match3 = GameObject.Find("Match3").GetComponent<AudioSource>();
		match4 = GameObject.Find("Match4").GetComponent<AudioSource>();
		match5 = GameObject.Find("Match5").GetComponent<AudioSource>();
		boom = GameObject.Find("Boom").GetComponent<AudioSource>();

		types[0] = GameObject.Find("purple_dinosaur");
		types[1] = GameObject.Find("green_dinosaur");
		types[2] = GameObject.Find("red_dinosaur");
		types[3] = GameObject.Find("orange_dinosaur");
		types[4] = GameObject.Find("teal_dinosaur");
		types[5] = GameObject.Find("blue_dinosaur");
		types[6] = GameObject.Find("pink_dinosaur");
		types[7] = GameObject.Find("egg_dinosaur");

		
		resetBoard();
		highlightSquare(0, 0);
		initGameTimer();
		initScoreBoard();
		updateScore();
		checkAllMatches();
		music.Play();
	}

	// Update is called once per frame
	void Update()
	{

		// deltaTime returns a float
		if (gameOverTimer < 1)
		{
			boom.Play();
			gameOver();
			return;
		}

		gameOverTimer -= Time.deltaTime;
		timerText.text = string.Format("{0:N2}", gameOverTimer);

		if (Input.GetKeyDown(KeyCode.W))
		{
			highlightSquare(rowIdx + 1, colIdx);
		}
		else if (Input.GetKeyDown(KeyCode.S))
		{
			highlightSquare(rowIdx - 1, colIdx);
		}
		else if (Input.GetKeyDown(KeyCode.A))
		{
			highlightSquare(rowIdx, colIdx - 1);
		}
		else if (Input.GetKeyDown(KeyCode.D))
		{
			highlightSquare(rowIdx, colIdx + 1);
		}

		if (Input.GetMouseButtonDown(0))
		{
			Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

			setClicked((int)(mousePos2D.y + 5), (int)(mousePos2D.x + 6));
		}
	}

	void initGameTimer()
	{

		// Set the game over overlay off
		gameOverPanel.SetActive(false);
		gameOverTimer = 30.0f;
	}

	void initScoreBoard()
	{
		//set score to 0 to start
		score = 0;
	}

	void resetBoard()
	{
		GameObject mainSquare = GameObject.Find("AbstractSquare");

		for (int r = 0; r < 10; r++)
		{
			for (int c = 0; c < 10; c++)
			{
				
				squares[r, c] = Instantiate(mainSquare) as GameObject;
				squares[r, c].transform.position = new Vector3((float)(c - 5.5), (float)(r - 4.5), 1f);

				dinosaurs[r, c] = Instantiate(types[(int)Random.Range(0f, 8f)]) as GameObject;
				dinosaurs[r, c].transform.position = new Vector3((float)(c - 5.5), (float)(r - 4.5), -2f);
			}
		}
	}

	void explode(List<Tile> dinos)
	{
		int len = dinos.Count;
		for (int i = 0; i < len; i++)
		{
			dinosaurs[dinos[i].x, dinos[i].y].GetComponent<SpriteRenderer>().sprite = null;
			dinosaurs[dinos[i].x, dinos[i].y].GetComponent<SpriteRenderer>().name = null;
		}
		if (len == 3)
		{
			// trigger sound effect and points
			score += 100; //player has matched three dinosaurs, gets 100 points
			updateScore();
			match3.Play();
		}
		else if (len == 4)
		{
			// trigger better sound effect and points
			score += 200; //player has matched four dinosaurs, gets 200 points
			updateScore();
			match4.Play();

		}
		else
		{
			// trigger even better sound effect and points
			score += 400; //player has matched five or more dinosaurs, gets 400 points
			updateScore();
			match5.Play();
		}
	}


	void fillEmptySquares()
	{

		for (int x = 0; x < 10; x++)
		{

			for (int y = 0; y < 10; y++)
			{
				if (dinosaurs[x, y].GetComponent<SpriteRenderer>().sprite == null)
				{
					//Debug.Log(x + ", " + y + "is null!");
					int i = 1;

					while (x + i <= 9 && dinosaurs[x + i, y].GetComponent<SpriteRenderer>().sprite == null)
					{
						//Debug.Log($"Now checking {x+i}, {y}");
						i++;
					}
					if (x + i <= 9 && dinosaurs[x + i, y].GetComponent<SpriteRenderer>().sprite != null)
					{
						//Debug.Log($"Now setting dino at {x}, {y} to {x+i}, {y}");

						dinosaurs[x, y].GetComponent<SpriteRenderer>().sprite = dinosaurs[x + i, y].GetComponent<SpriteRenderer>().sprite;
						dinosaurs[x, y].GetComponent<SpriteRenderer>().name = dinosaurs[x + i, y].GetComponent<SpriteRenderer>().name;

						dinosaurs[x + i, y].GetComponent<SpriteRenderer>().sprite = null;
						dinosaurs[x + i, y].GetComponent<SpriteRenderer>().name = null;

					}

				}
				if (dinosaurs[x, y].GetComponent<SpriteRenderer>().sprite == null)
				{
					dinosaurs[x, y] = Instantiate(types[(int)Random.Range(0f, 8f)]) as GameObject;
					dinosaurs[x, y].transform.position = new Vector3((float)(y - 5.5), (float)(x - 4.5), -2f);
				}
			}
		}

	}

	bool checkMatch(GameObject d, int r, int c)
	{
		GameObject type = d;
		List<GameObject> matchingTiles = new List<GameObject>();
		List<int> xVals = new List<int>();
		List<int> yVals = new List<int>();

		// check horizontal
		for (int i = c; i >= 0; i--)
		{
			if (dinosaurs[r, i] != null && type != null)
			{
				if (dinosaurs[r, i].name == type.name && !matchingTiles.Contains(dinosaurs[r, i]))
				{
					matchingTiles.Add(dinosaurs[r, i]);
					xVals.Add(r);
					yVals.Add(i);
				}
				else
				{
					break;
				}
			}
		}
		for (int i = c + 1; i < 10; i++)
		{
			if (dinosaurs[r, i] != null && type != null && !matchingTiles.Contains(dinosaurs[r, i]))
			{
				if (dinosaurs[r, i].name == type.name)
				{
					matchingTiles.Add(dinosaurs[r, i]);
					xVals.Add(r);
					yVals.Add(i);
				}
				else
				{
					break;
				}
			}
		}
		var gametiles = new List<Tile>();
		if (matchingTiles.Count > 2)
		{

			for (int i = 0; i < matchingTiles.Count; i++)
			{
				var item = new Tile
				{
					x = xVals[i],
					y = yVals[i],
					go = matchingTiles[i],
					sprite = matchingTiles[i].GetComponent<SpriteRenderer>().sprite

				};

				gametiles.Add(item);
			}

			explode(gametiles);
			fillEmptySquares();

			return true;
		}
		else
		{
			matchingTiles.Clear();
			xVals.Clear();
			yVals.Clear();
			gametiles.Clear();
		}

		//check vertical
		for (int i = r; i >= 0; i--)
		{
			if (dinosaurs[i, c] != null && type != null)
			{
				if (dinosaurs[i, c].name == type.name && !matchingTiles.Contains(dinosaurs[i, c]))
				{
					matchingTiles.Add(dinosaurs[i, c]);
					xVals.Add(i);
					yVals.Add(c);
				}
				else
				{
					break;
				}
			}
		}
		for (int i = r; i < 10; i++)
		{
			if (dinosaurs[i, c] != null && type != null)
			{
				if (dinosaurs[i, c].name == type.name && !matchingTiles.Contains(dinosaurs[i, c]))
				{
					matchingTiles.Add(dinosaurs[i, c]);
					xVals.Add(i);
					yVals.Add(c);
				}
				else
				{
					break;
				}
			}
		}
		if (matchingTiles.Count > 2)
		{

			for (int i = 0; i < matchingTiles.Count; i++)
			{
				var item = new Tile
				{
					x = xVals[i],
					y = yVals[i],
					go = matchingTiles[i],
					sprite = matchingTiles[i].GetComponent<SpriteRenderer>().sprite


				};

				gametiles.Add(item);
			}

			explode(gametiles);
			fillEmptySquares();
			return true;
		}
		return false;
	}



	bool checkAllMatches()
	{
		bool ret = false;
		for (int i = 0; i < 10; i++)
		{
			for (int j = 0; j < 10; j++)
			{
				if (checkMatch(dinosaurs[i, j], i, j))
				{
					ret = true;
				}
			}
		}
		return ret;
	}

	void swapDinosaurs(GameObject first, GameObject second)
	{
		Sprite temp = first.GetComponent<SpriteRenderer>().sprite;
		string name = first.GetComponent<SpriteRenderer>().name;
		first.GetComponent<SpriteRenderer>().sprite = second.GetComponent<SpriteRenderer>().sprite;
		first.GetComponent<SpriteRenderer>().name = second.GetComponent<SpriteRenderer>().name;
		second.GetComponent<SpriteRenderer>().sprite = temp;
		second.GetComponent<SpriteRenderer>().name = name;

		if (!checkAllMatches())
		{
			// swap back
			temp = first.GetComponent<SpriteRenderer>().sprite;
			name = first.GetComponent<SpriteRenderer>().name;
			first.GetComponent<SpriteRenderer>().sprite = second.GetComponent<SpriteRenderer>().sprite;
			first.GetComponent<SpriteRenderer>().name = second.GetComponent<SpriteRenderer>().name;
			second.GetComponent<SpriteRenderer>().sprite = temp;
			second.GetComponent<SpriteRenderer>().name = name;
		}
	}

	void gameOver()
	{
		// Set the game over overlay active
		music.Stop();
		gameOverPanel.SetActive(true);
		timerText.text = "0.00";


		if (Input.GetKeyDown(KeyCode.X))
		{
			
			// Restart board
			initGameTimer();
			music.Play();
		}
	}

	void updateScore()
	{
		scoreText.text = score.ToString();
	}

	void setClicked(int row, int col)
	{

		// unselect anything already selected
		for (int x = 0; x < 10; x++)
		{
			for (int y = 0; y < 10; y++)
			{
				setColor(squares[x, y], Color.white);
			}
		}

		if (!attemptSwap)
		{
			// select new square
			//Debug.Log($"dinosaur type: {dinosaurs[row, col].GetComponent<SpriteRenderer>().name} ");
			setColor(squares[row, col], Color.black);
			currentDino = dinosaurs[row, col];

			if (row > 0)
			{
				setColor(squares[row - 1, col], Color.yellow);
			}
			if (row < 9)
			{
				setColor(squares[row + 1, col], Color.yellow);
			}
			if (col > 0)
			{
				setColor(squares[row, col - 1], Color.yellow);
			}
			if (col < 9)
			{
				setColor(squares[row, col + 1], Color.yellow);
			}
			attemptSwap = true;
		}
		else
		{
			swapDinosaurs(currentDino, dinosaurs[row, col]);

			attemptSwap = false;
		}

	}


	void highlightSquare(int row, int col)
	{
		if (row >= 10 || col >= 10 || row < 0 || col < 0)
		{
			return;
		}

		setColor(squares[rowIdx, colIdx], Color.white);
		rowIdx = row;
		colIdx = col;
	}

	void setColor(GameObject piece, Color color)
	{
		piece.GetComponent<Renderer>().material.SetColor("_Color", color);
	}

}
