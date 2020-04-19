using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// NOTE: public enum Compass declared in Dungeon.cs

public class Room : MonoBehaviour
{
    // Class (static) properties

    // Class (static) methods
    public static Room GetFirstRoom(Dungeon d, Room prefab)
    {
        Room room = Instantiate(prefab);
        room.dungeon = d;
        room.position = Vector2.zero;
        room.gameObject.transform.position = new Vector3(
            0.0f,
            0.0f,
            room.transform.position.z
        );
        return room;
    }

    // Unity methods
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Object properties
    [Header("Door Objects")]
    [SerializeField] private GameObject door_n;
    [SerializeField] private GameObject door_s;
    [SerializeField] private GameObject door_e;
    [SerializeField] private GameObject door_w;

    [Header("Dungeon Parameters")]
    [SerializeField] private Dungeon dungeon;

    // Dungeon parameters
    private Room neighbor_n;
    private Room neighbor_s;
    private Room neighbor_e;
    private Room neighbor_w;

    private Vector2 position;

    // Object methods
    public Vector2 GetPosition()
    {
        return position;
    }

    public Vector2 AddNeighbor(Room neighbor, Compass direction)
    {
        Vector2 offset_mul = Vector2.zero;

        switch (direction) {
            case Compass.North:
                door_n.gameObject.SetActive(false);
                neighbor.door_s.gameObject.SetActive(false);

                neighbor_n = neighbor;
                neighbor.neighbor_s = this;

                offset_mul.x = 0.0f;
                offset_mul.y = 1.0f;

                neighbor.position = new Vector2(
                    position.x,
                    position.y + 1
                );

                break;

            case Compass.South:
                door_s.gameObject.SetActive(false);
                neighbor.door_n.gameObject.SetActive(false);

                neighbor_s = neighbor;
                neighbor.neighbor_n = this;

                offset_mul.x = 0.0f;
                offset_mul.y = -1.0f;

                neighbor.position = new Vector2(
                    position.x,
                    position.y - 1
                );

                break;

            case Compass.East:
                door_e.gameObject.SetActive(false);
                neighbor.door_w.gameObject.SetActive(false);

                neighbor_e = neighbor;
                neighbor.neighbor_w = this;

                offset_mul.x = 1.0f;
                offset_mul.y = 0.0f;

                neighbor.position = new Vector2(
                    position.x + 1,
                    position.y
                );

                break;

            case Compass.West:
                door_w.gameObject.SetActive(false);
                neighbor.door_e.gameObject.SetActive(false);

                neighbor_w = neighbor;
                neighbor.neighbor_e = this;

                offset_mul.x = -1.0f;
                offset_mul.y = 0.0f;

                neighbor.position = new Vector2(
                    position.x - 1,
                    position.y
                );

                break;
        }

        // Return offset_mul for use in MakeNeighbor()
        return offset_mul;
    }

    public Room MakeNeighbor(Room prefab, Compass direction)
    {
        // Check that we are not overwriting an existing neighbor
        switch (direction) {
            case Compass.North:
                Debug.Assert(neighbor_n == null);
                break;

            case Compass.South:
                Debug.Assert(neighbor_s == null);
                break;

            case Compass.East:
                Debug.Assert(neighbor_e == null);
                break;

            case Compass.West:
                Debug.Assert(neighbor_w == null);
                break;
        }

        Room neighbor = Instantiate(prefab);
        neighbor.dungeon = dungeon;

        Vector2 offset_mul = AddNeighbor(neighbor, direction);

        neighbor.transform.position = new Vector3(
            gameObject.transform.position.x + (dungeon.GetRoomSpacing().x * offset_mul.x),
            gameObject.transform.position.y + (dungeon.GetRoomSpacing().y * offset_mul.y),
            gameObject.transform.position.z
        );

        return neighbor;
    }

    public bool HasNeighbor(Compass direction)
    {
        switch (direction) {
            case Compass.North:
                return (neighbor_n != null);

            case Compass.South:
                return (neighbor_s != null);

            case Compass.East:
                return (neighbor_e != null);

            case Compass.West:
                return (neighbor_w != null);
        }

        return false;
    }
}
