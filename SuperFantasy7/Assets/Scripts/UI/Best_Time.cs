using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Best_Time : MonoBehaviour
{

    // Object properties
    [SerializeField] private InputField seed;
    [SerializeField] private Text clock;

    // Object methods
    public void Display_Time(int s, float t) {
        seed.text = "" + s;
        clock.text = Player_Char.Format_Time(t, 2);
    }

    public void Play(string scene) {
        // Set seed
        Dungeon.Set_Seed(int.Parse(seed.text));
        // Load scene
		SceneManager.LoadScene(scene, LoadSceneMode.Single);
    }
}
