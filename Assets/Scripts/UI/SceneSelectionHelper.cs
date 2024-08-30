using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneSelectionHelper : MonoBehaviour
{
    [SerializeField] private Button lobbyButton;
    [SerializeField] private Button gymButton;
    [SerializeField] private Button dojoButton;
    [SerializeField] private Slider dodgeballCountSlider;
    [SerializeField] private TextMeshProUGUI dodgeballCountText;
    
    private void OnEnable()
    {
        var sceneIndex = SceneManager.GetActiveScene().buildIndex;

        lobbyButton.interactable = sceneIndex != 0;
        gymButton.interactable = sceneIndex != 1;
        dojoButton.interactable = sceneIndex != 2;
    }

    public void SetBalls(float value)
    {
        dodgeballCountText.text = value.ToString();
    }
}
