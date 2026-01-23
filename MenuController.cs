using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/**
 * Author: Rebekah Bledsoe
 * Version: 01/22/2026
 * Summary: The function of this script is to control the systems for
 *          the main menu and its cinematic custcenes/transitions.
 **/
public class MenuController : MonoBehaviour {
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject startPoint;
    [SerializeField] private GameObject endPoint;
    [SerializeField] private GameObject winMenu;
    [SerializeField] private GameObject boat;
    [SerializeField] private bool winScene = false;
    [SerializeField] private bool levelOne = false;

    private bool moving = false;
    private bool atEnd = false;
    private bool atWin = false;
    private bool restarted = false;
    private string newSceneName;

    //Start is called before the first frame
    private void Start() {
        if (!levelOne) {
            if (mainCamera != null) {
                mainCamera.transform.position = startPoint.transform.position;
            }
            if (winScene) {
                moving = true;
                atWin = false;
                atEnd = false;
            }
            else {
                moving = false;
                atEnd = false;
            }
        }
    }

    //Update is called once per frame
    private void Update() {
        if (!levelOne) {
            if (moving) {
                if (winScene) {
                    if (!restarted) {
                        if (!atWin) {
                            mainCamera.GetComponent<Rigidbody2D>().velocity = Vector2.left * 5f;
                        }
                        else {
                            moving = false;
                        }
                    }
                    else {
                        if (!atEnd) {
                            mainCamera.GetComponent<Rigidbody2D>().velocity = Vector2.right * 5f;
                        }
                        else {
                            moving = false;
                        }
                    }
                }
                else {
                    if (!atEnd) {
                        mainCamera.GetComponent<Rigidbody2D>().velocity = Vector2.right * 5f;
                    }
                    else {
                        moving = false;
                    }
                }
            }
            else {
                if (winScene) {
                    if (!restarted) {
                        if (atWin) {
                            mainCamera.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                            boat.GetComponent<SpriteRenderer>().flipX = false;
                            atEnd = false;
                            winMenu.SetActive(true);
                        }
                    }
                    else {
                        if (atEnd) {
                            mainCamera.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                            NewScene(newSceneName);
                        }
                    }
                }
                else {
                    if (atEnd) {
                        mainCamera.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                        NewScene(newSceneName);
                    }
                }
            }
        }
    }

    //This public method quits the game
    public void Quit() {
        Application.Quit();
    }

    //This public method loads a scene based on its name
    public void NewScene(string sceneName) {
        SceneManager.LoadScene(sceneName);
    }

    //This public method handles the start button functionality
    public void StartButton(string sceneName) {
        newSceneName = sceneName;
        moving = true;
        if (winScene) {
            restarted = true;
        }
    }

    //This checks for entering the various zones
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.name == "End Zone") {
            atEnd = true;
        }
        if (collision.gameObject.name == "Win Zone") {
            atWin = true;
        }
    }

    //This checks for exiting the various zones
    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.name == "End Zone") {
            atEnd = false;
        }
        if (collision.gameObject.name == "Win Zone") {
            atWin = false;
        }
    }
}
