using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Board : MonoBehaviour
{
    GameObject[,] squares = new GameObject[10,10];
    GameObject[,] dinosaurs = new GameObject[10,10];
    GameObject[] types = new GameObject[8];
    
    int rowIdx = 0;
    int colIdx = 0;

    // 10 second counter
    float gameOverTimer;

    GameObject gameOverPanel;
    Text timerText;

    // Start is called before the first frame update
    void Start() {
        gameOverPanel = GameObject.Find("game_over_panel");
        timerText = GameObject.Find("timer_text").GetComponent<Text>();

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
    }

    // Update is called once per frame
    void Update() {
        // deltaTime returns a float
        if ( gameOverTimer < 0 )
        {
            gameOver();
            return;
        }

        gameOverTimer -= Time.deltaTime;
        timerText.text = string.Format("{0:N2}", gameOverTimer);

        if ( Input.GetKeyDown(KeyCode.W) ) {
            highlightSquare(rowIdx+1, colIdx);
        } else if ( Input.GetKeyDown(KeyCode.S) ) {
            highlightSquare(rowIdx-1, colIdx);
        } else if ( Input.GetKeyDown(KeyCode.A) ) {
            highlightSquare(rowIdx, colIdx-1);
        } else if ( Input.GetKeyDown(KeyCode.D) ) {
            highlightSquare(rowIdx, colIdx+1);
        }
        
        if (Input.GetMouseButtonDown(0)) {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            setClicked((int) (mousePos2D.y+5), (int) (mousePos2D.x+6));
        }
    }

    void initGameTimer()
    {
        // Set the game over overlay off
        gameOverPanel.SetActive(false);
        gameOverTimer = 10.0f;
    }
    
    void resetBoard() {
        GameObject mainSquare = GameObject.Find("AbstractSquare");
        GameObject mainPurpleDinosaur = GameObject.Find("purple_dinosaur");
        
        for (int r = 0; r < 10; r++) {
            for (int c = 0; c < 10; c++) {
                squares[r,c] = Instantiate(mainSquare) as GameObject;
                squares[r,c].transform.position = new Vector3((float) (c-5.5), (float) (r-4.5), 1f);

                dinosaurs[r,c] = Instantiate(types[(int)Random.Range(0f, 8f)]) as GameObject;
                dinosaurs[r,c].transform.position = new Vector3((float) (c-5.5), (float) (r-4.5), -2f);
            }
        }
    }

    void gameOver()
    {
        // Set the game over overlay active
        gameOverPanel.SetActive(true);

        timerText.text = "0.00";

        if (Input.GetKeyDown(KeyCode.X))
        {
            // Restart board
            initGameTimer();
        }
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

        // select new square
        setColor(squares[row, col], Color.black);

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

    }
    
    void highlightSquare(int row, int col) {
        if (row >= 10 || col >= 10 || row < 0 || col < 0) {
            return;
        }

        setColor(squares[rowIdx, colIdx], Color.white);
        setColor(squares[row, col], Color.red);
        rowIdx = row;
        colIdx = col;
    }
    
    void setColor(GameObject piece, Color color) {
        piece.GetComponent<Renderer>().material.SetColor("_Color", color);
    }

}
