using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sol.Dungeon
{
    public class DunGen3D : MonoBehaviour
    {
        [SerializeField] GameObject roomPrefab;

        //dungeon grid
        Room3D[,] rooms = null;

        [SerializeField] int numX = 10;
        [SerializeField] int numZ = 10;

        //room W (x, i) and H (z, j)
        float roomWidth;
        float roomLength;

        //backtracking stack
        Stack<Room3D> stack = new Stack<Room3D>();

    bool generating = false;

    private void GetRoomSize()
    {
        SpriteRenderer[] spriteRenderers = 
            roomPrefab.GetComponentsInChildren<SpriteRenderer>();

        Vector3 minBounds = Vector3.positiveInfinity;
        Vector3 maxBounds = Vector3.negativeInfinity;   

        foreach(SpriteRenderer ren in spriteRenderers)
        {
            minBounds = Vector3.Min(minBounds, ren.bounds.min);
            maxBounds = Vector3.Max(maxBounds, ren.bounds.max);
        }

        roomWidth = maxBounds.x - minBounds.x;
        roomLength = maxBounds.z - minBounds.z;
    }

    private void Start()
    {
        GetRoomSize();

        rooms = new Room3D[numX, numZ];

        for(int i = 0; i < numX; ++i)
        {
            for(int j = 0; j < numZ; ++j)
            {
                GameObject room = Instantiate(roomPrefab,
                   new Vector3(i * roomWidth, 0f, j * roomLength),
                   Quaternion.identity);

                room.name = "Room_" + i.ToString() + "_" + j.ToString();
                rooms[i, j] = room.GetComponent<Room3D>();
                rooms[i, j].Index = new Vector3Int(i, 0, j);
            }
        }

    }

    private void RemoveRoomWall (int x, int z, Room3D.Directions dir)
    {
        if (dir != Room3D.Directions.NONE)
        {
            rooms[x, z].SetDirFlag(dir, false);
        }

        Room3D.Directions opp = Room3D.Directions.NONE;
        switch (dir)
        {
            case Room3D.Directions.NORTH:
                if(z < numZ -1)
                    {
                      opp = Room3D.Directions.SOUTH;
                      ++z;  
                    }
                break;
            case Room3D.Directions.EAST:
                if(x < numX -1)
                    {
                      opp = Room3D.Directions.WEST;
                      ++x;  
                    }
                break;
            case Room3D.Directions.SOUTH:
                if(z > 0)                    {
                      opp = Room3D.Directions.NORTH;
                      --z;  
                    }
                break;
            case Room3D.Directions.WEST:
                if(x > 0)
                    {
                      opp = Room3D.Directions.EAST;
                      --x;  
                    }
                break;
        }
        if (opp != Room3D.Directions.NONE)
        {
        rooms[x, z].SetDirFlag(opp, false);
        }
    }

    public List<Tuple<Room3D.Directions, Room3D>> GetUnvisitedNeighbors(int cx, int cz)
    {
        List<Tuple<Room3D.Directions, Room3D>> neighbours =
            new List<Tuple<Room3D.Directions, Room3D>>();

        foreach(Room3D.Directions dir in Enum.GetValues(typeof(Room3D.Directions)))
        {
            int x = cx;
            int z = cz;

            switch(dir)
            {
                case Room3D.Directions.NORTH:
                    if(z < numZ - 1)
                    {
                        ++z;
                        if (!rooms[x, z].visited)
                        {
                            neighbours.Add(new Tuple<Room3D.Directions, Room3D>(
                                Room3D.Directions.NORTH,
                                rooms[x, z]));
                        }
                    }
                break;

                case Room3D.Directions.EAST:
                    if(x < numX - 1)
                    {
                        ++x;
                        if (!rooms[x, z].visited)
                        {
                            neighbours.Add(new Tuple<Room3D.Directions, Room3D>(
                                Room3D.Directions.EAST,
                                rooms[x, z]));
                        }
                    }
                break;

                case Room3D.Directions.SOUTH:
                    if(z > 0)
                    {
                        --z;
                        if (!rooms[x, z].visited)
                        {
                            neighbours.Add(new Tuple<Room3D.Directions, Room3D>(
                                Room3D.Directions.SOUTH,
                                rooms[x, z]));
                        }
                    }
                break;

                case Room3D.Directions.WEST:
                    if(x > 0)
                    {
                        --x;
                        if (!rooms[x, z].visited)
                        {
                            neighbours.Add(new Tuple<Room3D.Directions, Room3D>(
                                Room3D.Directions.WEST,
                                rooms[x, z]));
                        }
                    }
                break;
            }
        }
        return neighbours;
    }

    private bool GenerateStep()
    {
        if(stack.Count == 0) return true;

        Room3D r = stack.Peek();
        var neighbours = GetUnvisitedNeighbors(r.Index.x, r.Index.z);

        if(neighbours.Count != 0)
        {
            var index = 0;
            if(neighbours.Count > 1)
            {
                index = UnityEngine.Random.Range(0, neighbours.Count);
            }

            var item = neighbours[index];
            Room3D neighbour = item.Item2;
            neighbour.visited = true;
            RemoveRoomWall(r.Index.x, r.Index.z, item.Item1);

            stack.Push(neighbour);
        }
        else
        {
            stack.Pop();
        }
        return false;
    }

    public void CreateDungeon()
        {
            if (generating) 
            {
                Debug.Log("Already generating dungeon. Please wait.");
                return;
            }

            Reset();
            RemoveRoomWall(0, 0, Room3D.Directions.SOUTH);

            RemoveRoomWall(numX - 1, numZ - 1, Room3D.Directions.EAST);

            stack.Push(rooms[0, 0]);

            StartCoroutine(Coroutine_DunGen());
        }

    IEnumerator Coroutine_DunGen()
{
    generating = true;
    bool flag = false;
    while(!flag)
    {
        for(int i = 0; i < 10; i++) // Process 10 steps per frame
        {
            flag = GenerateStep();
            if(flag) break;
        }
        yield return null; // Wait one frame
    }

    generating = false;
}

    private void Reset()
    {
        for(int i = 0; i < numX; ++i)
        {
            for(int j = 0; j < numZ; ++j)
            {
                rooms[i, j].visited = false;
                rooms[i, j].SetDirFlag(Room3D.Directions.NORTH, true);
                rooms[i, j].SetDirFlag(Room3D.Directions.SOUTH, true);
                rooms[i, j].SetDirFlag(Room3D.Directions.EAST, true);
                rooms[i, j].SetDirFlag(Room3D.Directions.WEST, true);
                rooms[i, j].visited = false;
            }
        }
    }

    private void Update()
    {
        // Modern Input System approach
        if(UnityEngine.InputSystem.Keyboard.current.rKey.wasPressedThisFrame)
        {
            if(!generating)
            {
                CreateDungeon();
            }
        }

    }

}
}

    

