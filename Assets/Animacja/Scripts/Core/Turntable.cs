using UnityEngine;

namespace Animacja
{
    /// <summary>Obrotowa podstawa – pokazuje obiekt ze wszystkich stron.</summary>
    public class Turntable : MonoBehaviour
    {
        public float degreesPerSecond = 25f;
        public static bool Paused;

        void Update()
        {
            if (!Paused)
                transform.Rotate(0f, degreesPerSecond * Time.deltaTime, 0f, Space.World);
        }
    }
}
