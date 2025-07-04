using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Odium.Components
{
    internal class ResourceWrapper
    {
        public enum Icons
        {
            Heart,
            World,
            Logo
        }


        public static Sprite GetResource(Icons icon)
        {
            switch (icon)
            {
                case Icons.Heart:
                    {
                        string base64Image = @"iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAACXBIWXMAAAsTAAALEwEAmpwYAAABAElEQVR4nO3UPUoDQRxA8b2CjWghImhhYxkstBIVsdUj6JVUiCktPI2NjWAhfiAIFopREshPFiYwyGSzGydg4YOBhZ19b/kzTFH80xQs4Qz36OMBbSxHe1ZwHt71w97T8ttx8l28S9PDEY7Dc4o37FT9+Sj5kEFYVZSRxVSgHEsuTlKBco65uEsFvjIGPlOB24yBm1SgkzHQTgXWM8kHaI06qpcZAhdJeQjM4ukX8mfMjQyEyCa6E8i72KiUR5Hthse2h/1a8ihyWHHn/JQfNJJHka0x99MH9iaSR5EWXhLy19ozrxFZLe+XSP6ItSzyIZjHFa6xUEwDzJRrKvI/yzdEgj3eN2QLxgAAAABJRU5ErkJggg==";
                        var sprite = SpriteUtil.Create(SpriteUtil.CreateTextureFromBase64(base64Image), new UnityEngine.Rect(0, 0, 24, 24), new UnityEngine.Vector2(0, 0));
                        return sprite;
                    }
                case Icons.World:
                    {
                        string base64Image = @"iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAaJSURBVHhe3ZtZ6G1THMdvEh7Mw4N5KJ6QrgcpLgrFi4gHIkNkSCJ5kynkiav7SCgiETIkEjI+CBcPRMmUTDdDkUh/38++/3X6nXW+a5+1z97n/P/3futzzj5rr732+a2111q/New1S0tLWzNrxXqxUURtEhvEGndRYl8TtiWwp7hGfCqmaYNLIHGsCVvN7CVuEn+KWm1yCQGl/2EWNhQHiRMDLk5XMPyvZTrJJQaXCHSYcOdr2EmcJqiDr4tp+ko8I+4SRwqXZg5P6WdiZrlEdxA/CvSecHHauFw8K/rqG0FmHCrye+wiXhQlvSl+23zYrjzhEwRGRz0qDhR53Mj24m7xg5iHXhMnCe51nfhVOD0hDhDEIxOmaWMyYHdxgfhOOH0ijhcpfuRiMS/Dc32x/J2Lun+m2E7wn8iEGq3PjdlVPCj+CVwr8niwm3hbrLSeEzuL+N+eFzVaGy+KUOIYT2a48+vEH6JGP4mnxK3iFJFa/0OWv08WNLr3iLz6TdMtIv9vx4ka4ScUHSH+FBnAE5GfO1/U6HFxhsivn8bB4mpRqo5ReHix9Dn+WtQIZ6nVE3Slf5GYphvFtoE8jS7QKOdubK6PRMoEqkONcJbwGFszIHdSppX8q2JvEa8ZisvEv8s4kUlUhxrRYOI4NWnnN4rEx58636bbRLx2HuwjPhB9RQbgNjfp5jdx0NqXGrzfxTnCXTcPyISHRB+NSh9i4iVKXR3G17qsQzNrJuBuj6U19sOAk1PSIks+ss0y74iumhjhjv3IwL0teXiLqPMlUgZcKrqIscNEehMBAXx7J1p7F39RzGI8YwcGUBPpTQQESqU/r66ulsNF1UgviAGUS6uYAQxpncZa0BWAOtzVeIbmLq0GGyhK4/lR/7kC7Ci+FF1EZjEx49JrcIFc4HS/cPEXAcYzRddVdJcuvREukGksJ8bbLv4imMV4NNHv57hA5vByMaR1cRdBX8+PYbdLt8EFuglMxvMu7rzpazw6W7i0G1ygE5MZLu48GcJ4xPDcpd/gAp2YyXFx58VQxiOqtLtHQx5wlHAaagGjBuYgZ9HD4t3Nh2OiSrv7NOQBGOq0qAyomXFywvA9BJmQa5AMaG1JB2JW4xHGk8bNza9xbRFPwKzG4+kdI1I6vZ8ApsGcmCXO4w5Fn5Jnqi6m9ZLI9bSIccZwgU7M27u4feljvPtPbk/AHSKPN8IF4j7mYtHCxe3D0MaD05XCxW1wgSxR55pllbiNPsY/IFyaLJ46sbaQ4jCapasfXTc6CLAk7cSKjYvfldr5e6e20R17fpxiHBrMc2NYPJlgpteJ5SoXvwt9PLw248EtieU9wL3ikRgWT0bYnJCLtToXt5Z5Gn+WcLpBpDiUfhLTak14OplTqgaxPnVhnsbDy8Ip7XRjEvVnApbFJGlTFfKEEuwIcWINzsUvwUzsvI0vLds9KTh/qnD/4TGxLk8s8pZwul64+DkY/7GYVTXGw7fCKfdeWchJYqG3CY8RctgSU9K0qfFFGe98f0Thufh4hSz1jcLiSYdrDNH7opQJR4iuU9dRtcaXGj5UGrvwFIxKH+LJHBYT2uT+6NGij/ElJyeHrprFWafSth5ImzRHYfFkhEe4tBUtKmbCeQT0UO14o814umq3raeIDRRtmxBzvSGYM+yjWuN57EvGo87dtAtk+SmX23I+hKgu+ZC2RKnBS2Kfo7uuFRf4uYhKreY89gXGyYwSZFCpq0vKJz73y34XyQNY/Iz6W9BwpPNkwhBPQprDi/fO4XEveXhRbtaXxd08zBJ/MFTMt5u7ZWW3ctRFTFuVjGdIy6iudq+fe+wp/WqPNf5Ie+6TXhHxPE/CEFWA0icTqNN8M41V83ZHFK19qcGj9P8TVdUgHbBpMH/TIvaX7NP7RawG0c+3dXXfC3SfcOfHSAdsG81FOJnAVvXVINzbkoeXYKwfdaFw8Uakg66P4CLFqG6a4fuL0jZZhvbFJ4YPXi0bWvTveIlugrVGzOQwmTHtzTUMO11Mu88Lwr7vwEffVj0X22vithRWlViiZpWWe2FchEnYO8VVYlpJt3GFyMX7ga1p8jGUh8fYobgba0HETMD4qa/+8TGEGDvYfXgrAE8Vul2482Pw0UfUvam5vGDSWMadm4CPrsJZGttzvwopzQhNwAd1pVY4Sxi+kvsFayi94TYBH6UVlSj8BJyl5jWTLYBp7zmOSAdkQv4k0DvQbeEnjF209bC05n+In4qn4bYaCgAAAABJRU5ErkJggg==";
                        var sprite = SpriteUtil.Create(SpriteUtil.CreateTextureFromBase64(base64Image), new UnityEngine.Rect(0, 0, 64, 64), new UnityEngine.Vector2(0, 0));
                        return sprite;
                    }
                case Icons.Logo:
                    {
                        string base64Image = @"iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAMAAADXqc3KAAABblBMVEVHcExYNZNKLnI/H2c+HWRNNHM4GFtXNo5VM4tHJHVBH2s/IGJBIGk0E1Q5GGA4GVpTMoZVMo1YN5BTMYdUMYpHJnQ0E1c5GV9BHmszElU8H1w2FVo1FFdZN5VYNpJUM4pVNYpZN5NaOJFPLYJWOIpKKHo2F1U4F11NK308G2Q+HmI5GV47GWJPL4BNKn1OL31bOpNaOJVSMIg0E1hMKX0/HWlDIG43FlxBH2tPLINGI3JKJ3pTMYk7GWE8GmQ4F19IJXZZNpBWM45OKoA9G2d0WpVIJnBQLoZXOX5aOoutncVMLXFVNIXJv9VuUpX08vZePpBoS5KVgbKqmsHNw9molsNDIWxfQoGCaqHh2+eKdKanlrzv7PLY0eH49/q+sc5YNoufjbp2W52lk8A+HmJQL3mZh66Gb597YaFRMH+PeLO5q8u3qMra0+Tz8PVUNnSzpcJOLX3m4uzm4etlRZjHvNZbOpH////VzeDRyN2YU6maAAAAM3RSTlMA/gdXRANlJrjywhRfg/kjMOBXaft7y6/p9wvw3OvWfBijT+sO3xd0odwzcvE30Bs65NUihQEpAAABsUlEQVQoz12S52OCMBDFQXFra9XubfdeQMWiAhZEHHXi3t3D7vXf94Ld+Zbfe7m8uwTDPpeNMC6t2VfHsL+r3zE0HgoF+kasRtsvrDNO7O3tgxA8pHxW4puPOUZ/OE2P2D+5wQE4dnuMuFQ48fst+p6gB//xaUJGfrFW5DhuwYn44hAcCJSPzijKJ2WrIscdkMsGEIxa/UIpUfCpYikhAyfnNiDRsHavdFc/o6VKPUMCDzPT0MGElocqdk+lk8ZDUuMRsw4jxns55VpOTj824hqP7M5g+j7gMZWSKtXzTLdMkvEoE2EHTCAEgmpM9dFi9SpXSpPheJRlkUCsB6kYaljOdes35SjDsClWmJrB+lcOVfDT/ovr1yMl24Y6TV6Ay3XDlKrSfu4gmei+p1OojsDjEBezq2BPtlqN2k0u3+m00oLAT6KZ2KwgiJXs85uSVRQlnxRwfBaNBCMsUOj+on3VbjZTKZ7H8SlTb+yDaG5kPHOducyfg3/S+/WAgxbUb7TYyV8+4fiO6/sJDfrNMAM5+RcoZN7+/Rm23OY5j8eDD8y65v/9k3mny+32mnRf+w/602cM/E5NYwAAAABJRU5ErkJggg==";
                        var sprite = SpriteUtil.Create(SpriteUtil.CreateTextureFromBase64(base64Image), new UnityEngine.Rect(0, 0, 64, 64), new UnityEngine.Vector2(0, 0));
                        return sprite;
                    }
            }

            return null;
        }
    }
}
