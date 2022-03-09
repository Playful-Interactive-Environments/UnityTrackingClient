using System.Collections.Generic;
using UnityEngine;
using UnityPharusAPI.TransmissionFrameworks.Tracklink;

namespace Assets.UnityPharusAPI.Helper
{
    public static class VectorAdapter
    {
        public static Vector2 ToUnityVector2(Vector2f input)
        {
            return new Vector2(input.x, input.y);
        }

        public static List<Vector2> ToUnityVector2List(List<Vector2f> input)
        {
            List<Vector2> outputList = new List<Vector2>();
            for (int i = 0; i < input.Count; i++)
            {
                outputList.Add(new Vector2(input[i].x, input[i].y));
            }

            return outputList;
        }

        public static List<Vector2f> ToPharusVector2List(List<Vector2> input)
        {
            List<Vector2f> outputList = new List<Vector2f>();
            for (int i = 0; i < input.Count; i++)
            {
                outputList.Add(new Vector2f(input[i].x, input[i].y));
            }

            return outputList;
        }
    }
}