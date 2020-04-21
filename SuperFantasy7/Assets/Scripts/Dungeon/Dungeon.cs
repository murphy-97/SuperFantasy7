using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum Compass {
	North = 0,
	East,
	South,
    West
}

public class Dungeon : MonoBehaviour
{
    // Class (static) properties
    private static bool use_seed = false;
    private static int seed = 0;

    // Class (static) methods
    public static void Set_Seed(int s) {
        seed = s;
        use_seed = true;
    }

    public static void Reset_Seed() {
        // Mechanics of reset handled in dungeon generation
        use_seed = false;
    }

    // Unity methods
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
    [SerializeField] private Room[] si_prefabs;
    [SerializeField] private GameObject building_screen;

    [SerializeField][Range(0,1)] private float[] add_neighbor_chance;

    private Room entry_room;
    private Dictionary<Tuple<int, int>, Room> room_dict;

    // Object methods
    public void GenerateDungeon()
    {
        // Handle random seed
        int local_seed = seed;
        if (!use_seed) {
            // Detach new random seed from last used seed
            UnityEngine.Random.InitState(System.Environment.TickCount);

            // Get new local seed
            local_seed = BitConverter.ToInt32(
                BitConverter.GetBytes(UnityEngine.Random.value),
                0
            );
        }
        Debug.Log("Building from seed " + local_seed);
        UnityEngine.Random.InitState(local_seed);

        // Initiailze dungeon
        building_screen.gameObject.SetActive(true);
        entry_room = Room.GetFirstRoom(this, entry_prefab);
        entry_room.transform.SetParent(transform);

        room_dict = new Dictionary<Tuple<int, int>, Room>();
        room_dict.Add(entry_room.GetPositionTuple(), entry_room);

        // Create first neighbor
        Room next = entry_room.MakeNeighbor(
            prefabs[UnityEngine.Random.Range(0, prefabs.Length)],
            Compass.West
        );
        next.transform.SetParent(transform);
        room_dict.Add(next.GetPositionTuple(), next);

        room_count -= 2;    // Account for entry and first neighbor

        // Iteratively generate rooms
        Stack<Room> si_stack = new Stack<Room>(); // Used when placing special item rooms
        Stack<Room> build_stack = new Stack<Room>();
        build_stack.Push(next);
        si_stack.Push(next);
        Stack<Room> save_stack = new Stack<Room>();

        while (room_count > 0) {

            while (build_stack.Count > 0 && room_count > 0) {
                next = build_stack.Pop();
                save_stack.Push(next);

                // Build shuffled list to assign directions in random order
                List<Compass> dirs = new List<Compass> {
                    Compass.North, Compass.South, Compass.East, Compass.West
                };

                for (int i = 0; i < dirs.Count; i++) {
                    Compass temp = dirs[i];
                    int randomIndex = UnityEngine.Random.Range(i, dirs.Count);
                    dirs[i] = dirs[randomIndex];
                    dirs[randomIndex] = temp;
                }

                // Generate rooms in order
                for (int i = 0; i < dirs.Count; i++) {
                    if (!next.HasNeighbor(dirs[i])) {

                        float neighbor_chance = 0.0f;
                        try {
                            neighbor_chance = add_neighbor_chance[next.NeighborCount()];
                        } catch {
                            /* Out of bounds. Leave chance at 0. */
                        }

                        // Does not have a neighbor in this direction
                        if (room_dict.ContainsKey(next.GetNeighborTuple(dirs[i]))) {
                            // Room exists in this direction. Check random and connect
                            if (UnityEngine.Random.value < neighbor_chance && room_dict[next.GetNeighborTuple(dirs[i])] != entry_room) {
                                // Connect rooms
                                next.AddNeighbor(
                                    room_dict[next.GetNeighborTuple(dirs[i])],
                                    dirs[i]
                                );
                            }
                        } else if (UnityEngine.Random.value < neighbor_chance) {
                            // Room does not exist. Making new room
                            Room new_room = next.MakeNeighbor(
                                prefabs[UnityEngine.Random.Range(0, prefabs.Length)],
                                dirs[i]
                            );
                            new_room.transform.SetParent(transform);
                            room_count -= 1;
                            room_dict.Add(new_room.GetPositionTuple(), new_room);
                            build_stack.Push(new_room);
                            si_stack.Push(new_room);
                        }
                    }
                }
            }

            while (save_stack.Count > 0) {
                // Rebuild build stack to finish building dungeon
                build_stack.Push(save_stack.Pop());
            }
        }

        // Place special item rooms next to rooms with fewest rooms
        // Create priority stack for rooms to neighbor special item rooms
        float counter = 0.0f;
        float counter_max = (float)si_stack.Count;
        SortedList<float, Room> priority = new SortedList<float, Room>();
        while (si_stack.Count > 0) {
            next = si_stack.Pop();
            if (next == entry_room) {
                continue;
            }

            float key = 0.0f;

            List<Compass> dirs = new List<Compass> {
                Compass.North, Compass.South, Compass.East, Compass.West
            };

            for (int i = 0; i < dirs.Count; i++) {
                if (room_dict.ContainsKey(next.GetNeighborTuple(dirs[i]))) {
                    key += 1.0f;
                }
            }

            key += counter / counter_max;   // Enforce unique keys for each room
            counter += 1.0f;

            if (key < 4.0f) {
                priority.Add(key, next);
            }
        }

        Debug.Assert(si_prefabs.Length <= priority.Count);
        int priority_offset = 0;
        for (int i = 0; i < si_prefabs.Length; i++) {
            next = priority[priority.Keys[i+priority_offset]];

            // Add special item room as neighbor at random available direction
            List<Compass> dirs = new List<Compass> {
                Compass.North, Compass.South, Compass.East, Compass.West
            };

            for (int j = 0; j < dirs.Count; j++) {
                Compass temp = dirs[j];
                int randomIndex = UnityEngine.Random.Range(j, dirs.Count);
                dirs[j] = dirs[randomIndex];
                dirs[randomIndex] = temp;
            }

            bool assigned = false;
            for (int j = 0; j < dirs.Count; j++) {
                if (!room_dict.ContainsKey(next.GetNeighborTuple(dirs[j]))) {
                    // Found suitable location
                    Room si_room = next.MakeNeighbor(si_prefabs[i], dirs[j]);
                    assigned = true;
                    room_dict.Add(si_room.GetPositionTuple(), si_room);
                    break;
                }
            }

            if (!assigned) {
                priority_offset += 1;
            }
        }

        // Finalize creation of dungeon
        building_screen.gameObject.SetActive(false);
    }

    public Vector2 GetRoomSpacing()
    {
        return room_spacing;
    }
}
