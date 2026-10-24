using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelSelectEventSystemHandler : DynamicEventSystemHandler
{
    private Image _image;
    private LevelButton _levelButton;
    private LevelSelectManager _levelSelectManager;

    private bool _initialMoveComplete;

    private void Awake()
    {
        _levelSelectManager = GetComponentInParent<LevelSelectManager>();
    }

    public override void OnPointerEnter(BaseEventData eventData) { }

    public override void OnPointerExit(BaseEventData eventData) { }

    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);

        _image = eventData.selectedObject.GetComponent<Image>();
        _levelButton = eventData.selectedObject.GetComponent<LevelButton>();
        if ( _levelButton != null )
        {
            _levelSelectManager.levelHeader.SetText(_levelButton.name);

            RectTransform rectTrans = eventData.selectedObject.GetComponent<RectTransform>();

            if (_initialMoveComplete)
                _levelSelectManager.MovePlayerToButton(_levelSelectManager.playerObj, rectTrans, _levelSelectManager.worldSpaceCanvas);

            _initialMoveComplete = true;

            if (_image != null)
            {
                _image.color = Color.blue;
            }
        }
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        base.OnDeselect(eventData);

        if (_levelButton != null)
        {
            _levelSelectManager.levelHeader.SetText("");

            if (_image != null)
            {
                _image.color = _levelButton.ReturnColor;
            }
        }
    }
}
