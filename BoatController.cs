using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**
 * Author: Rebekah Bledsoe
 * Version: 01/22/2026
 * Summary: The function of this script is to control the systems for
 *          the boat and its treasure selling menu.
 **/
public class BoatController : MonoBehaviour
{
    [SerializeField] private GameObject[] treasures;
    [SerializeField] private GameObject treasureMenu;
    [SerializeField] private PlayerController playerController;

    private int collectedTreasures = 0;
    private bool isPaused = false;

    //This method checks for when the player object enters the trigger area and opens the treasure menu
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.name == "Player") {
            if (!treasureMenu.activeInHierarchy) {
                treasureMenu.SetActive(true);
                isPaused = true;
            }
        }
    }

    //This method returns the pause status of the boat menu
    public bool GetPauseStatus() {
        return isPaused;
    }

    //This public method unpauses the game for button functionality
    public void UnpauseGame() {
        if (treasureMenu.activeInHierarchy) {
            treasureMenu.SetActive(false);
            isPaused = false;
            IEnumerator depletionRoutine = playerController.Waiter(0.5f);
            StartCoroutine(depletionRoutine);
        }
    }

    //This public method is used for storing collected treasure, which will display on the dock
    public void StoreTreasure(GameObject treasure) {
        treasure.SetActive(false);
    }
}
