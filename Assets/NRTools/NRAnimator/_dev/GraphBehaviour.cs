using UnityEngine;

[ExecuteAlways]
public class GraphBehaviour : MonoBehaviour
{
    public AnimationGraph graph;
    protected virtual void OnEnable()
    {
        if (graph == null)
        {
            graph = ScriptableObject.CreateInstance<AnimationGraph>();
        }
    }
}
