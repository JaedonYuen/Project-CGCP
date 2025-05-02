using UnityEngine;

public class spinTest : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public bool active = true;
    public Vector3 rotationVector = new Vector3(0, 0, 1);
    public float speed = 1.0f;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        if (active)
        {
            transform.Rotate(rotationVector * speed * Time.deltaTime, Space.Self);
        }
    }
}
