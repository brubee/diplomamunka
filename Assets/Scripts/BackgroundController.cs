using UnityEngine;

public class BackgroundController : MonoBehaviour
{
    private float startX, startY, length;
    public GameObject cam;
    public float parallaxEffect;
    public float verticalEffect;

    void Start()
    {
        startX = transform.position.x;
        startY = transform.position.y;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void LateUpdate()
    {
        float camX = cam.transform.position.x;
        float camY = cam.transform.position.y;

        float distanceX = camX * parallaxEffect;
        float movementX = camX * (1 - parallaxEffect);

        float distanceY = camY * verticalEffect;

        transform.position = new Vector3(startX + distanceX, startY + distanceY, transform.position.z);

        if (movementX > startX + length)
        {
            startX += length;
        }
        else if (movementX < startX - length)
        {
            startX -= length;
        }
    }
}
