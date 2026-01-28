using UnityEngine;

public class AimController : MonoBehaviour
{
    public Transform player;
    public float maxDistance;
    public LayerMask groundLayer;
    public float surfaceOffset = 0.1f;
    bool flipped = false;

    // Update is called once per frame
    void Update()
    {
        Vector2 origin = player.position;
        Vector2 direction = flipped ? Vector2.left : Vector2.right;
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, maxDistance, groundLayer);

        if (hit)
        {
            Vector2 pos = hit.point + hit.normal * surfaceOffset;
            transform.position = pos;
        } else
        {
            transform.position = origin + direction * maxDistance;
        }
    }


    public void Flip()
    {
        Vector3 flipped = new Vector3(transform.position.x * -1, transform.position.y * -1, 1);

        transform.position =  flipped;

        this.flipped = !this.flipped;
    }
}
