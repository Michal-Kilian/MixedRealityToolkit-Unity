using MixedReality.Toolkit.UX;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(PressableButton))]
public class PressableButtonInteractionLog : MonoBehaviour
{
    [SerializeField] private InteractionType type;

    private PressableButton button;

    private void Awake()
    {
        button = GetComponent<PressableButton>();
        button.selectEntered.AddListener(OnSelect);
    }

    private void OnSelect(SelectEnterEventArgs _)
    {
        ExperimentManager.Instance.LogInteraction(type);
    }
}
