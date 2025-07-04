using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VRC.Localization;

namespace Odium.Wrappers
{
    internal class LocalizableStringWrapper
    {
        public static VRC.Localization.LocalizableString Create(string str)
        {
            var lstring = new VRC.Localization.LocalizableString();
            lstring._localizationKey = str;

            return lstring;
        }
    }
}
