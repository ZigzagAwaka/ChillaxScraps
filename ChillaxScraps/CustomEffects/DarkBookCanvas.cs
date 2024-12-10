using System;
using UnityEngine;
using UnityEngine.UI;

namespace ChillaxScraps.CustomEffects
{
    internal class DarkBookCanvas : MonoBehaviour
    {
        [SerializeField] private Button closeButton;
        [SerializeField] private GameObject namesPrefab;
        [SerializeField] private Transform contentContainer;

        private DarkBook _darkBook;
        public Action onExit;

        public void Initialize(DarkBook darkBook)
        {
            foreach (Transform t in contentContainer)
            {
                Destroy(t.gameObject);
            }

            closeButton.onClick.AddListener(Close);
            _darkBook = darkBook;
            onExit += OnExit;

            if (darkBook.playerHeldBy != null)
            {
                foreach (var controller in darkBook.playerList)
                {
                    DarkBookName darkBookName = Instantiate(namesPrefab, contentContainer).GetComponent<DarkBookName>();
                    darkBookName.Initialize(controller.gameObject, _darkBook, this);
                }
            }

            if (darkBook.enemyList != null)
            {
                foreach (var enemy in darkBook.enemyList)
                {
                    DarkBookName darkBookName = Instantiate(namesPrefab, contentContainer).GetComponent<DarkBookName>();
                    darkBookName.Initialize(enemy.gameObject, _darkBook, this);
                }
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
