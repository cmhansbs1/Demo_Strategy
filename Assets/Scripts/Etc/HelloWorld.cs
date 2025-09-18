using UnityEngine;

public class HelloWorld : MonoBehaviour
{
    public int num;

    private void Reset()
    {
        Debug.Log("Reset");
        num = 100;
    }

    private void Awake()
    {
        Debug.Log("Awake");
        num = 0;
    }

    private void OnEnable()
    {
        Debug.Log("OnEnable");
        num = 10;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Start");
        //Ctrl + L
        //Ctr; + D
        num = 100;
    }

    private void OnDisable()
    {
        Debug.Log("OnDisable");
    }

    private void OnDestroy()
    {
        Debug.Log("OnDestroy");
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
