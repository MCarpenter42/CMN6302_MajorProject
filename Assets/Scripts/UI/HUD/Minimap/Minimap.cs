using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum MinimapState { Hidden = -1, Corner, Fullview }
public class Minimap : UIObject
{
    private MinimapState currentState = (MinimapState)(-2);
    private struct StateSettings
    {
        public Rect viewportRect;
        public float viewportSize;
        public float markerSize;

        public StateSettings(Rect viewportRect, float viewportSize, float markerSize)
        {
            this.viewportRect = viewportRect;
            this.viewportSize = viewportSize;
            this.markerSize = markerSize;
        }

        public void Apply(Minimap minimap)
        {
            minimap.cam.rect = viewportRect;
            minimap.cam.orthographicSize = viewportSize;
            minimap.scaleMarker.localScale = Vector3.one * markerSize;
        }
    }

    [Header("World Objects")]
    public Transform scaleMarker;
    public Camera cam;

    [Header("Corner View")]
    [SerializeField] UIObject cornerFrame;
    [SerializeField] Rect crnrRect = new Rect() { width = 0.15f, height = 0.15f, x = 0.00f, y = 0.85f };
    [SerializeField] float crnrViewport = 4;
    [SerializeField] float crnrMarkerSize = 6.0f;
    StateSettings corner { get { return new StateSettings(crnrRect, crnrViewport, crnrMarkerSize); } }

    [Header("Full View")]
    [SerializeField] UIObject fullviewFrame;
    [SerializeField] Rect fullRect = new Rect() { width = 0.70f, height = 0.70f, x = 0.15f, y = 0.15f };
    [SerializeField] float fullViewport = 12;
    [SerializeField] float fullMarkerSize = 10.0f;
    StateSettings fullview { get { return new StateSettings(fullRect, fullViewport, fullMarkerSize); } }

    public bool expanded { get; private set; }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public void SetState(MinimapState state)
    {
        if (state != currentState)
        {
            currentState = state;
            switch (state)
            {
                default:
                case MinimapState.Hidden:
                    cornerFrame.Show(false);
                    fullviewFrame.Show(false);
                    cam.enabled = false;
                    break;

                case MinimapState.Corner:
                    cornerFrame.gameObject.SetActive(true);
                    fullviewFrame.gameObject.SetActive(false);
                    cam.enabled = true;
                    corner.Apply(this);
                    break;

                case MinimapState.Fullview:
                    cornerFrame.gameObject.SetActive(false);
                    fullviewFrame.gameObject.SetActive(true);
                    cam.enabled = true;
                    fullview.Apply(this);
                    break;
            }
        }
    }

    public override void Toggle()
    {
        switch (currentState)
        {
            default:
            case MinimapState.Hidden:
                break;

            case MinimapState.Corner:
                cornerFrame.gameObject.SetActive(false);
                fullviewFrame.gameObject.SetActive(true);
                cam.gameObject.SetActive(true);
                currentState = MinimapState.Fullview;
                fullview.Apply(this);
                break;

            case MinimapState.Fullview:
                cornerFrame.gameObject.SetActive(true);
                fullviewFrame.gameObject.SetActive(false);
                cam.gameObject.SetActive(true);
                currentState = MinimapState.Corner;
                corner.Apply(this);
                break;
        }
    }

    protected override void OnShow() => SetState(MinimapState.Corner);
    protected override void OnHide() => SetState(MinimapState.Hidden);
}
