/// Credit BinaryX
/// Sourced from - http://forum.unity3d.com/threads/scripts-useful-4-6-scripts-collection.264161/page-2#post-1945602
/// Updated by ddreaper - removed dependency on a custom ScrollRect script. Now implements drag interfaces and standard Scroll Rect.

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
[AddComponentMenu("UI/Extensions/Horizontal Scroll Snap")]
public class HorizontalScrollSnap : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    private RectTransform _screensContainerRect;

    public int _screens = 1;
    public int _startingScreen = 1;

    private ScrollRect _scroll_rect;
    private Vector3 _lerp_target;
    public bool _lerp;

    public int _containerSize;

    public Boolean UseFastSwipe = true;
    public int FastSwipeThreshold = 100;

    private bool _startDrag = true;

    public int curScreen = -1;
    private bool initialized = false;

    public int _currentScreen
    {
        set
        {
            if (curScreen != value)
            {
                curScreen = value;
                if (OnIndexChanged != null) OnIndexChanged(curScreen);
            }
        }
        get
        {
            return curScreen;
        }
    }

    public delegate void IndexChanged(int index);

    public IndexChanged OnIndexChanged;
    public IndexChanged OnSnapStartEvent;
    public IndexChanged OnSnapEndEvent;
    public IndexChanged OnDragStartEvent;

    public float IndexThreshhold = 1f;
    public float SnapThreshhold = 0.05f;

    public bool UseCanvasSize = true;
    public Canvas Canvas;
    public float ImageWidth = 0f;

    public bool AutoStart = true;

    [Header("Screen Change Animation")]
    [ReadOnly]
    public bool Animating = false;

    public AnimationCurve AnimationCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0, 0), new Keyframe(1, 1) });
    public int StartScreen = 0;
    public int EndScreen = 1;

    public SimpleLibrary.Timer AnimationTimer = new SimpleLibrary.Timer(2f);

    private Vector3 AnimationoStartPosition;

    public void StartAnimation()
    {
        AnimationTimer.Reset();
        Animating = true;

        AnimationoStartPosition = ClosestPosition(StartScreen);
        SetCurrentScreen(EndScreen);
        _scroll_rect.enabled = false;
    }

    private void Start()
    {
        if (AutoStart) Init();
    }

    public void Init()
    {
        initialized = true;
        _scroll_rect = gameObject.GetComponent<ScrollRect>();
        _screensContainerRect = _scroll_rect.content;
        DistributePages();

        _screens = _screensContainerRect.childCount;

        _lerp = false;

        if (UseCanvasSize)
        {
            ImageWidth = Canvas.pixelRect.width / Canvas.scaleFactor;
        }

        _scroll_rect.horizontalNormalizedPosition = (float)(_startingScreen - 1) / (float)(_screens - 1);

        _containerSize = (int)_screensContainerRect.offsetMax.x;
        //_containerSize = (int)_screensContainer.gameObject.GetComponent<RectTransform>().sizeDelta.x;
        _containerSize = (int)(ImageWidth * _screens);
    }

    public float LerpSpeed = 7.5f;

    private void Update()
    {
        if (Animating)
        {
            AnimationTimer.Update();

            _screensContainerRect.anchoredPosition3D = Vector3.Lerp(AnimationoStartPosition, _lerp_target, AnimationCurve.Evaluate(AnimationTimer.Procentage));

            if ((_screensContainerRect.anchoredPosition3D - _lerp_target).magnitude < IndexThreshhold)
            {
                _currentScreen = CurrentScreen(_screensContainerRect.anchoredPosition);
            }

            if (AnimationTimer.Finished)
            {
                Animating = false;
                _screensContainerRect.anchoredPosition3D = _lerp_target;
                _currentScreen = CurrentScreen(_screensContainerRect.anchoredPosition);
                if (OnSnapEndEvent != null) OnSnapEndEvent(_currentScreen);
                _lerp = false;
                _scroll_rect.enabled = true;
            }
            return;
        }
        if (_lerp)
        {
            _scroll_rect.velocity -= Vector2.ClampMagnitude(_scroll_rect.velocity * (Time.deltaTime / 0.016f) * 0.5f, _scroll_rect.velocity.magnitude);
            _screensContainerRect.anchoredPosition3D = Vector3.Lerp(_screensContainerRect.anchoredPosition3D, _lerp_target, LerpSpeed * Time.deltaTime);

            float distance = Vector3.Distance(_screensContainerRect.anchoredPosition3D, _lerp_target);

            if (distance < IndexThreshhold)
            {
                _currentScreen = CurrentScreen(_screensContainerRect.anchoredPosition);

                if (distance < SnapThreshhold)
                {
                    _screensContainerRect.anchoredPosition3D = _lerp_target;
                    _currentScreen = CurrentScreen(_screensContainerRect.anchoredPosition);
                    _lerp = false;
                    if (OnSnapEndEvent != null) OnSnapEndEvent(_currentScreen);
                }
            }
        }
    }

    //Function for switching screens with buttons
    private void NextScreen(Vector2 pos)
    {
        //Debug.Log("Next");
        if (CurrentScreen(pos) < _screens - 1)
        {
            SetCurrentScreen(CurrentScreen(pos) + 1);
        }
        else
        {
            SetCurrentScreen(CurrentScreen(pos));
        }
    }

    //Function for switching screens with buttons
    private void PreviousScreen(Vector2 pos)
    {
        //Debug.Log("Prev");
        if (CurrentScreen(pos) > 0)
        {
            SetCurrentScreen(CurrentScreen(pos) - 1);
        }
        else
        {
            SetCurrentScreen(CurrentScreen(pos));
        }
    }

    public void Swipe(string direction)
    {
        if (!initialized) Init();
        if (direction == "right")
        {
            NextScreen(_screensContainerRect.anchoredPosition);
        }
        else if (direction == "left")
        {
            PreviousScreen(_screensContainerRect.anchoredPosition);
        }
    }

    public void GotoIndex(int i)
    {
        _startingScreen = i;
        if (!initialized) Init();
        SetCurrentScreen(i, true);
    }

    private Vector3 ClosestPosition(int index)
    {
        Vector3 pos = -Vector3.right * (ImageWidth * (float)index);
        //Debug.Log(string.Format("Index:{0} = Pos:{1}", index, pos));
        return pos;
    }

    //find the closest registered point to the releasing point
    private Vector3 FindClosestFrom(Vector3 start, System.Collections.Generic.List<Vector3> positions)
    {
        return ClosestPosition(CurrentScreen(_screensContainerRect.anchoredPosition));
    }

    //returns the current screen that the is seeing
    public int CurrentScreen(Vector2 pos)
    {
        float absPoz = Math.Abs(pos.x);

        absPoz = Mathf.Clamp(absPoz, 1, _containerSize - 1);

        float calc = (absPoz / _containerSize) * _screens;

        return Mathf.RoundToInt(calc);
    }

    private void SetCurrentScreen(int index, bool instant = false)
    {
        if (index >= _screens)
            index = _screens - 1;
        if (index < 0)
            index = 0;

        if (OnDragStartEvent != null) OnDragStartEvent(_currentScreen);
        if (OnSnapStartEvent != null) OnSnapStartEvent(index);

        _lerp = true;
        _lerp_target = ClosestPosition(index);

        if (instant)
            _screensContainerRect.anchoredPosition3D = _lerp_target;

        //Should force "OnIndexChanged" event
        curScreen = -1;
    }

    //used for changing between screen resolutions
    private void DistributePages()
    {
        int _offset = 0;
        int _step = Screen.width;
        int _dimension = 0;

        int currentXPosition = 0;

        for (int i = 0; i < _screensContainerRect.childCount; i++)
        {
            RectTransform child = _screensContainerRect.GetChild(i).gameObject.GetComponent<RectTransform>();
            currentXPosition = _offset + i * _step;
            child.anchoredPosition = new Vector2(currentXPosition, 0f);
            child.sizeDelta = new Vector2(gameObject.GetComponent<RectTransform>().sizeDelta.x, gameObject.GetComponent<RectTransform>().sizeDelta.y);
        }

        _dimension = currentXPosition + _offset * -1;

        _screensContainerRect.offsetMax = new Vector2(_dimension, 0f);
    }

    #region Interfaces

    private int startDragScreen = -1;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (Animating)
            return;
        if (_screensContainerRect == null)
            return;

        _startDrag = false;
        startDragScreen = CurrentScreen(_screensContainerRect.anchoredPosition);
        if (OnDragStartEvent != null) OnDragStartEvent(startDragScreen);
    }

    [ReadOnly]
    public Vector2 LastVelocity;

    public void OnEndDrag(PointerEventData eventData)
    {
        if (Animating)
            return;
        if (_screensContainerRect == null)
            return;

        _startDrag = true;
        if (_scroll_rect.horizontal)
        {
            if (UseFastSwipe)
            {
                LastVelocity = _scroll_rect.velocity;
                if (LastVelocity.x < -FastSwipeThreshold)
                {
                    if (CurrentScreen(_screensContainerRect.anchoredPosition) != startDragScreen)
                        SetCurrentScreen(CurrentScreen(_screensContainerRect.anchoredPosition));
                    else
                        NextScreen(_screensContainerRect.anchoredPosition);
                }
                else if (LastVelocity.x > FastSwipeThreshold)
                {
                    if (CurrentScreen(_screensContainerRect.anchoredPosition) != startDragScreen)
                        SetCurrentScreen(CurrentScreen(_screensContainerRect.anchoredPosition));
                    else
                        PreviousScreen(_screensContainerRect.anchoredPosition);
                }
                else
                {
                    SetCurrentScreen(CurrentScreen(_screensContainerRect.anchoredPosition));
                }
            }
            else
            {
                SetCurrentScreen(CurrentScreen(_screensContainerRect.anchoredPosition));
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (Animating)
            return;
        if (_screensContainerRect == null)
            return;

        _lerp = false;
        if (_startDrag)
        {
            OnBeginDrag(eventData);
        }
    }

    #endregion Interfaces
}