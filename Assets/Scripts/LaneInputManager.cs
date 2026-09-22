using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class LaneInputManager : MonoBehaviour
{
    public static LaneInputManager instance;
 
    [Tooltip("Drag GameObject button setiap lane ke sini (urutan kiri ke kanan).")]
    public GameObject[] laneButtons;
 
    [Tooltip("Pelebaran area sentuh ke semua sisi (pixel layar). 0 = pas ukuran sprite.")]
    public float touchPadding = 20f;
 
    // Area layar yang dihitung otomatis dari laneButtons
    private Rect[] _laneAreas;
 
    // State per lane
    private bool[] _isDown;
    private bool[] _justDown;
    private bool[] _justUp;
    private int[]  _touchId;
 
    void Awake()
    {
        instance = this;
 
        int count = laneButtons != null ? laneButtons.Length : 0;
        _laneAreas = new Rect[count];
        _isDown    = new bool[count];
        _justDown  = new bool[count];
        _justUp    = new bool[count];
        _touchId   = new int[count];
 
        for (int i = 0; i < count; i++)
            _touchId[i] = -1;
    }
 
    void Start()
    {
        // Hitung Rect setelah semua object sudah ter-layout
        RebuildAreas();
    }
 
    // Hitung area sentuh dari setiap button 
    void RebuildAreas()
    {
        Camera cam = Camera.main;
 
        for (int i = 0; i < laneButtons.Length; i++)
        {
            if (laneButtons[i] == null) continue;
 
            Rect r = GetScreenRect(laneButtons[i], cam);
 
            // Tambah padding ke semua sisi
            r.x      -= touchPadding;
            r.y      -= touchPadding;
            r.width  += touchPadding * 2f;
            r.height += touchPadding * 2f;
 
            _laneAreas[i] = r;
        }
    }
 
    Rect GetScreenRect(GameObject go, Camera cam)
    {
        // Coba RectTransform (UI Canvas)
        RectTransform rt = go.GetComponent<RectTransform>();
        if (rt != null)
        {
            Vector3[] corners = new Vector3[4];
            rt.GetWorldCorners(corners);
 
            // Jika Canvas pakai Screen Space Overlay, corners sudah dalam pixel layar
            // Jika Canvas pakai Camera/World Space, perlu konversi
            Canvas canvas = rt.GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                for (int c = 0; c < 4; c++)
                    corners[c] = cam.WorldToScreenPoint(corners[c]);
            }
 
            float minX = corners[0].x;
            float minY = corners[0].y;
            float maxX = corners[2].x;
            float maxY = corners[2].y;
 
            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }
 
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Bounds bounds = sr.bounds;
 
            Vector3 bl = cam.WorldToScreenPoint(new Vector3(bounds.min.x, bounds.min.y, bounds.center.z));
            Vector3 tr = cam.WorldToScreenPoint(new Vector3(bounds.max.x, bounds.max.y, bounds.center.z));
 
            return new Rect(bl.x, bl.y, tr.x - bl.x, tr.y - bl.y);
        }
 
        Vector3 screenPos = cam.WorldToScreenPoint(go.transform.position);
        Debug.LogWarning($"[LaneInputManager] {go.name} tidak punya SpriteRenderer/RectTransform. " +
                         "Menggunakan area default 100×100 px.");
        return new Rect(screenPos.x - 50f, screenPos.y - 50f, 100f, 100f);
    }
 
    void Update()
    {
        int count = _laneAreas.Length;
 
        for (int i = 0; i < count; i++)
        {
            _justDown[i] = false;
            _justUp[i]   = false;
        }

        // Blokir semua input lane jika kondisi tertentu aktif
        if (GameManager.instance != null && GameManager.instance.IsInputBlocked())
            return;
 
        HashSet<int> activeTouchIds = new HashSet<int>();
 
        for (int t = 0; t < Input.touchCount; t++)
        {
            Touch touch = Input.GetTouch(t);
            activeTouchIds.Add(touch.fingerId);
 
            Vector2 pos = touch.position;
 
            if (touch.phase == TouchPhase.Began)
            {
                for (int i = 0; i < count; i++)
                {
                    if (_touchId[i] == -1 && _laneAreas[i].Contains(pos))
                    {
                        _touchId[i]  = touch.fingerId;
                        _isDown[i]   = true;
                        _justDown[i] = true;
                        break;
                    }
                }
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                for (int i = 0; i < count; i++)
                {
                    if (_touchId[i] == touch.fingerId)
                    {
                        _touchId[i] = -1;
                        _isDown[i]  = false;
                        _justUp[i]  = true;
                    }
                }
            }
        }
 
        for (int i = 0; i < count; i++)
        {
            if (_touchId[i] != -1 && !activeTouchIds.Contains(_touchId[i]))
            {
                _touchId[i] = -1;
                _isDown[i]  = false;
                _justUp[i]  = true;
            }
        }
 
#if UNITY_EDITOR
        KeyCode[] editorKeys = { KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F };
        for (int i = 0; i < count && i < editorKeys.Length; i++)
        {
            if (Input.GetKeyDown(editorKeys[i])) { _isDown[i] = true;  _justDown[i] = true; }
            if (Input.GetKeyUp(editorKeys[i]))   { _isDown[i] = false; _justUp[i]   = true; }
        }
#endif
    }
 
    public bool GetLaneDown(int laneIndex)
    {
        if (laneIndex < 0 || laneIndex >= _justDown.Length) return false;
        return _justDown[laneIndex];
    }
 
    public bool GetLane(int laneIndex)
    {
        if (laneIndex < 0 || laneIndex >= _isDown.Length) return false;
        return _isDown[laneIndex];
    }
 
    public bool GetLaneUp(int laneIndex)
    {
        if (laneIndex < 0 || laneIndex >= _justUp.Length) return false;
        return _justUp[laneIndex];
    }
 
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (_laneAreas == null) return;
 
        Gizmos.color = new Color(0f, 1f, 0.5f, 0.35f);
        Camera cam = Camera.main;
        if (cam == null) return;
 
        foreach (Rect r in _laneAreas)
        {
            Vector3 bl = cam.ScreenToWorldPoint(new Vector3(r.xMin, r.yMin, cam.nearClipPlane + 0.1f));
            Vector3 br = cam.ScreenToWorldPoint(new Vector3(r.xMax, r.yMin, cam.nearClipPlane + 0.1f));
            Vector3 tl = cam.ScreenToWorldPoint(new Vector3(r.xMin, r.yMax, cam.nearClipPlane + 0.1f));
            Vector3 tr = cam.ScreenToWorldPoint(new Vector3(r.xMax, r.yMax, cam.nearClipPlane + 0.1f));
 
            Gizmos.DrawLine(bl, br);
            Gizmos.DrawLine(br, tr);
            Gizmos.DrawLine(tr, tl);
            Gizmos.DrawLine(tl, bl);
        }
    }
#endif
}