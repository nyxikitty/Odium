using UnityEngine;

namespace Odium.Api
{
    internal class UiElement
    {
        protected GameObject _obj;
        protected Transform _placementTransform;
        protected TextMeshProUGUIEx _text;
        protected ToolTip[] _toolTips;

        internal GameObject GetGameObject()
        {
            return _obj;
        }

        internal Transform GetPlacementTransform()
        {
            return _placementTransform;
        }
    }
}
