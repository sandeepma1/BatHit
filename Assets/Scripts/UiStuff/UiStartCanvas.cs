﻿using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UiStartCanvas : MonoBehaviour
{
    public static Action<bool> OnToggleUiStartPanel;
    public static Action OnGameStart;
    public static Action OnSettingsButtonPressed;
    public static Action OnLeaderboardButtonPressed;
    public static Action OnAchievementsButtonPressed;
    public static Action OnShopButtonPressed;
    public static Action OnDailyRewardsButtonPressed;

    [SerializeField] private Button tapToStartButton;
    [Space(10)]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private RectTransform topPanelRect;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [Space(10)]    //Left Buttons
    [SerializeField] private RectTransform leftButtonsRect;
    [SerializeField] private Button leaderboardButton;
    [SerializeField] private Button achievementsButton;
    [SerializeField] private Button reviewAppButton;
    [Space(10)]    //Right Buttons
    [SerializeField] private RectTransform rightButtonsRect;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button rewardsButton;
    [SerializeField] private TextMeshProUGUI dailyRewardsText;

    [Space(10)]    //Bottom Buttons
    [SerializeField] private RectTransform bottomButtonsRect;
    [SerializeField] private Button gemsShopButton;
    [SerializeField] private Button shopButton;

    private TimeSpan rewardTimeSpan;
    private float topHideYPos;
    private float leftHideXPos;
    private float rightHideXPos;
    private float bottomHideYPos;
    private float leftShowXPos = 15;
    private float rightShowXPos = -15;
    private float bottomShowYPos = 100;
    private const float animSpeed = 0.25f;

    private void Awake()
    {
        mainPanel.gameObject.SetActive(true);
        GpsManager.OnSaveDataLoaded += OnCloudDataLoaded;
        OnToggleUiStartPanel += ToggleUiStartPanel;
        Player.OnPlayerDataLoaded += OnPlayerDataLoaded;
        Player.OnUpdateRewardTimer += OnUpdateRewardTimer;
        Player.OnRewardAvailable += OnRewardAvailable;
    }

    private void Start()
    {
        StartCoroutine(GetAllPanelSizes());
        tapToStartButton.onClick.AddListener(OnTapToStartButtonClicked);
        leaderboardButton.onClick.AddListener(OnLeaderBoardButtonClicked);
        achievementsButton.onClick.AddListener(OnAchievementsButtonClicked);
        rewardsButton.onClick.AddListener(OnRewardsButtonPressed);
        shopButton.onClick.AddListener(OnShopButtonClicked);
        settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        gemsShopButton.onClick.AddListener(() => UiGemsShopCanvas.OnShowBuyGemsMenu?.Invoke());
        UpdateAllSavedValues();
        InvokeRepeating("CheckReward", 1f, 1f);
        if (!Player.IsPlayerDataNull())
        {
            OnCloudDataLoaded(Player.isCloudDataLoaded, "");
        }
    }

    private void OnDestroy()
    {
        GpsManager.OnSaveDataLoaded -= OnCloudDataLoaded;
        Player.OnPlayerDataLoaded -= OnPlayerDataLoaded;
        Player.OnUpdateRewardTimer -= OnUpdateRewardTimer;
        Player.OnRewardAvailable -= OnRewardAvailable;
        OnToggleUiStartPanel -= ToggleUiStartPanel;
        tapToStartButton.onClick.RemoveListener(OnTapToStartButtonClicked);
        leaderboardButton.onClick.RemoveListener(OnLeaderBoardButtonClicked);
        achievementsButton.onClick.RemoveListener(OnAchievementsButtonClicked);
        rewardsButton.onClick.RemoveListener(OnRewardsButtonPressed);
        shopButton.onClick.RemoveListener(OnShopButtonClicked);
        settingsButton.onClick.RemoveListener(OnSettingsButtonClicked);
        gemsShopButton.onClick.RemoveListener(() => UiGemsShopCanvas.OnShowBuyGemsMenu?.Invoke());
    }

    private void OnCloudDataLoaded(bool isCloudDataLoaded, string arg2)
    {
        leaderboardButton.interactable = isCloudDataLoaded;
        achievementsButton.interactable = isCloudDataLoaded;
    }

    private void OnPlayerDataLoaded()
    {
        UpdateAllSavedValues();
    }

    private IEnumerator GetAllPanelSizes()
    {
        yield return new WaitForEndOfFrame();
        topHideYPos = topPanelRect.rect.height + 100;
        leftHideXPos = leftButtonsRect.rect.width + 100;
        rightHideXPos = rightButtonsRect.rect.width + 100;
        bottomHideYPos = -bottomButtonsRect.rect.height - 100;
        leftShowXPos = leftButtonsRect.anchoredPosition.x;
        rightShowXPos = rightButtonsRect.anchoredPosition.x;
        bottomShowYPos = bottomButtonsRect.anchoredPosition.y;
    }

    private void ToggleUiStartPanel(bool isVisible)
    {
        tapToStartButton.gameObject.SetActive(isVisible);
        if (isVisible)
        {
            mainPanel.SetActive(isVisible);
            topPanelRect.DOAnchorPosY(0, animSpeed);
            leftButtonsRect.DOAnchorPosX(leftShowXPos, animSpeed);
            rightButtonsRect.DOAnchorPosX(rightShowXPos, animSpeed);
            bottomButtonsRect.DOAnchorPosY(bottomShowYPos, animSpeed);
        }
        else
        {
            topPanelRect.DOAnchorPosY(topHideYPos, animSpeed);
            leftButtonsRect.DOAnchorPosX(-leftHideXPos, animSpeed);
            rightButtonsRect.DOAnchorPosX(rightHideXPos, animSpeed);
            bottomButtonsRect.DOAnchorPosY(bottomHideYPos, animSpeed).OnComplete(() => mainPanel.SetActive(isVisible));
        }
    }


    #region Rewards stuff
    //Dont delete, used by invoke repeating
    private void CheckReward()
    {
        if (Player.IsPlayerDataNull())
        {
            return;
        }
        rewardTimeSpan = Player.rewardsDateTime.Subtract(DateTime.UtcNow);
        if (rewardTimeSpan.TotalSeconds <= 0)
        {
            OnRewardAvailable();
        }
        else
        {
            //OnUpdateRewardTimer(rewardTimeSpan.ToFormattedDuration());
            OnUpdateRewardTimer(string.Format("{0:D2}:{1:D2}:{2:D2}",
                rewardTimeSpan.Hours, rewardTimeSpan.Minutes, rewardTimeSpan.Seconds));
        }
    }

    private void OnRewardAvailable()
    {
        rewardsButton.interactable = true;
        dailyRewardsText.text = "Ready";
    }

    private void OnUpdateRewardTimer(string rewardText)
    {
        rewardsButton.interactable = false;
        dailyRewardsText.text = rewardText;
    }

    private void OnRewardsButtonPressed()
    {
        rewardsButton.interactable = false;
        Player.RewardDateTime = DateTime.UtcNow.AddHours(AppData.nextRewardInHours);
        UiGemsSpawnCanvas.OnSpawnMultipleGem2d?.Invoke(AppData.dailyGemsRewards);
        AnalyticsManager.ButtonPressed(GameButtons.DailyReward);
    }

    private void UpdateAllSavedValues()
    {
        if (Player.IsPlayerDataNull())
        {
            return;
        }
        highScoreText.text = "High Score: " + Player.GetHighScore();
    }
    #endregion

    private void OnShopButtonClicked()
    {
        OnShopButtonPressed?.Invoke();
        AnalyticsManager.ButtonPressed(GameButtons.Shop);
    }

    private void OnSettingsButtonClicked()
    {
        OnSettingsButtonPressed?.Invoke();
        AnalyticsManager.ButtonPressed(GameButtons.Settings);
    }

    private void OnTapToStartButtonClicked()
    {
        ToggleUiStartPanel(false);
        OnGameStart?.Invoke();
        AnalyticsManager.GameStart();
    }

    private void OnAchievementsButtonClicked()
    {
        GpsManager.Instance.ShowAchievementsUI();
        AnalyticsManager.ScreenVisit(GameScreens.Achievements);
        AnalyticsManager.ButtonPressed(GameButtons.Achievements);
    }

    private void OnLeaderBoardButtonClicked()
    {
        GpsManager.Instance.ShowLeaderboardUI();
        AnalyticsManager.ScreenVisit(GameScreens.Leaderboards);
        AnalyticsManager.ButtonPressed(GameButtons.Leaderboards);
    }
}