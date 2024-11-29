using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public List<GameObject> snappingPoints;
    public Vector2Int gridSlot;
    public int weight = 10;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void RemoveSnappingPoint(GameObject snappingPoint)
    {
        snappingPoints.Remove(snappingPoint);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
