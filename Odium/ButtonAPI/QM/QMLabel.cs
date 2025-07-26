using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Odium.ButtonAPI.QM
{
    public class QMLabel
    {
        private QMMenuBase labelMenu;
        private Color color;
        private Rect rect;
        private int fontSize;
        private bool bold;
        private string text;

        Transform parent;
        GameObject Label;
        TextMeshProUGUIEx LabelOptions;
        RectTransform LabelRect;

        public QMLabel(QMMenuBase labelMenu, Color color, Rect rect, string text, int fontSize, bool bold = true)
        {
            this.labelMenu = labelMenu;
            this.color = color;
            this.rect = rect;
            this.text = text;
            this.fontSize = fontSize;
            this.bold = bold;
            Initialize();
        }

        public void Initialize()
        {
            parent = ApiUtils.QuickMenu.transform.Find(ApiUtils.QuickMenu + labelMenu.GetMenuName()).transform;
            
            // duplicate an existing label and parent it to the menu
            Label = UnityEngine.Object.Instantiate(ApiUtils.GetQMLabelTemplate(), parent, true);
            Label.name = $"{ApiUtils.Identifier}-Label-{ApiUtils.RandomNumbers()}";

            // get label components
            Label.GetComponent<VRC.UI.Core.Styles.StyleElement>().enabled = false;
            LabelOptions = Label.GetComponent<TextMeshProUGUIEx>();
            LabelRect = Label.GetComponent<RectTransform>();

            // set text options
            LabelOptions.text = text;
            LabelOptions.fontSize = fontSize;
            LabelOptions.color = color;
            LabelOptions.fontWeight = TMPro.FontWeight.Regular;

            if (bold)
                LabelOptions.fontStyle = TMPro.FontStyles.Bold;

            // set positon and scale
            SetPosition(rect);
        }

        public void SetPosition(Rect rect)
        {
            LabelRect.anchoredPosition = new Vector2(rect.x, -10 - rect.y);
            LabelRect.sizeDelta = new Vector2(rect.width, rect.height);
        }

        public void SetText() => LabelOptions.text = text;

        public string GetText() => LabelOptions.text;

    }
}
