using UnityEngine;

public class TireRotation : MonoBehaviour
{
    public float speed = 2f;
    private float timer = 0f;
    private bool canRotate = false;

    void Update()
    {
        if (!canRotate)
        {
            timer += Time.deltaTime;
            if (timer >= 5f)
            {
                canRotate = true;
            }
        }

        if (canRotate)
        {
            transform.Rotate(speed, 0f, 0f);
        }
    }
}