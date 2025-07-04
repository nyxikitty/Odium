namespace Odium.Components
{
    public class LoudMic
    {
        public static void Activated(bool state)
        {
            if (state)
            {
                USpeaker.field_Internal_Static_Single_1 = float.MaxValue;
            }
            else 
            {
                USpeaker.field_Internal_Static_Single_1 = 1f;
            }
        }
    }
}