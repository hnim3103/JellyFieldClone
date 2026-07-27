using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GoalDisplay : MonoBehaviour {
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private RectTransform goalContainer;

    [Header("Sprites & Assets")]
    [SerializeField] private Sprite checkSprite;
    [SerializeField] private TMP_FontAsset fontAsset;

    [Header("UI Layout & Aesthetics")]
    [SerializeField] private float cardWidth = 90f;
    [SerializeField] private float cardHeight = 110f;
    [SerializeField] private float blockSize = 56f;
    [SerializeField] private float spacing = 16f;
    [SerializeField] private float countFontSize = 26f;

    private readonly Dictionary<JellyColor, TextMeshProUGUI> countTexts = new Dictionary<JellyColor, TextMeshProUGUI>();
    private readonly Dictionary<JellyColor, Image> checkImages = new Dictionary<JellyColor, Image>();
    private readonly Dictionary<JellyColor, Image> colorBlocks = new Dictionary<JellyColor, Image>();
    private readonly Dictionary<JellyColor, Image> cardBackgrounds = new Dictionary<JellyColor, Image>();

    // Vibrant, rich jelly color palette for UI
    private static readonly Dictionary<JellyColor, Color> ColorMap = new Dictionary<JellyColor, Color> {
        { JellyColor.Aquamarine, new Color(0.12f, 0.88f, 0.75f) }, // Vibrant Teal/Cyan
        { JellyColor.Emerald, new Color(0.15f, 0.82f, 0.40f) }, // Rich Green
        { JellyColor.Ruby, new Color(0.95f, 0.26f, 0.28f) }, // Vibrant Red
        { JellyColor.Sapphire, new Color(0.22f, 0.58f, 0.95f) }, // Bright Blue
        { JellyColor.Topaz, new Color(1.00f, 0.76f, 0.08f) }, // Warm Amber Yellow
    };

    private void Awake() {
        if (goalContainer == null) {
            goalContainer = transform as RectTransform;
        }
    }

    public void Initialize(LevelTarget[] targets) {

        ClearAll();

        if (targets == null || targets.Length == 0) return;

        EnsureLayoutGroup();

        for (int i = 0; i < targets.Length; i++) {
            JellyColor color = (JellyColor)targets[i].color;
            CreateGoalCard(color, targets[i].count);
        }

        RefreshUI();
    }

    private void ClearAll() {
        if (goalContainer == null) return;

        for (int i = goalContainer.childCount - 1; i >= 0; i--) {
            Destroy(goalContainer.GetChild(i).gameObject);
        }
        countTexts.Clear();
        checkImages.Clear();
        colorBlocks.Clear();
        cardBackgrounds.Clear();
    }

    private void EnsureLayoutGroup() {
        if (goalContainer == null) return;

        HorizontalLayoutGroup hlg = goalContainer.GetComponent<HorizontalLayoutGroup>();
        if (hlg == null) {
            hlg = goalContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
        }
        hlg.spacing = spacing;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        hlg.padding = new RectOffset(16, 16, 8, 8);

        ContentSizeFitter csf = goalContainer.GetComponent<ContentSizeFitter>();
        if (csf == null) {
            csf = goalContainer.gameObject.AddComponent<ContentSizeFitter>();
        }
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    private void CreateGoalCard(JellyColor jellyColor, int count) {
        // Main Card Container
        GameObject cardObj = new GameObject($"GoalCard_{jellyColor}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
        RectTransform cardRect = cardObj.GetComponent<RectTransform>();
        cardRect.SetParent(goalContainer, false);
        cardRect.sizeDelta = new Vector2(cardWidth, cardHeight);

        LayoutElement le = cardObj.GetComponent<LayoutElement>();
        le.preferredWidth = cardWidth;
        le.preferredHeight = cardHeight;

        Image cardBg = cardObj.GetComponent<Image>();
        cardBg.color = new Color(0.08f, 0.05f, 0.15f, 0.55f); // Soft dark violet backdrop

        Outline cardOutline = cardObj.AddComponent<Outline>();
        cardOutline.effectColor = new Color(1f, 1f, 1f, 0.15f);
        cardOutline.effectDistance = new Vector2(1f, -1f);

        cardBackgrounds[jellyColor] = cardBg;

        // Color Block
        GameObject blockObj = new GameObject("ColorBlock", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform blockRect = blockObj.GetComponent<RectTransform>();
        blockRect.SetParent(cardRect, false);
        blockRect.anchorMin = new Vector2(0.5f, 1f);
        blockRect.anchorMax = new Vector2(0.5f, 1f);
        blockRect.pivot = new Vector2(0.5f, 1f);
        blockRect.sizeDelta = new Vector2(blockSize, blockSize);
        blockRect.anchoredPosition = new Vector2(0f, -10f);

        Image blockImage = blockObj.GetComponent<Image>();
        if (ColorMap.TryGetValue(jellyColor, out Color uiColor)) {
            blockImage.color = uiColor;
        }

        // Inner subtle outline 
        Outline blockOutline = blockObj.AddComponent<Outline>();
        blockOutline.effectColor = new Color(1f, 1f, 1f, 0.35f);
        blockOutline.effectDistance = new Vector2(1.5f, -1.5f);

        colorBlocks[jellyColor] = blockImage;

        //  Checkmark Icon 
        GameObject checkObj = new GameObject("CheckIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform checkRect = checkObj.GetComponent<RectTransform>();
        checkRect.SetParent(cardRect, false);
        checkRect.anchorMin = new Vector2(0.5f, 0f);
        checkRect.anchorMax = new Vector2(0.5f, 0f);
        checkRect.pivot = new Vector2(0.5f, 0f);
        checkRect.sizeDelta = new Vector2(countFontSize + 6f, countFontSize + 6f);
        checkRect.anchoredPosition = new Vector2(0f, 10f);

        Image checkImg = checkObj.GetComponent<Image>();
        if (checkSprite != null) {
            checkImg.sprite = checkSprite;
        }
        checkImg.color = new Color(0.25f, 0.95f, 0.45f); 
        checkObj.SetActive(false); 

        checkImages[jellyColor] = checkImg;

        // Remaining Count Text 
        GameObject textObj = new GameObject("CountText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.SetParent(cardRect, false);
        textRect.anchorMin = new Vector2(0.5f, 0f);
        textRect.anchorMax = new Vector2(0.5f, 0f);
        textRect.pivot = new Vector2(0.5f, 0f);
        textRect.sizeDelta = new Vector2(cardWidth, 32f);
        textRect.anchoredPosition = new Vector2(0f, 8f);

        TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();

        // Copy font asset if null
        TMP_FontAsset fontToUse = fontAsset;
        if (fontToUse == null) {
            TextMeshProUGUI existingText = FindAnyObjectByType<TextMeshProUGUI>();
            if (existingText != null && existingText.font != null) {
                fontToUse = existingText.font;
            }
        }
        if (fontToUse != null) {
            tmp.font = fontToUse;
        }

        tmp.text = count.ToString();
        tmp.fontSize = countFontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.fontStyle = FontStyles.Bold;
        tmp.enableAutoSizing = false;

        countTexts[jellyColor] = tmp;
    }

    public void RefreshUI() {
        if (scoreManager == null) return;

        foreach (var kvp in countTexts) {
            JellyColor colorKey = kvp.Key;
            int remaining = scoreManager.GetRemaining(colorKey);

            if (remaining <= 0) {
                // Goal Completed - Hide count text, show Checkmark Sprite
                kvp.Value.gameObject.SetActive(false);

                if (checkImages.TryGetValue(colorKey, out Image checkImg)) {
                    checkImg.gameObject.SetActive(true);
                }

                // Soft dim and green glow on completed card
                if (colorBlocks.TryGetValue(colorKey, out Image block)) {
                    Color c = block.color;
                    c.a = 0.5f;
                    block.color = c;
                }

                if (cardBackgrounds.TryGetValue(colorKey, out Image bg)) {
                    bg.color = new Color(0.05f, 0.25f, 0.12f, 0.65f); // Greenish completed tint
                }
            } 
            else {
                // Goal Pending - Show count text, hide Checkmark Sprite
                kvp.Value.gameObject.SetActive(true);
                kvp.Value.text = remaining.ToString();

                if (checkImages.TryGetValue(colorKey, out Image checkImg)) {
                    checkImg.gameObject.SetActive(false);
                }

                if (colorBlocks.TryGetValue(colorKey, out Image block)) {
                    Color c = block.color;
                    c.a = 1.0f;
                    block.color = c;
                }

                if (cardBackgrounds.TryGetValue(colorKey, out Image bg)) {
                    bg.color = new Color(0.08f, 0.05f, 0.15f, 0.55f);
                }
            }
        }
    }
}
