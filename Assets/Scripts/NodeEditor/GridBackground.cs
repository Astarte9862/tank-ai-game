using UnityEngine;
using UnityEngine.UIElements;

public class GridBackground : VisualElement
{
    System.Func<Vector2> getPan;
    System.Func<float> getZoom;

    public GridBackground(System.Func<Vector2> panGetter,
                          System.Func<float> zoomGetter)
    {
        getPan = panGetter;
        getZoom = zoomGetter;

        style.position = Position.Absolute;
        this.StretchToParentSize();

        generateVisualContent += Draw;
        pickingMode = PickingMode.Ignore;
    }

    void Draw(MeshGenerationContext ctx)
    {
        var painter = ctx.painter2D;

        float zoom = getZoom();
        Vector2 pan = getPan();

        float smallGrid = 20 * zoom;
        float largeGrid = 100 * zoom;

        Rect rect = contentRect;

        float startX = pan.x % smallGrid;
        float startY = pan.y % smallGrid;

        painter.lineWidth = 1;

        // 小グリッド
        painter.strokeColor = new Color(1,1,1,0.05f);

        for (float x = startX; x < rect.width; x += smallGrid)
        {
            painter.BeginPath();
            painter.MoveTo(new Vector2(x,0));
            painter.LineTo(new Vector2(x,rect.height));
            painter.Stroke();
        }

        for (float y = startY; y < rect.height; y += smallGrid)
        {
            painter.BeginPath();
            painter.MoveTo(new Vector2(0,y));
            painter.LineTo(new Vector2(rect.width,y));
            painter.Stroke();
        }

        // 大グリッド
        startX = pan.x % largeGrid;
        startY = pan.y % largeGrid;

        painter.strokeColor = new Color(1,1,1,0.1f);

        for (float x = startX; x < rect.width; x += largeGrid)
        {
            painter.BeginPath();
            painter.MoveTo(new Vector2(x,0));
            painter.LineTo(new Vector2(x,rect.height));
            painter.Stroke();
        }

        for (float y = startY; y < rect.height; y += largeGrid)
        {
            painter.BeginPath();
            painter.MoveTo(new Vector2(0,y));
            painter.LineTo(new Vector2(rect.width,y));
            painter.Stroke();
        }
    }
}