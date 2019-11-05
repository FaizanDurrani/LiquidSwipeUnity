using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using SVGMeshUnity;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;


public class LiquidSwipe : UIBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, ICanvasElement
{
    [SerializeField, Range(0f, 1f)] private float _minDistance;
    [SerializeField] private AnimationCurve _ease;
    [SerializeField] private WaveImage _sidePopup;
    
    private Vector2 _dragStart, _currentLocation;
    private float _totalDistance;
    private Vector2 _acceleration;

    
    private RectTransform NextSlide => transform.GetChild(1) as RectTransform;
    private RectTransform CurrentSlide => transform.GetChild(0) as RectTransform;
    private RectTransform Self => transform as RectTransform;

    private Vector2 _startingPosition;
    protected override void Start()
    {
        base.Start();

        _startingPosition = Vector2.right * Self.rect.size.x;


        foreach (RectTransform child in transform)
        {
            if (CurrentSlide == child)
            {
                child.anchoredPosition = Vector2.zero;
            }
            else
            {
                child.anchoredPosition = _startingPosition;
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _dragStart = eventData.position;
        _totalDistance = 0;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_totalDistance >= _minDistance)
        {
            NextSlide.DOAnchorPos(Vector2.zero, 0.5f, true).SetEase(_ease).OnComplete(SlideTweenCompleted);
        }
    }

    private void SlideTweenCompleted()
    {
        var nextSlide = NextSlide;
        var currentSlide = CurrentSlide;

        currentSlide.SetAsLastSibling();
        currentSlide.anchoredPosition = _startingPosition;

        nextSlide.SetAsFirstSibling();
        nextSlide.anchoredPosition = Vector2.zero;
    }

    public void OnDrag(PointerEventData eventData)
    {
        _totalDistance = (_dragStart - eventData.position).x / Screen.width;
    }

    public void Rebuild(CanvasUpdate executing)
    {
    }

    public void LayoutComplete()
    {
    }

    public void GraphicUpdateComplete()
    {
    }
}