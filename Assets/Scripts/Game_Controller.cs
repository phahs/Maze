using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Game_Controller : MonoBehaviour
{
    public static Game_Controller control;

    public Maze mazePrefab;
    public InputField difficulty;
    public Camera menuCamera;
    public Canvas startMenu;

    private bool gameStart;
    private int level;
    private Maze mazeInstance;

    private void Awake()
    {
        control = this;
    }

    // Update is called once per frame
    void Update()
    {
        if(gameStart)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ExitGame();
            }
        }
    }

    private void ExitGame()
    {
        gameStart = false;
        Destroy(mazeInstance.gameObject);
        startMenu.gameObject.SetActive(true);
        menuCamera.gameObject.SetActive(true);

    }

    public void playGame()
    {
        gameStart = true;
        if(difficulty.text == "")
        {
            int rand = Random.Range(0, 100);
            setLevel(rand);
        }
        else
        {
            setLevel(Mathf.Max(0, int.Parse(difficulty.text)));
        }

        mazeInstance = Instantiate(mazePrefab) as Maze;
        mazeInstance.begin(level);
    }

    public void exitFound()
    {
        ExitGame();
    }

    private void setLevel(int lvl)
    {
        level = lvl;
    }
}
