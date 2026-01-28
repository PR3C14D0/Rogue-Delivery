using UnityEngine;

public class Spawner : MonoBehaviour
{
    public int instanceCount = 10;
    public GameObject obj;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for(int i = 0; i < instanceCount; i++)
        {
            Instantiate(obj, transform.position, Quaternion.identity);
        }
    }
}
