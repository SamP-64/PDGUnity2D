using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public TMP_Text floorText;
    [SerializeField] public TextLog textLog;

    public int floor = 1;

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

    public void NextFloor(RoomFirstDG dungeonGenerator) // When the Player moves to the stairs
    {
        dungeonGenerator.GenerateDungeon();
        floor = floor + 1;
        floorText.text = "Floor : " + floor;
        Debug.Log("next");
    }
}