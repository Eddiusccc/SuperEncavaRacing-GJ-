using System.Collections;
using System.Collections.Generic;
using Framework.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Framework.Dialogue
{
    [RequireComponent(typeof(CanvasGroup))]
    public class DialogueUI : MonoBehaviour
    {
        [SerializeField] TMP_Text speakerLabel;
        [SerializeField] TMP_Text bodyLabel;
        [SerializeField] Button continueButton;
        [SerializeField] Transform choicesContainer;
        [SerializeField] Button choiceButtonPrefab;
        [SerializeField] float secondsPerCharacter = 0.4f;

        CanvasGroup group;
        Coroutine typing;
        readonly List<Button> choiceButtons = new();

        void Awake()
        {
            group = GetComponent<CanvasGroup>();
            SetVisible(false);
            continueButton.onClick.AddListener(OnContinueClicked);
        }
        void OnEnable()
        {
            EventBus.Subscribe<DialogueStarted>(OnStarted);
            EventBus.Subscribe<DialogueLineShown>(OnLine);
            EventBus.Subscribe<DialogueEnded>(OnEnded);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<DialogueStarted>(OnStarted);
            EventBus.Unsubscribe<DialogueLineShown>(OnLine);
            EventBus.Unsubscribe<DialogueEnded>(OnEnded);
        }

        void OnStarted(DialogueStarted _) => SetVisible(true);
        void OnEnded(DialogueEnded _) => SetVisible(false);

        void OnLine(DialogueLineShown e)
        {
            speakerLabel.text = e.Speaker;
            ClearChoices();

            if (typing != null) StopCoroutine(typing);
            typing = StartCoroutine(Typewriter(e.Text, e.Choices));
        }

        /// <summary>
        /// Una función que se puede "pausar" y continuar más tarde.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="choices"></param>
        /// <returns></returns>
        IEnumerator Typewriter(string text, string[] choices)
        {
            continueButton.gameObject.SetActive(false);

            bodyLabel.text = text;
            bodyLabel.ForceMeshUpdate();
            int total = bodyLabel.textInfo.characterCount;

            for (int i = 0; i <= total; i++)
            {
                bodyLabel.maxVisibleCharacters = i;
                yield return new WaitForSecondsRealtime(secondsPerCharacter);
            }

            typing = null;
            ShowOptions(choices);
        }

        void ShowOptions(string[] choices)
        {
            if (choices.Length == 0)
            {
                continueButton.gameObject.SetActive(true);
                EventSystem.current.SetSelectedGameObject(continueButton.gameObject);
                return;
            }

            for (int i = 0; i < choices.Length; i++)
            {
                int captured = i; // copia local para el click
                Button b = Instantiate(choiceButtonPrefab, choicesContainer);
                b.GetComponentInChildren<TMP_Text>().text = choices[i];
                b.onClick.AddListener(() => EventBus.Publish(new DialogueChoiceSelected(captured)));
                choiceButtons.Add(b);
            }
            EventSystem.current.SetSelectedGameObject(choiceButtons[0].gameObject);
        }

        void OnContinueClicked()
        {
            EventBus.Publish(new DialogueAdvanceRequested());
        }

        void ClearChoices()
        {
            foreach (var b in choiceButtons) Destroy(b.gameObject);
            choiceButtons.Clear();
        }

        void SetVisible(bool visible)
        {
            group.alpha = visible ? 1f : 0f;
            group.interactable = visible;
            group.blocksRaycasts = visible;
        }
    }
}