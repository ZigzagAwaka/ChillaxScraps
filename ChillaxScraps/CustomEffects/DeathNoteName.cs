using GameNetcodeStuff;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChillaxScraps.CustomEffects
{
    internal class DeathNoteName : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private Button killButton;

        private GameObject _objectToKill;
        private DeathNote _deathNote;
        private DeathNoteCanvas _deathNoteCanvas;

        public void Initialize(GameObject objectToKill, DeathNote deathNote, DeathNoteCanvas deathNoteCanvas)
        {
            _deathNote = deathNote;
            _objectToKill = objectToKill;
            _deathNoteCanvas = deathNoteCanvas;

            if (objectToKill.transform.TryGetComponent(out PlayerControllerB player))
            {
                nameText.text = player.playerUsername;
            }
            else if (objectToKill.transform.TryGetComponent(out EnemyAI enemy))
            {
                nameText.text = enemy.enemyType.ToString();
            }

            killButton.onClick.AddListener(Kill);
        }

        private void Kill()
        {
            _deathNote.ActivateDeathNote(_objectToKill);
            _deathNoteCanvas.Close();
        }
    }
}
