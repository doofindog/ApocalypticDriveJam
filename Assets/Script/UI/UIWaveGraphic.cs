using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class UIWaveGraphic : Graphic
{
    [Header("Wave")]
    [SerializeField] public float amplitude = 30f;     // pixels
    [SerializeField] public float frequency = 2f;      // cycles across width
    [SerializeField] public float phase = 0f;          // radians
    [SerializeField] public int segments = 120;        // more = smoother

    [Header("Stroke")]
    [SerializeField] private float thickness = 3f;      // pixels

    [Header("Style")]
    [SerializeField] private bool dotted = false;
    [SerializeField] private float dotSpacing = 14f;    // pixels between dots (approx)
    [SerializeField] private float dotLength = 6f;      // pixels of “on” segment

    public void SetWave(float amp, float freq, float ph)
    {
        amplitude = amp;
        frequency = freq;
        phase = ph;
        SetVerticesDirty();
    }

	public void Update()
	{
		SetVerticesDirty();
	}

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Rect r = rectTransform.rect;
        float width = r.width;
        float height = r.height;
        float midY = r.yMin + height * 0.5f;

        if (segments < 2 || width <= 0f) return;

        // Convert “dotted” spacing in pixels to normalized [0..1] step.
        float spacingN = (dotSpacing <= 0f) ? 1f : (dotSpacing / width);
        float lengthN  = (dotLength  <= 0f) ? spacingN : (dotLength / width);

        Vector2 prev = Vector2.zero;
        bool hasPrev = false;

        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments; // 0..1 across width
            float x = r.xMin + t * width;

            // cycles across width: sin( t * frequency * 2π + phase )
            float y = midY + Mathf.Sin(t * frequency * Mathf.PI * 2f + phase) * amplitude;

            Vector2 p = new Vector2(x, y);

            // Dotted: only draw small “on” chunks
            if (dotted)
            {
                float mod = t % spacingN;
                bool drawThis = mod <= lengthN;

                if (!drawThis)
                {
                    hasPrev = false;
                    continue;
                }
            }

            if (hasPrev)
                AddThickLine(vh, prev, p, thickness, color);

            prev = p;
            hasPrev = true;
        }
    }

    private static void AddThickLine(VertexHelper vh, Vector2 a, Vector2 b, float thickness, Color32 col)
    {
        Vector2 dir = (b - a).normalized;
        Vector2 n = new Vector2(-dir.y, dir.x) * (thickness * 0.5f);

        UIVertex v0 = UIVertex.simpleVert; v0.color = col; v0.position = a - n;
        UIVertex v1 = UIVertex.simpleVert; v1.color = col; v1.position = a + n;
        UIVertex v2 = UIVertex.simpleVert; v2.color = col; v2.position = b + n;
        UIVertex v3 = UIVertex.simpleVert; v3.color = col; v3.position = b - n;

        int start = vh.currentVertCount;
        vh.AddVert(v0); vh.AddVert(v1); vh.AddVert(v2); vh.AddVert(v3);

        vh.AddTriangle(start + 0, start + 1, start + 2);
        vh.AddTriangle(start + 2, start + 3, start + 0);
    }
}
