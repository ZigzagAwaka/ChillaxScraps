using GameNetcodeStuff;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChillaxScraps.CustomEffects
{
    internal class DarkBookName : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private Button killButton;

        private GameObject _objectToKill;
        private DarkBook _darkBook;
        private DarkBookCanvas _darkBookCanvas;

        public void Initialize(GameObject objectToKill, DarkBook darkBook, DarkBookCanvas darkBookCanvas)
        {
            _darkBook = darkBook;
            _objectToKill = objectToKill;
            _darkBookCanvas = darkBookCanvas;

            if (objectToKill.transform.TryGetComponent(out PlayerControllerB player))
            {
                nameText.text = player.playerUsername;
            }
            else if (objectToKill.transform.TryGetComponent(out EnemyAI enemy))
            {
                nameText.text = enemy.enemyType.enemyName;
            }

            killButton.onClick.AddListener(Kill);
        }

        private void Kill()
        {
            _darkBook.ActivateDeathNote(_objectToKill);
            _darkBookCanvas.Close();
        }
    }
}
