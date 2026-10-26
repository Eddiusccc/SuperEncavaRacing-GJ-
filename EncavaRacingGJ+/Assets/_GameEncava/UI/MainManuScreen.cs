using System;
using Framework.UI;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuScreen : UIScreen
{
    [SerializeField] SaveController saveController;
    [SerializeField] Button deleteProgressButton;
    [SerializeField] Image confirmationPanel;
    [SerializeField] Button confirmationButton;
    [Tooltip("Un pobre gallo le tendría miedo a cancelar el borrado de partida")]
    [SerializeField] Button cancelButton;


    protected override void Awake()
    {
        base.Awake();
        deleteProgressButton.onClick.AddListener(OnDeleteProgressClick);
        confirmationButton.onClick.AddListener(OnConfirmationDelete);
        cancelButton.onClick.AddListener(OnCancelDelete);
    }

    void OnDeleteProgressClick()
    {
        // Mostrar confirmacion
        confirmationPanel.gameObject.SetActive(true);
    }

    public void OnConfirmationDelete()
    {
        saveController.ClearSave();
    }

    public void OnCancelDelete()
    {
        confirmationPanel.gameObject.SetActive(false);
    }
}