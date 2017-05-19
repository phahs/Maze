using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exit_Zone : MonoBehaviour
{
    public Player player;
    public Game_Controller controller;

    private void OnTriggerEnter(Collider other)
    {
        if(other == player)
        {
            controller.exitFound();
        }
    }
}
