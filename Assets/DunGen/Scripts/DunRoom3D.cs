using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sol.Dungeon //this script will control and manage the visibility of walls and the creation of a continuous path through the maze.
{ 

public class Room3D : MonoBehaviour
{
    public enum Directions
    {
        NORTH,
        SOUTH,
        EAST,
        WEST,
        NONE,
    }

    [SerializeField] GameObject NorthWall;
    [SerializeField] GameObject SouthWall;
    [SerializeField] GameObject EastWall;
    [SerializeField] GameObject WestWall;

    Dictionary<Directions, GameObject> walls =
      new Dictionary<Directions, GameObject>();

      public Vector3Int Index { get; set; }

    public bool visited { get; set; } = false;

    Dictionary<Directions, bool> dirFlags =
      new Dictionary<Directions, bool>();
    
    private void Start()
    {
        walls[Directions.NORTH] = NorthWall;
        walls[Directions.SOUTH] = SouthWall;
        walls[Directions.EAST] = EastWall;
        walls[Directions.WEST] = WestWall;
    }

    private void SetActive(Directions dir, bool flag)
    {
        walls[dir].SetActive(flag);
    }

    public void SetDirFlag(Directions dir, bool flag)
    {
        dirFlags[dir] = flag;
        SetActive(dir, flag);
    }
}
}
