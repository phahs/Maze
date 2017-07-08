using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Light_Change : MonoBehaviour {

    public Player player;
    public Light light;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == player.tag)
        {
            light.color = Color.green;
        }
    }
}
