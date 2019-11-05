using SVGMeshUnity.Internals;
using SVGMeshUnity.Internals.Cdt2d;
using UnityEngine;
using UnityEngine.UI;

namespace SVGMeshUnity
{
    public class WaveImage : Graphic
    {
        // https://github.com/mattdesl/adaptive-bezier-curve

        [SerializeField, Range(0f, 1f)] private float waveCenterY;
        [SerializeField, Range(0f, 1f)] private float waveHorRadius, waveVertRadius, sideWidth;
        private SVGData _data;

        private static WorkBufferPool WorkBufferPool = new WorkBufferPool();
        private MeshData MeshData = new MeshData();
        private BezierToVertex BezierToVertex;
        private Triangulation Triangulation;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            if (BezierToVertex == null)
                BezierToVertex = new BezierToVertex
                {
                    WorkBufferPool = WorkBufferPool,
                    Scale = 1
                };

            if (Triangulation == null)
                Triangulation = new Triangulation
                {
                    Delaunay = false,
                    Interior = true,
                    Exterior = false,
                    Infinity = false,
                    WorkBufferPool = WorkBufferPool
                };


            var selfRect = ((RectTransform) transform).rect;
            var svgData = BuildWave(selfRect,
                waveCenterY * selfRect.height,
                waveHorRadius * selfRect.width,
                waveVertRadius * selfRect.height,
                sideWidth * selfRect.width);

            MeshData.Clear();
            BezierToVertex.GetContours(svgData, MeshData);
            Triangulation.BuildTriangles(MeshData);

            MeshData.MakeUnityFriendly();
            MeshData.Upload(vh, color);
        }

        private static SVGData BuildWave(Rect bounds, float waveCenterY, float waveHorRadius, float waveVertRadius,
            float sideWidth)
        {
            var rect = bounds;
            var path = new SVGData();
            var maskWidth = rect.width - sideWidth;
            //path.Move(new Vector2(maskWidth - sideWidth, 0f - waveVertRadius * 2));
            //path.Line(new Vector2(0f, 0f - waveVertRadius * 2));
            //path.Line(new Vector2(0f, rect.height + waveVertRadius * 2));
            //path.Line(new Vector2(maskWidth, rect.height + waveVertRadius * 2));

            var curveStartY = waveCenterY + waveVertRadius;

            path.Move(new Vector2(maskWidth, curveStartY));

            path.CurveOther(
                new Vector2(maskWidth - waveHorRadius * 0.1561501458f, curveStartY - waveVertRadius * 0.3322374268f),
                new Vector2(maskWidth, curveStartY - waveVertRadius * 0.1346194756f),
                new Vector2(maskWidth - waveHorRadius * 0.05341339583f, curveStartY - waveVertRadius * 0.2412779634f)
            );

            path.CurveOther(
                new Vector2(maskWidth - waveHorRadius * 0.5012484792f, curveStartY - waveVertRadius * 0.5350576951f),
                new Vector2(maskWidth - waveHorRadius * 0.2361659167f, curveStartY - waveVertRadius * 0.4030805244f),
                new Vector2(maskWidth - waveHorRadius * 0.3305285625f, curveStartY - waveVertRadius * 0.4561193293f)
            );

            path.CurveOther(
                new Vector2(maskWidth - waveHorRadius * 0.574934875f, curveStartY - waveVertRadius * 0.5689655122f),
                new Vector2(maskWidth - waveHorRadius * 0.515878125f, curveStartY - waveVertRadius * 0.5418222317f),
                new Vector2(maskWidth - waveHorRadius * 0.5664134792f, curveStartY - waveVertRadius * 0.5650349878f)
            );

            path.CurveOther(
                new Vector2(maskWidth - waveHorRadius * 0.8774032292f, curveStartY - waveVertRadius * 0.7399037439f),
                new Vector2(maskWidth - waveHorRadius * 0.7283715208f, curveStartY - waveVertRadius * 0.6397387195f),
                new Vector2(maskWidth - waveHorRadius * 0.8086618958f, curveStartY - waveVertRadius * 0.6833456585f)
            );

            path.CurveOther(
                new Vector2(maskWidth - waveHorRadius, curveStartY - waveVertRadius),
                new Vector2(maskWidth - waveHorRadius * 0.9653464583f, curveStartY - waveVertRadius * 0.8122605122f),
                new Vector2(maskWidth - waveHorRadius, curveStartY - waveVertRadius * 0.8936183659f)
            );

            path.CurveOther(
                new Vector2(maskWidth - waveHorRadius * 0.8608411667f, curveStartY - waveVertRadius * 1.270484439f),
                new Vector2(maskWidth - waveHorRadius, curveStartY - waveVertRadius * 1.100142878f),
                new Vector2(maskWidth - waveHorRadius * 0.9595746667f, curveStartY - waveVertRadius * 1.1887991951f)
            );

            path.CurveOther(
                new Vector2(maskWidth - waveHorRadius * 0.5291125625f, curveStartY - waveVertRadius * 1.4665102805f),
                new Vector2(maskWidth - waveHorRadius * 0.7852123333f, curveStartY - waveVertRadius * 1.3330544756f),
                new Vector2(maskWidth - waveHorRadius * 0.703382125f, curveStartY - waveVertRadius * 1.3795848049f)
            );

            path.CurveOther(
                new Vector2(maskWidth - waveHorRadius * 0.5015305417f, curveStartY - waveVertRadius * 1.4802616098f),
                new Vector2(maskWidth - waveHorRadius * 0.5241858333f, curveStartY - waveVertRadius * 1.4689677195f),
                new Vector2(maskWidth - waveHorRadius * 0.505739125f, curveStartY - waveVertRadius * 1.4781625854f)
            );

            path.CurveOther(
                new Vector2(maskWidth - waveHorRadius * 0.1541165417f, curveStartY - waveVertRadius * 1.687403f),
                new Vector2(maskWidth - waveHorRadius * 0.3187486042f, curveStartY - waveVertRadius * 1.5714239024f),
                new Vector2(maskWidth - waveHorRadius * 0.2332057083f, curveStartY - waveVertRadius * 1.6204116463f)
            );

            path.CurveOther(
                new Vector2(maskWidth, curveStartY - waveVertRadius * 2f),
                new Vector2(maskWidth - waveHorRadius * 0.0509933125f, curveStartY - waveVertRadius * 1.774752061f),
                new Vector2(maskWidth, curveStartY - waveVertRadius * 1.8709256829f)
            );

            //path.Line(new Vector2(maskWidth, 0f - waveVertRadius * 2));
            return path;
        }
    }
}