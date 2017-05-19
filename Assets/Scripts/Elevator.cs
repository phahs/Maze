using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator : MonoBehaviour
{
    public GameObject rail;
    public GameObject platform;

    public bool top;
    public bool bottom;
    public bool pause;
    private float start;
    private float distance;
    private float wait = 10f;
    public float timer;

    // Use this for initialization
    void Start()
    {
        start = platform.transform.localPosition.y;
        distance = rail.transform.localScale.y;
        top = false;
        bottom = false;
        pause = false;
        timer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (!pause)
        {
            if (top)
            {
                platform.transform.localPosition -= new Vector3(0, distance * Time.deltaTime, 0);
            }

            if (bottom)
            {
                platform.transform.localPosition += new Vector3(0, distance * Time.deltaTime, 0);
            }
        }
        else if(pause)
        {
            timer += Time.deltaTime;
        }

        if (start >= platform.transform.localPosition.y)
        {
            bottom = true;
            pause = true;
            top = false;
        }
        if (start + distance <= platform.transform.localPosition.y)
        {
            top = true;
            pause = true;
            bottom = false;
        }

        if (timer >= wait)
        {
            pause = false;
            timer = 0f;
        }
    }
}
