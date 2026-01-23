using System.Collections;
using TMPro;
using UnityEngine;


/**
 * Author: Rebekah Bledsoe
 * Version: 01/22/2026
 * Summary: The function of this script is to control the player
 *          and the various systems that surround them. This includes
 *          oxygen depletion/restoration, treasure storage, coin collection,
 *          and the functionality for the pause menu.
 **/
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float movementSpeed = 3.0f;
    [SerializeField] private BoatController boatController;
    [SerializeField] private int currentBagCapacity = 1;
    [SerializeField] private GameObject deathMenu;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject boat;
    [SerializeField] private GameObject[] bag;
    [SerializeField] private TMP_Text oxygenAmount;
    [SerializeField] private TMP_Text goldAmount;
    [SerializeField] private TMP_Text bagUpgradeText;
    [SerializeField] private TMP_Text oxygenUpgradeText;
    [SerializeField] private TMP_Text speedUpgradeText;
    [SerializeField] private AudioSource underwaterSource;
    [SerializeField] private AudioSource airSource;
    [SerializeField] private Sprite goldBoat;

    private int maxBagCapacity = 3;
    private int bagUpgradeCost = 25;
    private int oxygenUpgradeCost = 25;
    private int speedUpgradeCost = 25;
    private int suitUpgradeCost = 75;
    private int boatUpgradeCost = 250;
    private int maxOxygenAmount = 100;
    private int currentOxygenAmount = 100;
    private int goldCoins = 0;
    private bool isFollowing = false;
    private bool oxygenDepleting = false;
    private bool waiting = false;
    private bool coralOxygen = false;
    private bool alive = true;
    private bool start = false;
    private bool finished = false;
    private bool finalSpeed = false;
    private bool finalOxygen = false;
    private bool finalBag = false;
    private bool hasGoldenSuit = false;
    private bool hasGoldenBoat = false;
    private bool gamePaused = false;
    private Vector3 mousePosition;

    //Start is called before the first frame
    private void Start() {
        GetComponent<Collider2D>().isTrigger = true;
        GetComponent<Rigidbody2D>().AddForce(new Vector2(-30, -90), ForceMode2D.Force);
    }


    // Update is called once per frame
    private void Update() {
        if (finished) {
            start = true;
        }
        //This statement allows for the player functionality when the player is alive and has started the game
        if (start) {
            if (alive) {
                //This controls if the user inputs the escape key to pause the game
                //The user cannot enter the pause menu if using the boat menu
                if (boatController.GetPauseStatus() == false) {
                    if (Input.GetKeyDown(KeyCode.Escape)) {
                        if (gamePaused) {
                            gamePaused = false;
                            pauseMenu.SetActive(false);
                        }
                        else {
                            gamePaused = true;
                            pauseMenu.SetActive(true);
                        }
                    }
                }

                //This controls the main function of the player if the game isn't paused by the user
                if (!gamePaused) {
                    //This statement checks for if the game is paused, and if it isn't it allows the run-time gameplay
                    if (boatController.GetPauseStatus() == false) {
                        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        mousePosition.z = transform.position.z;
                        transform.position = Vector3.MoveTowards(transform.position, mousePosition, movementSpeed * Time.deltaTime);

                        //This if statement has the treasure within the bag follow the player
                        if (isFollowing) {
                            for (int i = 0; i < bag.Length; i++) {
                                if (bag[i] != null) {
                                    TreasureFollow(bag[i], i);
                                }
                            }
                        }

                        //This if statement checks for if the oxygen is depleting and acts accordingly
                        if (oxygenDepleting) {
                            for (int i = currentOxygenAmount; i > 0; i--) {
                                if (!waiting) {
                                    if (!boatController.GetPauseStatus()) {
                                        currentOxygenAmount = currentOxygenAmount - 1;
                                        StartCoroutine(Waiter(0.5f));
                                    }
                                    if (currentOxygenAmount < 0) {
                                        currentOxygenAmount = 0;
                                    }
                                }
                            }

                        }
                        else {
                            for (int i = currentOxygenAmount; i < maxOxygenAmount; i++) {
                                if (!waiting) {
                                    if (!boatController.GetPauseStatus()) {
                                        if (coralOxygen) {
                                            currentOxygenAmount = currentOxygenAmount + 1;
                                            StartCoroutine(Waiter(0.5f));
                                        }
                                        else {
                                            currentOxygenAmount = currentOxygenAmount + 1;
                                            StartCoroutine(Waiter(0.05f));
                                        }
                                    }
                                    if (currentOxygenAmount > maxOxygenAmount) {
                                        currentOxygenAmount = maxOxygenAmount;
                                    }
                                }
                            }
                        }
                    }

                    //This statement checks for if the player is alive and sets the bool accordingly
                    if (currentOxygenAmount <= 0) {
                        if (!waiting) {
                            StartCoroutine(Waiter(0.5f));
                        }
                        alive = false;
                    }
                }

                //These lines set the oxygen and gold amount UI for the player to see
                oxygenAmount.text = currentOxygenAmount.ToString();
                goldAmount.text = goldCoins.ToString();

                if (oxygenDepleting) {
                    if (!underwaterSource.isPlaying) {
                        airSource.Stop();
                        underwaterSource.Play();
                    }
                }
                else {
                    if (!airSource.isPlaying) {
                        if (!coralOxygen) {
                            airSource.Play();
                            underwaterSource.Stop();
                        }
                    }
                }

            }
            else {
                if (!waiting) {
                    StartCoroutine(Waiter(1.0f));
                }
                deathMenu.SetActive(true);
            }
        }
    }

    //This waiter method takes an input time and waits that amount
    public IEnumerator Waiter(float waitTime) {
        waiting = true;
        yield return new WaitForSeconds(waitTime);
        waiting = false;
    }


    //This method checks for when the player enters the trigger areas
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.CompareTag("Treasure")) {
            UpdateBag(collision.gameObject);
            isFollowing = true;
        }
        else if (collision.gameObject.CompareTag("Air")) {
            oxygenDepleting = false;
            if (collision.gameObject.name == "Coral Air") {
                coralOxygen = true;
            }
            else if (collision.gameObject.name == "Air") {
                coralOxygen = false;
            }
        }
        else if (collision.gameObject.CompareTag("Water")) {
            if (GetComponent<Collider2D>().isTrigger == true) {
                GetComponent<Collider2D>().isTrigger = false;
                GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                finished = true;
            }
            oxygenDepleting = true;
        }
    }

    //This method checks for when the player leaves trigger areas
    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.CompareTag("Air")) {
            oxygenDepleting = true;
        }
        else if (collision.gameObject.CompareTag("Water")) {
            oxygenDepleting = false;
        }
    }

    //This method takes a game object reference and has it follow the player
    private void TreasureFollow(GameObject treasure, int num) {
        if (num == 0) {
            treasure.transform.position = new Vector3(transform.position.x + 0.5f, transform.position.y - 0.15f, transform.position.z);
        }
        else if (num == 1) {
            treasure.transform.position = new Vector3(transform.position.x - 0.5f, transform.position.y - 0.15f, transform.position.z);
        }
        else {
            treasure.transform.position = new Vector3(transform.position.x, transform.position.y + 1.0f, transform.position.z);
        }
    }

    //This public method upgrades the bag to its max capacity
    public void UpgradeBag(GameObject upgradeButton) {
        if (goldCoins - bagUpgradeCost >= 0) {
            if (currentBagCapacity < maxBagCapacity) {
                currentBagCapacity = currentBagCapacity + 1;
                if (currentBagCapacity > maxBagCapacity) {
                    currentBagCapacity = maxBagCapacity;
                }
            }

            //If the upgrade reaches the max of 3, the button is disabled
            if (currentBagCapacity == 3) {
                upgradeButton.SetActive(false);
                finalBag = true;
            }

            goldCoins = goldCoins - bagUpgradeCost;
            if (bagUpgradeCost != 50) {
                bagUpgradeCost = 50;
                bagUpgradeText.text = "Upgrade Bag - 50g";
            }
        }
        else {
            Debug.Log("You need " + (bagUpgradeCost - goldCoins) + " more coins to upgrade.");
        }
    }

    //This public method upgrades the player's oxygen amount to its max capacity
    public void UpgradeOxygen(GameObject upgradeButton) {
        if (goldCoins - oxygenUpgradeCost >= 0) {
            if (maxOxygenAmount < 300) {
                maxOxygenAmount = maxOxygenAmount + 100;
                if (currentOxygenAmount > maxOxygenAmount) {
                    currentOxygenAmount = maxOxygenAmount;
                }
            }
            
            //If the oxygen amount is at its max of 300, the button is disabled
            if (maxOxygenAmount == 300) {
                upgradeButton.SetActive(false);
                finalOxygen = true;
            }

            goldCoins = goldCoins - oxygenUpgradeCost;
            if (oxygenUpgradeCost != 50) {
                oxygenUpgradeCost = 50;
                oxygenUpgradeText.text = "Upgrade O2 - 50g";
            }
        }
        else {
            Debug.Log("You need " + (oxygenUpgradeCost - goldCoins) + " more coins to upgrade.");
        }
    }

    //This public method upgrades the player's speed to its max value
    public void UpgradeSpeed(GameObject upgradeButton) {
        if (goldCoins - speedUpgradeCost >= 0) {
            if (movementSpeed < 6.0f) {
                movementSpeed = movementSpeed + 1.5f;
                if (movementSpeed > 6.0f) {
                    movementSpeed = 6.0f;
                }
            }

            //If the oxygen amount is at its max of 300, the button is disabled
            if (movementSpeed == 6.0f) {
                upgradeButton.SetActive(false);
                finalSpeed = true;
            }

            goldCoins = goldCoins - speedUpgradeCost;
            if (speedUpgradeCost != 50) {
                speedUpgradeCost = 50;
                speedUpgradeText.text = "Upgrade Speed - 50g";
            }
        }
        else {
            Debug.Log("You need " + (speedUpgradeCost - goldCoins) + " more coins to upgrade.");
        }
    }

    //This method checks the bag based on an int collision value input
    private void UpdateBag(GameObject treasure) {
        if (currentBagCapacity == 1) {
            if (bag[0] == null) {
                bag[0] = treasure;
            }
        }
        else if (currentBagCapacity == 2) {
            if (bag[0] == null) {
                bag[0] = treasure;
            }
            else if (bag[1] == null) {
                bag[1] = treasure;
            }
        }
        else if (currentBagCapacity == 3) {
            if (bag[0] == null) {
                bag[0] = treasure;
            }
            else if (bag[1] == null) {
                bag[1] = treasure;
            }
            else if (bag[2] == null) {
                bag[2] = treasure;
            }
        }
        else {
            Debug.Log("Bag is full!");
        }
    }

    //This public method purchases the golden suit if the player has all maxed upgrades
    public void PurchaseGoldSuit(GameObject upgradeButton) {
        if (goldCoins - suitUpgradeCost >= 0) {
            if (finalBag ==  true && finalOxygen  == true && finalSpeed == true) {
                Debug.Log("suit upgraded");
                GetComponent<Animator>().SetBool("isGold", true);
                GetComponent<SpriteRenderer>().sprite = goldBoat;
                hasGoldenSuit = true;
            }
            goldCoins = goldCoins - suitUpgradeCost;
        }
        else {
            Debug.Log("You need " + (suitUpgradeCost - goldCoins) + " more coins to upgrade.");
        }

        //If the suit is upgraded, the button is disabled
        if (hasGoldenSuit) {
            upgradeButton.SetActive(false);
        }
    }

    //This public method purchases the golden boat
    public void PurchaseGoldBoat(GameObject upgradeButton) {
        if (goldCoins - boatUpgradeCost >= 0) {
            if (hasGoldenSuit == true) {
                gameObject.GetComponent<MenuController>().NewScene("Win Level");
                hasGoldenBoat = true;
            }
            goldCoins = goldCoins - boatUpgradeCost;
        }
        else {
            Debug.Log("You need " + (boatUpgradeCost - goldCoins) + " more coins to upgrade.");
        }

        //If the boat is upgraded, the button is disabled
        if (hasGoldenBoat) {
            upgradeButton.SetActive(false);
        }
    }

    //This public method sells the items on a button press
    public void SellItem() {
        if (bag[0] != null) {
            Transaction(0);
            boatController.StoreTreasure(bag[0]);
            bag[0] = null;
        }
        else if (bag[1] != null) {
            Transaction(1);
            boatController.StoreTreasure(bag[1]);
            bag[1] = null;
        }
        else if (bag[2] != null) {
            Transaction(2);
            boatController.StoreTreasure(bag[2]);
            bag[2] = null;
        }
        else {
            Debug.Log("You need an item to sell it!");
        }
    }

    //This method controls the transactions based on item type
    private void Transaction(int itemID) {
        if (bag[itemID].gameObject.name == "Crown") {
            goldCoins = goldCoins + 20;
        }
        else if (bag[itemID].gameObject.name == "Jewel") {
            goldCoins = goldCoins + 6;
        }
        else if (bag[itemID].gameObject.name == "Pearl") {
            goldCoins = goldCoins + 8;
        }
        else if (bag[itemID].gameObject.name == "Trash") {
            goldCoins = goldCoins + 2;
        }
        else if (bag[itemID].gameObject.name == "Coin") {
            goldCoins = goldCoins + 4;
        }
        else if (bag[itemID].gameObject.name == "Bar") {
            goldCoins = goldCoins + 10;
        }
        else if (bag[itemID].gameObject.name == "Art") {
            goldCoins = goldCoins + 30;
        }
    }
}
