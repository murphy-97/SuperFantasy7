using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Compass {
	North = 0,
	East,
	South,
    West
}

public class Dungeon : MonoBehaviour
{
    // Class (static) properties

    // Class (static) methods

    void Awake()
    {
        GenerateDungeon();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Object properties
    [Header("Generation Data")]
    [SerializeField] private int room_count;
    [SerializeField] private Vector2 room_spacing;
    [SerializeField] private Room entry_prefab;
    [SerializeField] private Room[] prefabs;
    [SerializeField] private GameObject building_screen;

    private Room entry_room;

    // Object methods
    public void GenerateDungeon()
    {
        // Initiailze dungeon
        building_screen.gameObject.SetActive(true);
        entry_room = Room.GetFirstRoom(this, entry_prefab);

        // Create first neighbor
        Room next = entry_room.MakeNeighbor(
            prefabs[Random.Range(0, prefabs.Length)],
            Compass.West
        );

        // Iteratively generate rooms

        // Finalize creation of dungeon
        building_screen.gameObject.SetActive(false);
    }

    public Vector2 GetRoomSpacing()
    {
        return room_spacing;
    }
}
