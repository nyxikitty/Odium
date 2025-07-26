using Odium.ButtonAPI.QM;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.UI;
using VRC.Localization;

namespace Odium.Api.MM
{
    internal class MMDivider : UiElement
    {
        protected MMContainer _container;
        protected Image _separatorImage;
        protected RectTransform _rectTransform;

        internal MMDivider(MMContainer container, float height = 2f, Color? color = null)
            => Init(container, height, color);

        private void Init(MMContainer container, float height, Color? color)
        {
            _container = container;

            var separatorTemplate = GetSeparatorTemplate();
            if (separatorTemplate == null)
            {
                MelonLoader.MelonLogger.Error("Could not find separator template");
                return;
            }

            _obj = Object.Instantiate(separatorTemplate, _container.GetPlacementTransform(), false);
            _obj.name = ApiUtils.Identifier + "-Divider-" + ApiUtils.RandomNumbers();

            _separatorImage = _obj.GetComponent<Image>();
            _rectTransform = _obj.GetComponent<RectTransform>();

            SetHeight(height);

            if (color.HasValue)
                SetColor(color.Value);
        }

        private GameObject GetSeparatorTemplate()
        {
            try
            {
                var canvas = GameObject.Find("UserInterface/Canvas_MainMenu(Clone)");
                if (canvas == null) return null;

                var separatorPath = "Container/MMParent/HeaderOffset/Menu_Settings/Menu_MM_DynamicSidePanel/Panel_SectionList/ScrollRect_Navigation_Container/ScrollRect_Content/Viewport/VerticalLayoutGroup/AudioAndVoice/AudioVolume/Settings_Panel_1/VerticalLayoutGroup/Separator";

                var separatorTransform = canvas.transform.Find(separatorPath);
                if (separatorTransform == null)
                {
                    MelonLoader.MelonLogger.Warning("Separator template not found at expected path, searching for alternative...");
                    return FindAlternativeSeparator();
                }

                return separatorTransform.gameObject;
            }
            catch (System.Exception ex)
            {
                MelonLoader.MelonLogger.Error($"Error finding separator template: {ex.Message}");
                return CreateFallbackSeparator();
            }
        }

        private GameObject FindAlternativeSeparator()
        {
            try
            {
                var allSeparators = Object.FindObjectsOfType<Transform>();
                foreach (var transform in allSeparators)
                {
                    if (transform.name.ToLower().Contains("separator") &&
                        transform.GetComponent<Image>() != null)
                    {
                        MelonLoader.MelonLogger.Msg($"Found alternative separator: {transform.name}");
                        return transform.gameObject;
                    }
                }

                MelonLoader.MelonLogger.Warning("No separator found, creating fallback");
                return CreateFallbackSeparator();
            }
            catch (System.Exception ex)
            {
                MelonLoader.MelonLogger.Error($"Error in alternative separator search: {ex.Message}");
                return CreateFallbackSeparator();
            }
        }

        private GameObject CreateFallbackSeparator()
        {
            try
            {
                var separatorObj = new GameObject("FallbackSeparator");

                var rectTransform = separatorObj.AddComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(400f, 2f);

                var image = separatorObj.AddComponent<Image>();
                image.color = new Color(0.5f, 0.5f, 0.5f, 0.8f);

                var layoutElement = separatorObj.AddComponent<LayoutElement>();
                layoutElement.preferredHeight = 2f;
                layoutElement.flexibleWidth = 1f;
                layoutElement.minHeight = 1f;

                MelonLoader.MelonLogger.Msg("Created fallback separator");
                return separatorObj;
            }
            catch (System.Exception ex)
            {
                MelonLoader.MelonLogger.Error($"Error creating fallback separator: {ex.Message}");
                return null;
            }
        }

        internal void SetHeight(float height)
        {
            if (_rectTransform != null)
            {
                var sizeDelta = _rectTransform.sizeDelta;
                sizeDelta.y = height;
                _rectTransform.sizeDelta = sizeDelta;

                var layoutElement = _obj.GetComponent<LayoutElement>();
                if (layoutElement != null)
                {
                    layoutElement.preferredHeight = height;
                    layoutElement.minHeight = Mathf.Min(1f, height);
                }
            }
        }

        internal void SetColor(Color color)
        {
            if (_separatorImage != null)
            {
                _separatorImage.color = color;
            }
        }

        internal void SetAlpha(float alpha)
        {
            if (_separatorImage != null)
            {
                var color = _separatorImage.color;
                color.a = alpha;
                _separatorImage.color = color;
            }
        }

        internal void SetWidth(float width)
        {
            if (_rectTransform != null)
            {
                var sizeDelta = _rectTransform.sizeDelta;
                sizeDelta.x = width;
                _rectTransform.sizeDelta = sizeDelta;

                var layoutElement = _obj.GetComponent<LayoutElement>();
                if (layoutElement != null)
                {
                    layoutElement.preferredWidth = width;
                    layoutElement.flexibleWidth = 0f;
                }
            }
        }

        internal void SetMargins(float top = 0f, float bottom = 0f)
        {
            var layoutElement = _obj.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = _obj.AddComponent<LayoutElement>();
            }

            if (top > 0f || bottom > 0f)
            {
                if (top > 0f)
                {
                    CreateSpacer(true, top);
                }

                if (bottom > 0f)
                {
                    CreateSpacer(false, top);
                }
            }
        }

        private void CreateSpacer(bool isTop, float height)
        {
            var spacer = new GameObject(isTop ? "TopSpacer" : "BottomSpacer");
            var spacerRect = spacer.AddComponent<RectTransform>();
            var spacerLayout = spacer.AddComponent<LayoutElement>();

            spacerLayout.preferredHeight = height;
            spacerLayout.flexibleWidth = 1f;

            if (isTop)
            {
                spacer.transform.SetParent(_obj.transform.parent, false);
                spacer.transform.SetSiblingIndex(_obj.transform.GetSiblingIndex());
            }
            else
            {
                spacer.transform.SetParent(_obj.transform.parent, false);
                spacer.transform.SetSiblingIndex(_obj.transform.GetSiblingIndex() + 1);
            }
        }

        internal static MMDivider CreateThinDivider(MMContainer container, Color? color = null)
        {
            return new MMDivider(container, 1f, color ?? new Color(0.3f, 0.3f, 0.3f, 0.6f));
        }

        internal static MMDivider CreateThickDivider(MMContainer container, Color? color = null)
        {
            return new MMDivider(container, 4f, color ?? new Color(0.2f, 0.2f, 0.2f, 0.8f));
        }

        internal static MMDivider CreateSpacerDivider(MMContainer container, float height = 10f)
        {
            var divider = new MMDivider(container, height, Color.clear);
            return divider;
        }

        internal static MMDivider CreateColoredDivider(MMContainer container, Color color, float height = 2f)
        {
            return new MMDivider(container, height, color);
        }
    }
}