using UnityEngine;
using UnityEngine.UI;
public class HeartController : MonoBehaviour
{
    PlayerController Player;

    private GameObject[] heartContainers;
    private Image[] heartFills;
    public Transform heartsParent;
    public GameObject heartContainerPrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Player = PlayerController.Instance;
        heartContainers = new GameObject [PlayerController.Instance.MaxHealth];
        heartFills = new Image [PlayerController.Instance.MaxHealth];

        PlayerController.Instance.onHealthChangedCallBack += UpdateHeartsHUD;
        InstantiateHeartContainers();
        UpdateHeartsHUD();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void SetHeartContainers()
    {
        for (int i =0; i < heartContainers.Length; i++)
        {
            if (i < PlayerController.Instance.MaxHealth)
            {
                    heartContainers[i].SetActive(true);
            }
            else
            {
                heartContainers[i].SetActive(false);
            }
        }
    }
    void SetFilledHearts()
    {
        for (int i = 0; i < heartFills.Length; i++)
        {
            if (i < PlayerController.Instance.Health)
            {
                heartFills[i].fillAmount = 1;
            }
            else
            {
                heartFills[i].fillAmount = 0;
            }
        }
    }
    void InstantiateHeartContainers()
        {
        for (int i = 0; i < PlayerController.Instance.MaxHealth; i++)
        {
            GameObject temp = Instantiate(heartContainerPrefab);
            temp.transform.SetParent(heartsParent, false);
            heartContainers[i] = temp;
            heartFills[i] = temp.transform.Find("HeartFilled").GetComponent<Image>();
        }
    }
    void UpdateHeartsHUD()
    {
        SetHeartContainers();
        SetFilledHearts();
    }
}
