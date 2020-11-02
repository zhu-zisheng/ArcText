using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Text))]
public class ArcText : BaseMeshEffect
{
    [SerializeField][Range(-180, 180)] protected float _arcAngle;
    [SerializeField][Range(-1, 1)] private float _spacing = 0;
    private Text _text;
    protected RectTransform _rectTrans;
    
    protected override void Awake ()
    {
        base.Awake ();
        _rectTrans = GetComponent<RectTransform> ();
        _text = GetComponent<Text>();
        OnRectTransformDimensionsChange ();
    }
    protected override void OnEnable ()
    {
        base.OnEnable ();
        _rectTrans = GetComponent<RectTransform> ();
        _text = GetComponent<Text>();
        OnRectTransformDimensionsChange ();
    }
    
    public override void ModifyMesh (Mesh mesh)
    {
        if (!this.IsActive())
            return;
 
        List<UIVertex> list = new List<UIVertex>();
        using (VertexHelper vertexHelper = new VertexHelper(mesh))
        {
            vertexHelper.GetUIVertexStream(list);
        }
 
        ModifyVertices(list);  // calls the old ModifyVertices which was used on pre 5.2
 
        using (VertexHelper vertexHelper2 = new VertexHelper())
        {
            vertexHelper2.AddUIVertexTriangleStream(list);
            vertexHelper2.FillMesh(mesh);
        }
    }
    
    public override void ModifyMesh(VertexHelper vh)
    {
        if (!this.IsActive())
            return;

        List<UIVertex> vertexList = new List<UIVertex>();
        vh.GetUIVertexStream(vertexList);

        ModifyVertices(vertexList);

        vh.Clear();
        vh.AddUIVertexTriangleStream(vertexList);
    }
    
    protected virtual void ModifyVertices (List<UIVertex> verts)
    {
        if (!IsActive())
        {
            return;
        }
        
        if (_arcAngle == 0)
        {
            return;
        }

        var quads = new List<CharacterQuad>();
        var temp = new List<UIVertex>();
        for (int i = 0; i < verts.Count; i++)
        {
            temp.Add(verts[i]);
            if ((i + 1) % 6 == 0 && i > 0)
            {
                var quad = new CharacterQuad(temp.ToArray());
                quads.Add(quad);
                temp.Clear();
            }
        }
        var chord = _rectTrans.rect.width;
        var r0 = (chord * 0.5f) / Mathf.Sin(Mathf.Deg2Rad * _arcAngle * 0.5f) + _text.fontSize * 0.5f;
        var arc0 = Mathf.Deg2Rad * _arcAngle * r0;
        var h = r0 * Mathf.Cos(Mathf.Deg2Rad * _arcAngle * 0.5f);
        for (var i = 0; i < quads.Count; i++)
        {
            var quad = quads[i];
            var pivot = new Vector3(quad.Rect.center.x, quad.Rect.center.y);
            var r = r0 + pivot.y;
            var angle = pivot.x / chord * _arcAngle * (chord / arc0) * (1 + _spacing);
            var target = new Vector3(r * Mathf.Sin(Mathf.Deg2Rad * angle), r * Mathf.Cos(Mathf.Deg2Rad * angle) - h);
            var translation = target - pivot;
            var rotation = Quaternion.Euler(0, 0, -angle);
            var m = Matrix4x4.TRS(translation, rotation, Vector3.one);
            for (var j = 0; j < quad.Verts.Length; j++)
            {
                var vert = quad.Verts[j];
                var locelPos = vert.position - pivot;
                locelPos = m.MultiplyPoint3x4(locelPos);
                vert.position = pivot + locelPos;
                verts[i * 6 + j] = vert;
            }
        }
    }
    
    private class CharacterQuad
    {
        private UIVertex[] _verts;
        private Rect _rect;
        
        public UIVertex[] Verts
        {
            get { return _verts; }
        }
        
        public Rect Rect
        {
            get { return _rect; }
        }

        public CharacterQuad(UIVertex[] verts)
        {
            if (verts.Length != 6)
            {
                Debug.LogError("verts.Length != 6");
                return;
            }
            
            float xmin = verts[0].position.x;
            float ymin = verts[0].position.y;
            float xmax = verts[0].position.x;
            float ymax = verts[0].position.y;
            
            foreach (var vert in verts)
            {
                if (vert.position.x < xmin)
                {
                    xmin = vert.position.x;
                }

                if (vert.position.y < ymin)
                {
                    ymin = vert.position.y;
                }

                if (vert.position.x > xmax)
                {
                    xmax = vert.position.x;
                }

                if (vert.position.y > ymax)
                {
                    ymax = vert.position.y;
                }
            }
            
            _verts = verts;
            _rect = Rect.MinMaxRect(xmin, ymin, xmax, ymax);
        }
    }
}
