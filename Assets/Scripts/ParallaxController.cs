using UnityEngine;

public class ParallaxController : MonoBehaviour
{
    private float startPos;
    private float length;
    public GameObject cam;
    public float parallaxEffect;
    public float cameraOffsetY = 1.2f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPos = transform.position.x;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float distance = cam.transform.position.x * parallaxEffect;
        float movement = cam.transform.position.x * (1 - parallaxEffect);

        transform.position = new Vector3(startPos + distance, cam.transform.position.y - cameraOffsetY, transform.position.z);

        if (movement > startPos + length) startPos += length;
        else if (movement < startPos - length) startPos -= length;
    }
}
