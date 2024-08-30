using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    
    public static int teamOneScore;
    public static int teamTwoScore;


    private void Start()
    {
        if (!_instance)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        ResetScore();
    }

    public static void AddScore(float score)
    {

        UpdateScore();
    }


    
    public static void UpdateScore()
    {
        // update score gui
    
    }
    
    public void ResetScore()
    {
        // set score to 0
        UpdateScore();
    }
}
