using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class SpaceShipItem
{
    public string shipName = "Explorer";
    public Sprite shipSprite;
    public int price = 0; // 0 là miễn phí mặc định
}

/// <summary>
/// Quản lý Hangar / Cửa hàng Phi Thuyền (Shop):
/// - Tích lũy sao/tiền vàng từ các màn chơi.
/// - Mở khóa và mua các mẫu phi thuyền mới.
/// - Lưu trữ tàu đang chọn và đồng bộ vào màn chơi chính.
/// - Hiển thị Điểm Kỷ Lục (High Score) và Tổng Sao trên Menu.
/// </summary>
public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("Danh Sách Phi Thuyền Trong Shop")]
    public List<SpaceShipItem> availableShips = new List<SpaceShipItem>();

    [Header("Giao diện Shop UI")]
    public GameObject shopPanel;
    public Image shipPreviewImage;
    public TextMeshProUGUI shipNameText;
    public TextMeshProUGUI shipPriceText;
    public Button actionButton;
    public TextMeshProUGUI actionButtonText;

    [Header("Hiển thị Tiền & Kỷ lục trên Menu")]
    public TextMeshProUGUI menuStarsText;
    public TextMeshProUGUI menuHighScoreText;

    [Header("Hình Tàu Ngoài Menu Chính (Tự đổi khi chọn)")]
    public Image menuShipDecoration;

    private int currentViewingIndex = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        // Mặc định tàu đầu tiên luôn được mở khóa
        PlayerPrefs.SetInt("ShipUnlocked_0", 1);

        UpdateCurrencyAndHighScoreUI();

        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }

        // Cập nhật tàu đang trang bị lên màn hình Menu
        int selectedIndex = PlayerPrefs.GetInt("SelectedShip", 0);
        UpdateMenuShipDisplay(selectedIndex);
    }

    /// <summary>
    /// Cập nhật giao diện số sao và điểm kỷ lục trên menu
    /// </summary>
    public void UpdateCurrencyAndHighScoreUI()
    {
        int totalStars = PlayerPrefs.GetInt("TotalStars", 0);
        int highScore = PlayerPrefs.GetInt("HighScore", 0);

        if (menuStarsText != null)
        {
            menuStarsText.text = "STARS: " + totalStars;
        }

        if (menuHighScoreText != null)
        {
            menuHighScoreText.text = "BEST: " + highScore;
        }
    }

    /// <summary>
    /// Mở bảng Shop
    /// </summary>
    public void OpenShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
            currentViewingIndex = PlayerPrefs.GetInt("SelectedShip", 0);
            DisplayCurrentShip();
            UpdateCurrencyAndHighScoreUI();
        }
    }

    /// <summary>
    /// Đóng bảng Shop
    /// </summary>
    public void CloseShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
    }

    public void NextShip()
    {
        if (availableShips.Count == 0) return;
        currentViewingIndex = (currentViewingIndex + 1) % availableShips.Count;
        DisplayCurrentShip();
    }

    public void PrevShip()
    {
        if (availableShips.Count == 0) return;
        currentViewingIndex = (currentViewingIndex - 1 + availableShips.Count) % availableShips.Count;
        DisplayCurrentShip();
    }

    /// <summary>
    /// Hiển thị thông tin tàu đang xem trong Shop
    /// </summary>
    void DisplayCurrentShip()
    {
        if (availableShips.Count == 0 || currentViewingIndex >= availableShips.Count) return;

        SpaceShipItem ship = availableShips[currentViewingIndex];

        if (shipPreviewImage != null && ship.shipSprite != null)
        {
            shipPreviewImage.sprite = ship.shipSprite;
        }

        if (shipNameText != null)
        {
            shipNameText.text = ship.shipName;
        }

        bool isUnlocked = PlayerPrefs.GetInt("ShipUnlocked_" + currentViewingIndex, currentViewingIndex == 0 ? 1 : 0) == 1;
        int selectedIndex = PlayerPrefs.GetInt("SelectedShip", 0);
        int totalStars = PlayerPrefs.GetInt("TotalStars", 0);

        if (selectedShipIsEquipped(currentViewingIndex))
        {
            if (shipPriceText != null) shipPriceText.text = "<color=#34D399>EQUIPPED</color>";
            if (actionButtonText != null) actionButtonText.text = "EQUIPPED";
            if (actionButton != null) actionButton.interactable = false;
        }
        else if (isUnlocked)
        {
            if (shipPriceText != null) shipPriceText.text = "<color=#60A5FA>OWNED</color>";
            if (actionButtonText != null) actionButtonText.text = "SELECT";
            if (actionButton != null) actionButton.interactable = true;
        }
        else
        {
            if (shipPriceText != null) shipPriceText.text = "PRICE: <color=#FACC15>" + ship.price + " STARS</color>";
            if (actionButtonText != null) actionButtonText.text = "BUY SHIP";
            // Chỉ cho bấm Mua nếu đủ tiền
            if (actionButton != null) actionButton.interactable = (totalStars >= ship.price);
        }
    }

    bool selectedShipIsEquipped(int index)
    {
        return PlayerPrefs.GetInt("SelectedShip", 0) == index;
    }

    /// <summary>
    /// Bấm nút Mua hoặc Chọn tàu
    /// </summary>
    public void OnActionButtonClicked()
    {
        if (availableShips.Count == 0) return;

        SpaceShipItem ship = availableShips[currentViewingIndex];
        bool isUnlocked = PlayerPrefs.GetInt("ShipUnlocked_" + currentViewingIndex, currentViewingIndex == 0 ? 1 : 0) == 1;
        int totalStars = PlayerPrefs.GetInt("TotalStars", 0);

        if (!isUnlocked)
        {
            // Mua tàu
            if (totalStars >= ship.price)
            {
                totalStars -= ship.price;
                PlayerPrefs.SetInt("TotalStars", totalStars);
                PlayerPrefs.SetInt("ShipUnlocked_" + currentViewingIndex, 1);
                PlayerPrefs.SetInt("SelectedShip", currentViewingIndex);
                PlayerPrefs.Save();
            }
        }
        else
        {
            // Trang bị tàu
            PlayerPrefs.SetInt("SelectedShip", currentViewingIndex);
            PlayerPrefs.Save();
        }

        UpdateCurrencyAndHighScoreUI();
        DisplayCurrentShip();
        UpdateMenuShipDisplay(currentViewingIndex);
    }

    void UpdateMenuShipDisplay(int index)
    {
        if (menuShipDecoration != null && index >= 0 && index < availableShips.Count)
        {
            if (availableShips[index].shipSprite != null)
            {
                menuShipDecoration.sprite = availableShips[index].shipSprite;
            }
        }
    }
}
