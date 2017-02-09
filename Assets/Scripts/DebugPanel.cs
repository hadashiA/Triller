using Models;
using UnityEngine;
using UnityEngine.UI;

public class DebugPanel : MonoBehaviour
{
    [SerializeField]
    Text _directionText;

    [SerializeField]
    Text _coordText;

    [SerializeField]
    Text _positionText;

    [SerializeField]
    Text _analyzeText;

    public void DrawDebugGrid(Transform fieldTransform)
    {
        var field = Dig.Field;
        if (field == null) return;

        if (Input.GetMouseButtonDown(1))
        {
            var pos = Input.mousePosition;
            pos.z = 10f;
            var worldPos = Camera.main.ScreenToWorldPoint(pos);
            var localPos = fieldTransform.InverseTransformPoint(worldPos);
            var coord = Dig.Field.GetCoord(localPos);
            Analyze(coord);
        }

        foreach (var coord in field)
        {
            if (field.IsBlock(coord))
            {
                var block = field.GetBlock(coord);
                var color = Color.white;
                switch (block.Color)
                {
                    case BlockColor.Red:
                        color = Color.red;
                        break;
                    case BlockColor.Green:
                        color = Color.green;
                        break;
                    case BlockColor.Blue:
                        color = Color.blue;
                        break;
                    case BlockColor.Yellow:
                        color = Color.yellow;
                        break;
                }
                var pos = fieldTransform.TransformPoint(field.GetLocalPosition(coord));
                Debug.DrawLine(
                    pos + new Vector3(-0.5f, 0.5f),
                    pos + new Vector3(0.5f, -0.5f),
                    color);
                Debug.DrawLine(
                    pos + new Vector3(0.5f, 0.5f),
                    pos + new Vector3(-0.5f, -0.5f),
                    color);
                if (block.Falling)
                {
                    Debug.DrawRay(pos, Vector3.down * 0.5f, Color.magenta);
                }
            }
        }
    }

    public void SetDirection(Direction direction)
    {
        _directionText.text = direction.ToString();
    }

    public void SetCoord(FieldCoord coord)
    {
        _coordText.text = string.Format("{0},{1}", coord.Col, coord.Row);
    }

    public void SetPosition(Vector3 pos)
    {
        _positionText.text = string.Format("x:{0:F3} y:{1:F3}", pos.x, pos.y);
    }

    public void Analyze(FieldCoord coord)
    {
        if (Dig.Field.IsBlock(coord))
        {
            var block = Dig.Field.GetBlock(coord);
            _analyzeText.text = string.Format("{0},{1} = Block({2} {3})",
                coord.Col,
                coord.Row,
                block.Color,
                block.Falling);
        }
        else
        {
            _analyzeText.text = string.Format("{0},{1} = Empty", coord.Col, coord.Row);
        }

    }
}
