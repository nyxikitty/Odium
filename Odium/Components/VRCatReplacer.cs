using Odium.ButtonAPI.QM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRC.UI;

namespace Odium.Components
{
    // Big thanks to WTFBlaze
    class VRCatReplacer
    {
        private void OnQuickMenuInit()
        {
            //// Set Default Image
            //ApiUtils.QuickMenu.transform.Find("CanvasGroup/Container/ThankYouCharacter/Character/VRCat_Front").GetComponent<RawImageEx>().texture = IsInEarmuffMode() ? Assets.WraithFrontEarmuff.texture : Assets.WraithFront.texture;
            //ApiUtils.QuickMenu.transform.Find("CanvasGroup/Container/ThankYouCharacter/Character/VRCat_Back").GetComponent<RawImageEx>().texture = Assets.WraithBack.texture;

            //// Set Component Image Overrides
            //var comp = ApiUtils.QuickMenu.transform.Find("CanvasGroup/Container/ThankYouCharacter").GetComponent<VRCatEarmuffsEasterEgg>();
            //comp.regularCatFront = Assets.WraithFront.texture;
            //comp.regularCatBack = Assets.WraithBack.texture;

            //comp.regularRatFront = Assets.WraithFront.texture;
            //comp.regularRatBack = Assets.WraithBack.texture;

            //comp.earmuffCatFront = Assets.WraithFrontEarmuff.texture;
            //comp.earmuffCatBack = Assets.WraithBack.texture;

            //comp.earmuffRatFront = Assets.WraithFrontEarmuff.texture;
            //comp.earmuffRatBack = Assets.WraithBack.texture;
        }

        private bool IsInEarmuffMode()
        {
            return EarmuffsVisualAide.field_Public_Static_EarmuffsVisualAide_0.field_Private_Boolean_0;
        }
    }
}
