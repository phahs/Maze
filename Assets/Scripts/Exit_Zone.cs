﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exit_Zone : MonoBehaviour
{
    public Player player;

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == player.tag)
        {
            Game_Controller.control.exitFound();
        }
    }
}
