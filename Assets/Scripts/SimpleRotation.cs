using UnityEngine;

public class SimpleRotation : MonoBehaviour
{
    enum Axis{X,Y,Z};
    [SerializeField] private float rotationSpeed;
    [SerializeField] private Axis axis;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(axis == Axis.X)
        {
            this.gameObject.transform.Rotate(rotationSpeed*Time.deltaTime, 0,0);
        }
        else if(axis == Axis.Y)
        {
            this.gameObject.transform.Rotate(0,rotationSpeed*Time.deltaTime, 0);
        }
        else
        {
            this.gameObject.transform.Rotate(0,0,rotationSpeed*Time.deltaTime);
        }
    }
}

