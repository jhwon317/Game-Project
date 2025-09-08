using UnityEngine;

public class SimpleRotate : MonoBehaviour
{
    Transform myTransform;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        myTransform = GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        myTransform.Rotate(new Vector3(15, 30, 45) * Time.deltaTime);
    }
}
