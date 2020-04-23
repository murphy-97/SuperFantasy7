using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum Menu_Type {
    Main = 0,
    Play,
    Time
}

public class Main_Menu : MonoBehaviour
{
    // Class (static) properties
    private static SortedList<float, int> best_times;
    private static int best_time_count = 10;

    // Class (static) methods
    private static void Initialize_Best_Times(Best_Time[] time_list)
    {
        best_times = new SortedList<float, int>();
        best_time_count = time_list.Length;

        for (int i = 0; i < time_list.Length; i++) {

            if (PlayerPrefs.GetInt("scoreSet_" + i, 0) != 0) {
                best_times.Add(
                    PlayerPrefs.GetFloat("scoreTime_" + i, -1.0f),
                    PlayerPrefs.GetInt("scoreSeed_" + i, -1)
                );

            } else {
                break;
            }
        }
    }

    public static bool Consider_Best_Time(float t, int s)
    {
        bool is_new_best = false;

        if (best_times.Keys.Count < best_time_count) {
            is_new_best = true;
            best_times.Add(t, s);

            // Save best times in PlayerPrefs
            for (int j = 0; j < best_times.Keys.Count; j++) {
                PlayerPrefs.SetInt("scoreSet_" + j, 1);
                PlayerPrefs.SetFloat("scoreTime_" + j, t);
                PlayerPrefs.SetInt("scoreSeed_" + j, s);
            }
            return is_new_best;
        }

        for (int i = 0; i < best_times.Keys.Count; i++) {
            if (t < best_times[best_times.Keys[i]]) {
                is_new_best = true;
                best_times.Add(t, s);

                // Save best times in PlayerPrefs
                for (int j = 0; j < best_times.Keys.Count; j++) {
                    PlayerPrefs.SetInt("scoreSet_" + j, 1);
                    PlayerPrefs.SetFloat("scoreTime_" + j, t);
                    PlayerPrefs.SetInt("scoreSeed_" + j, s);
                }
                break;
            }
        }

        return is_new_best;
    }

    void Awake()
    {
        next_target = menu_pos_main;
        next_menu = menu_main;
    }

    // Start is called before the first frame update
    void Start()
    {
        Initialize_Best_Times(time_list);
        Display_Best_Times();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 cam_pos = camera_obj.transform.position;
        Vector3 cam_pos_new = cam_pos;

        bool lock_x = false;
        bool lock_y = false;

        if (Mathf.Abs(cam_pos.x - next_target.x) > camera_precision) {
            float heading = (cam_pos.x - next_target.x > 0.0f ? -1.0f : 1.0f);
            cam_pos_new.x += Time.deltaTime * camera_speed * heading;
        } else {
            // Lock onto position
            cam_pos_new.x = next_target.x;
            lock_x = true;
        }

        if (Mathf.Abs(cam_pos.y - next_target.y) > camera_precision) {
            float heading = (cam_pos.y - next_target.y > 0.0f ? -1.0f : 1.0f);
            cam_pos_new.y += Time.deltaTime * camera_speed * heading;
        } else {
            // Lock onto position
            cam_pos_new.y = next_target.y;
            lock_y = true;
        }

        // Apply camera movements
        camera_obj.transform.position = cam_pos_new;
        if (lock_x && lock_y) {
            next_menu.SetActive(true);
        }
    }

    // Object properties
    [SerializeField] private GameObject camera_obj;
    [SerializeField] private float camera_speed;
    [SerializeField] private float camera_precision;

    [SerializeField] private GameObject menu_main;
    [SerializeField] private GameObject menu_play;
    [SerializeField] private GameObject menu_times;

    [SerializeField] private Vector2 menu_pos_main;
    [SerializeField] private Vector2 menu_pos_play;
    [SerializeField] private Vector2 menu_pos_times;

    [SerializeField] private Best_Time[] time_list;

    [SerializeField] private InputField play_seed;
    [SerializeField] private string dungeon_scene;

    private Vector2 next_target;
    private GameObject next_menu;

    // Object methods
    public void Move_To_Menu(int menu) {

        next_menu.SetActive(false);

        switch ((Menu_Type)menu) {

            case Menu_Type.Main:
                next_target = menu_pos_main;
                next_menu = menu_main;
                break;
            
            case Menu_Type.Play:
                next_target = menu_pos_play;
                next_menu = menu_play;
                break;
            
            case Menu_Type.Time:
                next_target = menu_pos_times;
                next_menu = menu_times;
                break;
        }
    }

    public void Display_Best_Times() {
        for (int i = 0; i < time_list.Length; i++) {

            if (i < best_times.Keys.Count) {

                time_list[i].Display_Time(
                    best_times[best_times.Keys[i]],
                    best_times.Keys[i]
                );
                time_list[i].gameObject.SetActive(true);

            } else {
                time_list[i].gameObject.SetActive(false);
            }
        }
    }

    public void Play(bool use_seed) {
        // Set seed
        if (use_seed) {
            Dungeon.Set_Seed(int.Parse(play_seed.text));
        } else {
            Dungeon.Reset_Seed();
        }
        
        // Load scene
		SceneManager.LoadScene(dungeon_scene, LoadSceneMode.Single);
    }

    public void Quit_Game() {
        Application.Quit();
    }
}
