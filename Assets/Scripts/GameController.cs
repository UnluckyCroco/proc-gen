using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameObject startingTile;
    // private Room startingRoom;
    public List<GameObject> tilePrefabs;
    public List<GameObject> placedTiles = new();
    private readonly List<GameObject> availableTiles = new();
    private readonly List<Vector2Int> gridSlots = new();
    private Vector3 startingPosition;
    private readonly List<int> roomWeights = new();

    // Start is called before the first frame update
    void Start()
    {
        placedTiles.Add(startingTile);
        availableTiles.Add(startingTile);

        gridSlots.Add(new Vector2Int(0, 0));
        startingPosition = startingTile.transform.position;

        int count = 0;
        foreach (Room room in tilePrefabs.Select(tile => tile.GetComponent<Room>()))
        {
            for (int i = 0; i < room.weight; i++)
            {
                roomWeights.Add(count);
            }
            count++;
        }

        for (var i = 0; i < 2000; i++)
        {
            bool hasPlacedRoom = PlaceRoom();
            if (!hasPlacedRoom) i--;
        }


        // place walls on unused snaps
        //PlaceWalls();
    }

    private void PlaceWalls()
    {
        foreach (var tile in placedTiles)
        {
            var tileRoom = tile.GetComponent<Room>();
            foreach (var snap in tileRoom.snappingPoints)
            {
                var snapPoint = snap.GetComponent<SnappingPoint>();
                Instantiate(snapPoint.wallPrefab, snapPoint.wallSnappingPoint.transform);
            }
        }
    }

    private bool PlaceRoom()
    {
        GameObject randomTile = null;
        GameObject randomSnap = null;
        Room randomTileRoom = null;

        while (!randomSnap)
        {
            // what if no tiles available?
            randomTile = availableTiles[Random.Range(0, availableTiles.Count)];
            randomTileRoom = randomTile.GetComponent<Room>();
            List<GameObject> snapPoints = randomTileRoom.snappingPoints;

            randomSnap = snapPoints[Random.Range(0, snapPoints.Count)];
        }

        SnappingPoint randomSnapPoint = randomSnap.GetComponent<SnappingPoint>();
        Bounds doorSize = GetMaxBounds(randomSnapPoint.doorPrefab);
        Direction direction = DirectionEnum.ReverseDirection(randomSnapPoint.direction);

        // new room (we have to add doorsize to the transform because our empty snapping point is moved to the middle of the door object; it's a lil scuffed but it works? also I do *2 because it works, not because I know what it does.
        Vector3 newTilePosition = randomSnap.transform.TransformPoint(Vector3.forward * (5 + doorSize.extents.z * 2));
        Vector3 gridSlotV3 = (newTilePosition - startingPosition) / 10;
        Vector2Int gridSlot = new(Mathf.RoundToInt(gridSlotV3.x), Mathf.RoundToInt(gridSlotV3.z));

        GameObject newTile = null;
        Room newTileRoom = null;
        bool hasPlacedRoom = false;

        // check whether gridSlot is occupied, if not, place tile
        if (gridSlots.Contains(gridSlot))
        {
            newTile = placedTiles.Find(tile => tile.GetComponent<Room>().gridSlot == gridSlot);
            newTileRoom = newTile.GetComponent<Room>();

            // connecting tile has no valid snapping point, so place a wall
            if (! hasValidSnapInDirection(newTileRoom, direction))
            {
                //Debug.Log("Placed wall");
                Instantiate(randomSnapPoint.wallPrefab, randomSnapPoint.wallSnappingPoint.transform);

                randomTileRoom.RemoveSnappingPoint(randomSnap);
                RemovePlacedTilesWithoutSnaps(randomTile);
                return false;
            }
        }
        else
        {
            // weighted rooms
            GameObject randomTilePrefab = tilePrefabs[roomWeights[Random.Range(0, roomWeights.Count)]];

            // keep rolling tiles until you find a tile that can fit
            while (! hasValidSnapInDirection(randomTilePrefab.GetComponent<Room>(), direction))
            {
                randomTilePrefab = tilePrefabs[roomWeights[Random.Range(0, roomWeights.Count)]];
            }

            newTile = Instantiate(randomTilePrefab, newTilePosition, Quaternion.identity);
            placedTiles.Add(newTile);
            availableTiles.Add(newTile);
            newTileRoom = newTile.GetComponent<Room>();
            newTileRoom.gridSlot = gridSlot;
            gridSlots.Add(gridSlot);
            hasPlacedRoom = true;
        }


        // door for original room
        Instantiate(randomSnapPoint.doorPrefab, randomSnapPoint.doorSnappingPoint.transform);

        // for finding which side the new tile should place a door to connect to the original tile we just came from
        List<SnappingPoint> newTileSnaps = newTileRoom.snappingPoints.Select(snappingPoint => snappingPoint.GetComponent<SnappingPoint>()).ToList();

        // can have multiple finds, is not handled rn -> think multiple entrances from the south when a room is bigger -> also what about rotation and randomness? all rooms currently assume no rotation
        SnappingPoint newTileSnapPoint = newTileSnaps.Find((x) => x.direction == direction);
        Instantiate(newTileSnapPoint.doorPrefab, newTileSnapPoint.doorSnappingPoint.transform);

        randomTileRoom.RemoveSnappingPoint(randomSnap);
        newTileRoom.RemoveSnappingPoint(newTileSnapPoint.gameObject);
        RemovePlacedTilesWithoutSnaps(newTile);
        RemovePlacedTilesWithoutSnaps(randomTile);

        return hasPlacedRoom;
    }

    private void RemovePlacedTilesWithoutSnaps(GameObject tile)
    {
        if (tile.GetComponent<Room>().snappingPoints.Count == 0)
        {
            var d = availableTiles.Find(tile2 => tile2.GetComponent<Room>().gridSlot == tile.GetComponent<Room>().gridSlot);
            availableTiles.Remove(tile);
            var slots = availableTiles.Select(tile => tile.GetComponent<Room>().gridSlot).Aggregate("All Slots: ", (current, next) => current += $"({next.x}, {next.y}) ");

        }
    }

    private bool hasValidSnapInDirection(Room room, Direction direction)
    {
        return room.snappingPoints.Select(snap => snap.GetComponent<SnappingPoint>().direction).ToList().Contains(direction);
    }

    public static Bounds GetMaxBounds(GameObject g)
    {
        var b = new Bounds(g.transform.position, Vector3.zero);
        foreach (Renderer r in g.GetComponentsInChildren<Renderer>())
        {
            b.Encapsulate(r.bounds);
        }
        return b;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            PlaceRoom();
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            PlaceWalls();
        }
    }
}
