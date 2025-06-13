using UnityEngine;
using VContainer;

public class Test : MonoBehaviour
{
    [Inject] 
    private readonly MapLoader _mapLoader;
    
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(_mapLoader);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
