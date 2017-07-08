using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <Issues to look into>
/// Check every instance of RemoveWall being called. want to fix the random wall being destroyed bug
/// most likely it exists due to how the loop mechanic works. 
/// </summary>
public class Maze : MonoBehaviour
{
    //array of area
    public Tile[,,] map;

    //prefabs
    public Tile tilePrefab;
    public Elevator elevatorPrefab;
    public Player playerPrefab;
    public GameObject exitSign;
    public GameObject ceilingLight;

    //stack for backtracking
    public static Stack<Tile> tileList = new Stack<Tile>();

    //maze size limits
    private int sizeMin = 20;
    private int totalUnits = 10000;
    private int usableUnits;

    //hierarchy stuff
    GameObject floors;

    public void begin(int difficulty)
    {
        int numFloors = 1;
        int size = difficulty * difficulty;
        size += sizeMin * sizeMin;
        size += sizeMin * difficulty;

        usableUnits = Mathf.Min(size, totalUnits); //at difficulty = 458 size will be greater than totalUnits
        size /= totalUnits;

        int unitsPerSide = Mathf.FloorToInt(Mathf.Sqrt(usableUnits));
        int maxNumFloor = unitsPerSide / sizeMin;
        // determine if there will be more than 1 floor. minimum 25% chance to gain a floor.
        for (int i = 1; i < maxNumFloor; i++)
        {
            float roll = Random.Range(0.0f, 100f);

            if(roll > 25f)
            {
                numFloors += 1;
            }
        }

        unitsPerSide /= numFloors;

        buildMaze(unitsPerSide, numFloors, difficulty);

        placePlayer(unitsPerSide, numFloors);
    }

    private void buildMaze(int uPS, int nF, int diff)
    {
        setUpArray(uPS, nF);
        
        int x = 0;
        int y = 0;
        int z = 0;

        createExitTile(ref x, ref y, ref z, uPS, nF);

        while (tileList.Count != 0)
        {
            //check if tile already exists
            if(map[x, z, y] == null)
            {
                //create new tile at this location
                createNextTile(ref x, ref y, ref z, uPS, nF);
            }
            else if(map[x, z, y] == tileList.Peek())
            {
                //determine what to do: place an elevator, remove the dead end via looping, or nothing
                //base weight for each occurance
                int nothingWeight = 20;
                int elevatorWeight = 15;
                int loopWeight = 5;

                int difference = nothingWeight - loopWeight;

                //modifying weight based on difficulty, nothing is affected inversely to difficulty, and loop is affected proportionately to difficulty
                float nothingMod = nothingWeight - ((difference / 458) * Mathf.Min(diff, 458));
                float loopMod = loopWeight + ((difference / 458) * Mathf.Min(diff, 458));

                float totalWeight = nothingMod + elevatorWeight + loopMod;

                float nothingChance = nothingMod / totalWeight;
                float elevatorChance = elevatorWeight / totalWeight;
                float loopChance = loopMod / totalWeight;

                //set limiters for each instance
                float nothingLimit = nothingChance;
                float elevatorLimit = nothingLimit + elevatorChance;
                float loopLimit = elevatorLimit + loopChance;

                float roll = Random.Range(0f, 1f);

                if (roll < nothingLimit)
                {
                    //backtrack
                    backTrack(ref x, ref y, ref z, uPS, nF);
                }
                else if (roll >= nothingLimit && roll < elevatorLimit)
                {
                    if (nF > 1)
                    {
                        //build elevator
                        int[] upOrDown = { 0, 5 };
                        for (int i = 0; i < upOrDown.Length; i++)
                        {
                            int rand = Random.Range(i, upOrDown.Length);

                            if (validTile(upOrDown[rand], ref x, ref y, ref z, uPS, nF))
                            {
                                //if up, place wall now before new tile is created and the stack changes
                                if (upOrDown[rand] == 5)
                                {
                                    int[] walls = { 1, 2, 3, 4 };
                                    for (int k = 0; k < walls.Length; k++)
                                    {
                                        int wallCheck = Random.Range(k, walls.Length);
                                        if (!tileList.Peek().checkWalls(walls[wallCheck]))
                                        {
                                            //place elevator prefab here
                                            Elevator newLift = Instantiate(elevatorPrefab) as Elevator;
                                            newLift.name = "Elevator";
                                            newLift.transform.parent = tileList.Peek().transform;
                                            newLift.transform.localEulerAngles = new Vector3(0, 90 * (walls[wallCheck] - 1), 0);
                                            newLift.transform.localPosition = new Vector3(0, 0, 0);
                                            removeCeilingLight();
                                            break;
                                        }
                                        walls[wallCheck] = walls[k];
                                    }
                                }
                                //remove appropriate tile (floor or cieling)
                                tileList.Peek().removeWall(upOrDown[rand]);

                                //create the new tile
                                createNextTile(ref x, ref y, ref z, uPS, nF);

                                //if down, place wall after the stack is updated
                                if (upOrDown[rand] == 0)
                                {
                                    int[] walls = { 1, 2, 3, 4 };
                                    for (int k = 0; k < walls.Length; k++)
                                    {
                                        int wallCheck = Random.Range(k, walls.Length);
                                        if (!tileList.Peek().checkWalls(walls[wallCheck]))
                                        {
                                            //place elevator prefab here
                                            Elevator newLift = Instantiate(elevatorPrefab) as Elevator;
                                            newLift.name = "Elevator";
                                            newLift.transform.parent = tileList.Peek().transform;
                                            newLift.transform.localEulerAngles = new Vector3(0, 90 * (walls[wallCheck] - 1), 0);
                                            newLift.transform.localPosition = new Vector3(0, 0, 0);
                                            removeCeilingLight();
                                            break;
                                        }
                                        walls[wallCheck] = walls[k];
                                    }
                                }
                            }
                            upOrDown[rand] = upOrDown[i];
                        }
                    }
                    else
                    {
                        backTrack(ref x, ref y, ref z, uPS, nF);
                    }
                }
                else if (roll >= elevatorLimit && roll < loopLimit)
                {
                    //create loop
                    int topX = 0;
                    int topY = 0;
                    int topZ = 0;

                    tileList.Peek().getIndecies(ref topX, ref topY, ref topZ);

                    int rand;
                    int[] directions = { 1, 2, 3, 4 };

                    for (int k = 0; k < directions.Length; k++)
                    {
                        int tempX = topX;
                        int tempY = topY;
                        int tempZ = topZ;
                        bool wallRemoved = false;
                        rand = Random.Range(k, directions.Length);
                        
                        switch (directions[rand])
                        {
                            case 1:
                                tempZ += 1;
                                if (tempZ < uPS)
                                {
                                    //this direction is inside the array
                                    tileList.Peek().removeWall(directions[rand]);
                                    rand = 2;
                                    map[tempX, tempZ, tempY].removeWall(directions[rand]);
                                    wallRemoved = true;
                                }
                                break;
                            case 2:
                                tempX += 1;
                                if (tempX < uPS)
                                {
                                    //this direction is inside the array
                                    tileList.Peek().removeWall(directions[rand]);
                                    rand = 3;
                                    map[tempX, tempZ, tempY].removeWall(directions[rand]);
                                    wallRemoved = true;
                                }
                                break;
                            case 3:
                                tempZ -= 1;
                                if (tempZ > -1)
                                {
                                    //this direction is inside the array
                                    tileList.Peek().removeWall(directions[rand]);
                                    rand = 0;
                                    map[tempX, tempZ, tempY].removeWall(directions[rand]);
                                    wallRemoved = true;
                                }
                                break;
                            case 4:
                                tempX -= 1;
                                if (tempX > -1)
                                {
                                    //this direction is inside the array
                                    tileList.Peek().removeWall(directions[rand]);
                                    rand = 1;
                                    map[tempX, tempZ, tempY].removeWall(directions[rand]);
                                    wallRemoved = true;
                                }
                                break;
                        }
                        if(!wallRemoved)
                        {
                            directions[rand] = directions[k];
                        }
                        else
                        {
                            break;
                        }
                        
                    }
                    //backtrack I think;
                    backTrack(ref x, ref y, ref z, uPS, nF);
                }
            }
        }
    }

    private void setUpArray(int uPS, int nF)
    {
        map = new Tile[uPS, uPS, nF];

        for(int y = 0; y < nF; y++)
        {
            for(int z = 0; z < uPS; z++)
            {
                for(int x = 0; x < uPS; x++)
                {
                    map[x, z, y] = null;
                }
            }
        }
    }

    private void createExitTile(ref int x, ref int y, ref int z, int uPS, int nF)
    {
        //determine where to place exit
        int onX = Random.Range(0, 2);
        if (onX == 1) //exit is on the x
        {
            x = Random.Range(0, uPS);
            int side = Random.Range(0, 2);

            if (side == 0)
            {
                z = 0;
            }
            else
            {
                z = uPS - 1;
            }
        }
        else //exit is on the z
        {
            z = Random.Range(0, uPS);
            int side = Random.Range(0, 2);

            if (side == 0)
            {
                x = 0;
            }
            else
            {
                x = uPS - 1;
            }
        }

        y = Random.Range(0, nF);

        //create the tile
        createTile(x, y, z, uPS, nF);

        //determine where the next tile should be placed
        int rand;
        int[] directions = { 1, 2, 3, 4 };

        for(int k = 0; k < directions.Length; k++)
        {
            rand = Random.Range(k, directions.Length);

            if(validTile(directions[rand], ref x, ref y, ref z, uPS, nF))
            {
                //remove wall
                tileList.Peek().removeWall(directions[rand]);

                //place the exit sign
                GameObject sign = Instantiate(exitSign) as GameObject;
                sign.name = "Exit Sign";
                sign.transform.parent = tileList.Peek().transform;
                sign.transform.localEulerAngles = new Vector3(0, 90 * (directions[rand] - 1), 0);
                sign.transform.localPosition = new Vector3(0, 2.745f, 0);
                break;
            }

            directions[rand] = directions[k];
        }

    }

    private void createTile(int x, int y, int z, int uPS, int nF)
    {
        Tile newTile = Instantiate(tilePrefab) as Tile;
        map[x, z, y] = newTile;
        newTile.name = "Tile " + x + ", " + z + ", " + y;
        newTile.transform.parent = transform;
        newTile.transform.localPosition = new Vector3(x - uPS * 0.5f, y * 3, z - uPS * 0.5f);

        newTile.storeIndecies(x, y, z);

        tileList.Push(newTile);
    }

    private bool validTile(int dir, ref int x, ref int y, ref int z, int uPS, int nF)
    {
        int tempx = x;
        int tempz = z;
        int tempy = y;

        switch (dir)
        {
            case 0:
                tempy -= 1;
                if (tempy > -1)
                {
                    if (map[tempx, tempz, tempy] == null)
                    {
                        y = tempy;
                        return true;
                    }
                }
                break;
            case 1:
                tempz += 1;
                if (tempz < uPS)
                {
                    if (map[tempx, tempz, tempy] == null)
                    {
                        z = tempz;
                        return true;
                    }
                }
                break;
            case 2:
                tempx += 1;
                if (tempx < uPS)
                {
                    if (map[tempx, tempz, tempy] == null)
                    {
                        x = tempx;
                        return true;
                    }
                }
                break;
            case 3:
                tempz -= 1;
                if (tempz > -1)
                {
                    if (map[tempx, tempz, tempy] == null)
                    {
                        z = tempz;
                        return true;
                    }
                }
                break;
            case 4:
                tempx -= 1;
                if (tempx > -1)
                {
                    if (map[tempx, tempz, tempy] == null)
                    {
                        x = tempx;
                        return true;
                    }
                }
                break;
            case 5:
                tempy += 1;
                if (tempy < nF)
                {
                    if (map[tempx, tempz, tempy] == null)
                    {
                        y = tempy;
                        return true;
                    }
                }
                break;
        }
        return false;
    }

    private void createNextTile(ref int x, ref int y, ref int z, int uPS, int nF)
    {
        //determine missing wall from prev tile
        int prevX = 0;
        int prevY = 0;
        int prevZ = 0;

        tileList.Peek().getIndecies(ref prevX, ref prevY, ref prevZ);

        //create the tile
        createTile(x, y, z, uPS, nF);

        createCeilingLight();

        //remove wall blocking previous tile
        if (prevX != x)
        {
            //determine if the east or west wall
            if (prevX > x)
            {
                //prev west so remove this east
                tileList.Peek().removeWall(2);
            }
            else
            {
                //prev east so remove this west
                tileList.Peek().removeWall(4);
            }
        }
        else if (prevZ != z)
        {
            //determine if north or south wall
            if (prevZ > z)
            {
                //prev south so remove  this north
                tileList.Peek().removeWall(1);
            }
            else
            {
                //prev north so remove this south
                tileList.Peek().removeWall(3);
            }
        }

        if(prevY != y)
        {
            //determine if floor or cieling
            if (prevY > y)
            {
                //prev up so remove this up
                tileList.Peek().removeWall(5);
            }
            else if(prevY < y)
            {
                //prev down so remove this down
                tileList.Peek().removeWall(0);
            }
        }

        //determine where the next tile should be placed
        int rand;
        int[] directions = { 1, 2, 3, 4 };

        for (int k = 0; k < directions.Length; k++)
        {
            rand = Random.Range(k, directions.Length);

            if (validTile(directions[rand], ref x, ref y, ref z, uPS, nF))
            {
                //remove wall
                tileList.Peek().removeWall(directions[rand]);
                break;
            }

            directions[rand] = directions[k];
        }

    }

    private void backTrack(ref int x, ref int y, ref int z, int uPS, int nF)
    {
        do
        {
            if (tileList.Count > 1)
            {
                //pop a tile from the stack
                tileList.Pop();

                //change x, y, z to be current tile
                tileList.Peek().getIndecies(ref x, ref y, ref z);

                //check for other valid tiles around current tile
                int rand;
                int[] directions = { 1, 2, 3, 4 };

                for (int k = 0; k < directions.Length; k++)
                {
                    rand = Random.Range(k, directions.Length);

                    if (validTile(directions[rand], ref x, ref y, ref z, uPS, nF))
                    {
                        //remove wall
                        tileList.Peek().removeWall(directions[rand]);
                        break;
                    }

                    directions[rand] = directions[k];
                }
            }
            else
            {
                //tileList.count should be 1, which means it only has the exit stored.
                //which in turn means that it has backtracked to this point because
                //there are no more valid tiles. So pop it and end the maze creation.
                tileList.Pop();
                break;
            }

            //if no valid tiles, repeat
        } while (map[x, z, y] == tileList.Peek());
    }

    private void createCeilingLight()
    {
        GameObject newLight = Instantiate(ceilingLight) as GameObject;
        newLight.name = "Ceiling Light";
        newLight.transform.parent = tileList.Peek().transform;
        newLight.transform.localPosition = new Vector3(0, 2.67f, 0);
    }

    private void removeCeilingLight()
    {
        Transform[] all = tileList.Peek().GetComponentsInChildren<Transform>();
        for(int i = 0; i < all.Length; i++)
        {
            if(all[i].name == "Ceiling Light")
            {
                Destroy(all[i].gameObject);
            }
        }
    }

    private void placePlayer(int uPS, int nF)
    {
        int playerX, playerY, playerZ;

        playerX = Mathf.RoundToInt(uPS * 0.5f);
        playerZ = Mathf.RoundToInt(uPS * 0.5f);
        playerY = Mathf.RoundToInt(nF * 0.5f);

        Player player = Instantiate(playerPrefab) as Player;
        player.name = "Player";
        player.transform.parent = transform;
        player.transform.localPosition = new Vector3(playerX - uPS * 0.5f, playerY * 3 + 1.1f, playerZ - uPS * 0.5f);
    }
}
