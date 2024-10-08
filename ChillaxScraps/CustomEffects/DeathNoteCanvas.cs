using System;
using UnityEngine;
using UnityEngine.UI;

namespace ChillaxScraps.CustomEffects
{
    internal class DeathNoteCanvas : MonoBehaviour
    {
        [SerializeField] private Button closeButton;
        [SerializeField] private GameObject namesPrefab;
        [SerializeField] private Transform contentContainer;

        private DeathNote _deathNote;
        public Action onExit;

        public void Initialize(DeathNote deathNote)
        {
            foreach (Transform t in contentContainer)
            {
                Destroy(t.gameObject);
            }

            closeButton.onClick.AddListener(Close);
            _deathNote = deathNote;
            onExit += OnExit;

            foreach (var controller in deathNote.playerList)
            {
                DeathNoteName deathNoteName = Instantiate(namesPrefab, contentContainer).GetComponent<DeathNoteName>();
                deathNoteName.Initialize(controller.gameObject, _deathNote, this);
            }

            foreach (var enemy in deathNote.enemyList)
            {
                DeathNoteName deathNoteName = Instantiate(namesPrefab, contentContainer).GetComponent<DeathNoteName>();
                deathNoteName.Initialize(enemy.gameObject, _deathNote, this);
            }
        }

        private void OnExit()
        {
            Destroy(gameObject);
        }

        public void Close()
        {
            onExit?.Invoke();
        }
    }
}
