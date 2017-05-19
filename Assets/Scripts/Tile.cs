using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    //false means there is a wall/ true means there is no wall
    private bool down;
    private bool north;
    private bool east;
    private bool south;
    private bool west;
    private bool up;
    private int indexX;
    private int indexY;
    private int indexZ;

    private void Start()
    {
        down = false;
        north = false;
        east = false;
        south = false;
        west = false;
        up = false;
    }

    public void removeWall(int dir)
    {
        switch (dir)
        {
            case 0:
                down = true;
                Destroy(transform.GetChild(dir).gameObject);
                break;
            case 1:
                north = true;
                Destroy(transform.GetChild(dir).gameObject);
                break;
            case 2:
                east = true;
                Destroy(transform.GetChild(dir).gameObject);
                break;
            case 3:
                south = true;
                Destroy(transform.GetChild(dir).gameObject);
                break;
            case 4:
                west = true;
                Destroy(transform.GetChild(dir).gameObject);
                break;
            case 5:
                up = true;
                Destroy(transform.GetChild(dir).gameObject);
                break;
        }
    }

    public bool checkWalls(int dir)
    {
        switch (dir)
        {
            case 0:
                return down;
            case 1:
                return north;
            case 2:
                return east;
            case 3:
                return south;
            case 4:
                return west;
            case 5:
                return up;
            default:
                return false;
        }
    }

    public void storeIndecies(int x, int y, int z)
    {
        indexX = x;
        indexY = y;
        indexZ = z;
    }

    public void getIndecies(ref int x, ref int y, ref int z)
    {
        x = indexX;
        y = indexY;
        z = indexZ;
    }
}
