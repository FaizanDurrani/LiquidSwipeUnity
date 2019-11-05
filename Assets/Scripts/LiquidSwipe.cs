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
    [SerializeField] private WaveImage _waveMask;
    [SerializeField] private Transform _slides;

    private Vector2 _dragStart, _currentLocation;
    private float _totalDistance;
    private Vector2 _acceleration;
    private float _centerY;
    private bool _dragging;


    private RectTransform NextSlide =>
        _slides.childCount > 0 ? _slides.GetChild(_slides.childCount - 1) as RectTransform : null;

    private RectTransform CurrentSlide =>
        _waveMask.transform.childCount > 0 ? _waveMask.transform.GetChild(0) as RectTransform : null;

    private RectTransform Self => transform as RectTransform;

    private Vector2 _startingPosition;

    private float _initWaveMaskHorRadius, _initWaveMaskVertRadius, _initWaveMaskCenterY, _initWaveMaskSideWidth;

    protected override void Start()
    {
        base.Start();

        _centerY = _initWaveMaskCenterY = _waveMask.WaveCenterY;
        _initWaveMaskVertRadius = _waveMask.WaveVertRadius;
        _initWaveMaskHorRadius = _waveMask.WaveHorRadius;
        _initWaveMaskSideWidth = _waveMask.SideWidth;
        _startingPosition = Vector2.right * Self.rect.size.x;

        foreach (RectTransform slide in _slides)
        {
            slide.anchoredPosition = Vector2.zero;
            ;
        }
        
        if (CurrentSlide == null)
        {
            ChangeActiveSlide(NextSlide);
        }
    }

    private void Update()
    {
        if (!_dragging) return;

        _waveMask.WaveCenterY = Mathf.Lerp(_waveMask.WaveCenterY, _centerY, Time.deltaTime * 6);
    }

    private void ChangeActiveSlide(RectTransform slide)
    {
        if (CurrentSlide != null)
        {
            CurrentSlide.SetParent(_slides);
        }

        slide.SetParent(_waveMask.transform);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _dragging = true;
        _dragStart = eventData.position;
        _totalDistance = 0;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _dragging = false;
        if (_totalDistance >= _minDistance)
        {
            //NextSlide.DOAnchorPos(Vector2.zero, 0.5f, true).SetEase(_ease).OnComplete(SlideTweenCompleted);
            var sequence = DOTween.Sequence();
            sequence.Insert(0,
                DOTween.To(() => _waveMask.WaveHorRadius, value => _waveMask.WaveHorRadius = value, .2f, 0.5f));
            sequence.Insert(0,
                DOTween.To(() => _waveMask.WaveVertRadius, value => _waveMask.WaveVertRadius = value, 2,
                    0.5f));
            sequence.Insert(0,
                DOTween.To(() => _waveMask.SideWidth, value => _waveMask.SideWidth = value, 1, 0.5f));
            sequence.OnComplete(() =>
            {
                ChangeActiveSlide(NextSlide);
                _waveMask.WaveCenterY = _initWaveMaskCenterY;
                _waveMask.SideWidth = _waveMask.WaveHorRadius = _waveMask.WaveVertRadius = 0;

                var resetSequence = DOTween.Sequence();
                resetSequence.Insert(0, 
                    DOTween.To(() => _waveMask.WaveHorRadius, value => _waveMask.WaveHorRadius = value, _initWaveMaskHorRadius, 0.5f));
                resetSequence.Insert(0, 
                    DOTween.To(() => _waveMask.WaveVertRadius, value => _waveMask.WaveVertRadius = value, _initWaveMaskVertRadius, 0.5f));
                resetSequence.Insert(0,
                    DOTween.To(() => _waveMask.SideWidth, value => _waveMask.SideWidth = value, _initWaveMaskSideWidth, 0.5f));

            });
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
        var distance = _dragStart - eventData.position;
        _totalDistance = distance.x / Screen.width;

        _centerY = 1-eventData.position.y/Screen.height;
        
        _waveMask.WaveHorRadius = _initWaveMaskHorRadius + _totalDistance;
        _waveMask.WaveVertRadius = _initWaveMaskVertRadius + _totalDistance * .5f;
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