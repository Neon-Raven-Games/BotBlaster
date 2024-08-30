using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Gameplay;
using UnityEngine;

public class BallPool : MonoBehaviour
{
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private int poolSize = 8;
    
    private List<Projectile> _pool = new();
    private int _currentIndex;
   private static BallPool _instance;
    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        Initialize().Forget();
    }
    
    public async static UniTaskVoid Initialize()
    {
        await UniTask.DelayFrame(2);
        for (var i = 0; i < _instance.poolSize; i++)
        {
            await UniTask.Yield();
            var ball = Instantiate(_instance.ballPrefab, _instance.transform.position, Quaternion.identity, _instance.transform);
            ball.SetActive(false);
            _instance._pool.Add(ball.GetComponent<Projectile>());
        }
    }

    public static GameObject SetBall(Vector3 position)
    {
        var ball = _instance._pool[_instance._currentIndex];
        if (ball.gameObject.activeInHierarchy)
        {
            ball = _instance._pool.FirstOrDefault(x => !x.gameObject.activeInHierarchy);
            if (!ball) 
            {
                _instance._currentIndex = 0;
                ball = Instantiate(_instance.ballPrefab, _instance.transform).GetComponent<Projectile>();
                ball.gameObject.SetActive(false);
                _instance._pool.Add(ball); 
            }
        }
        
        ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
        ball.transform.position = position;
        _instance._currentIndex = (_instance._currentIndex + 1) % _instance.poolSize;
        return ball.gameObject;
    }

    public static void Sleep()
    {
        _instance._pool.ForEach(ball => ball.gameObject.SetActive(false));
    }

    public static Projectile GetBall(Vector3 position)
    {
        var ball = _instance._pool[_instance._currentIndex];
        if (ball.gameObject.activeInHierarchy)
        {
            ball = _instance._pool.FirstOrDefault(x => !x.gameObject.activeInHierarchy);
            if (!ball) 
            {
                _instance._currentIndex = 0;
                ball = Instantiate(_instance.ballPrefab, _instance.transform).GetComponent<Projectile>();
                ball.gameObject.SetActive(false);
                _instance._pool.Add(ball.GetComponent<Projectile>()); 
            }
        }
        
        ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
        ball.transform.position = position;
        _instance._currentIndex = (_instance._currentIndex + 1) % _instance.poolSize;
        return ball;
    }
}
