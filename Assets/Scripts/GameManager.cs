using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public TMP_Text scoreText;

    private int floor = 1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void NextFloor(RoomFirstDG dungeonGenerator)
    {
        dungeonGenerator.GenerateDungeon();
        floor = floor + 1;
        scoreText.text = "Floor : " + floor;
        Debug.Log("next");
    }
}