using UnityEngine;


public class ConfigurationManager : MonoBehaviour
{
    private static ConfigurationManager _instance;
    [SerializeField] public AudioClip blasterSound;
    

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static AudioClip GetBlasterSound() =>
        _instance.blasterSound;
    
}
