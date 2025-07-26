
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnhollowerRuntimeLib;
using System;

namespace Odium.ButtonAPI.QM
{
    public class QMEnum : QMEnumBase
    {
        public QMEnum(QMMenuBase enumMenu, float enumXLocation, float enumYLocation, Action<int> onIncrement, Action<int> onDecrement, string text, int defaultIndex, string[] options)
        {
            enumQMLoc = enumMenu.GetMenuName();
            Initialize(enumXLocation, enumYLocation, onIncrement, onDecrement, text, defaultIndex, options);
        }

        private Button LeftButtonOptions;
        private Button RightButtonOptions;
        private TextMeshProUGUIEx SelectionBoxTextOptions;
        private TextMeshProUGUIEx EnumNameOptions;
        private Transform OptionSelectionBox;
        private Transform LeftButton;
        private Transform RightButton;

        public int enumIndex = 0;
        public string[] enumOptions = new string[] { };

        private void Initialize(float enumXLocation, float enumYLocation, Action<int> onIncrement, Action<int> onDecrement, string text, int defaultIndex, string[] options)
        {
            if (parent == null)
                parent = ApiUtils.QuickMenu.transform.Find("CanvasGroup/Container/Window/QMParent/" + enumQMLoc).transform;
            
            enumHolder = UnityEngine.Object.Instantiate(ApiUtils.GetQMEnumTemplate(), parent, true);

            Transform rightObjects = enumHolder.transform.Find("RightItemContainer");
            Transform leftObjects = enumHolder.transform.Find("LeftItemContainer");

            LeftButton = rightObjects.Find("ButtonLeft");
            OptionSelectionBox = rightObjects.Find("OptionSelectionBox");
            RightButton = rightObjects.Find("ButtonRight");

            LeftButtonOptions = LeftButton.GetComponent<Button>();
            RightButtonOptions = RightButton.GetComponent<Button>();

            SelectionBoxTextOptions = OptionSelectionBox.GetComponentInChildren<TextMeshProUGUIEx>();

            enumHolder.name = $"{ApiUtils.Identifier}-Enum-{ApiUtils.RandomNumbers()}";

            enumHolder.GetComponent<RectTransform>().sizeDelta = new Vector2(950f, 100f);
            enumHolder.GetComponent<RectTransform>().anchoredPosition = new Vector2(40f, -310f);

            enumOptions = options;

            initShift[0] = 0;
            initShift[1] = 0;
            SetDefaultOption(defaultIndex);
            InitializeRightButton(onIncrement);
            InitializeLeftButton(onDecrement);
            SetLocation(enumXLocation, enumYLocation);
            SetActive(true);

            // placeholder text
            EnumNameOptions = leftObjects.GetComponentInChildren<TextMeshProUGUIEx>();
            EnumNameOptions.prop_String_0 = text;

        }

        public int GetCurrentIndex() => enumIndex;

        public string GetCurrentValue() => SelectionBoxTextOptions.prop_String_0;

        private void InitializeRightButton(Action<int> onIncrement)
        {
            RightButtonOptions.onClick.AddListener(new Action(() => 
            {
                enumIndex = (enumIndex + 1) % enumOptions.Length;
                UpdateSelectionBox(enumIndex);
                onIncrement(enumIndex);
            }));
        }

        private void InitializeLeftButton(Action<int> onDecrement)
        {
            LeftButtonOptions.onClick.AddListener(new Action(() => 
            {
                enumIndex = (enumIndex - 1 + enumOptions.Length) % enumOptions.Length;
                UpdateSelectionBox(enumIndex);
                onDecrement(enumIndex);
            }));
        }

        public void SetDefaultOption(int index)
        {
            enumIndex = index % enumOptions.Length;
            UpdateSelectionBox(enumIndex);
        }

        public void UpdateSelectionBox(int index)
        {
            SelectionBoxTextOptions.prop_String_0 = enumOptions[index];
        }

        public void SetInteractable(bool newState)
        {
            RightButtonOptions.interactable = newState;
            LeftButtonOptions.interactable = newState;
            RefreshEnum();
        }

        private void RefreshEnum()
        {
            enumHolder.SetActive(false);
            enumHolder.SetActive(true);
        }
    }
}
