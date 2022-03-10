using System.Collections.Generic;

namespace UnityPharusAPI.TransmissionFrameworks.Tracklink
{
    public static class UnityPharusUtils
    {
        /// <summary>
        /// Converts the list to the List<UnityEngine.Vector2> type
        /// </summary>
        /// <returns>new List<UnityEngine.Vector2>.</returns>
        /// <param name="v2fList">List<PharusTransmission.Vector2f>.</param>
        public static List<Vector2f> ToVector2List(this List<Vector2f> v2fList)
        {
            List<Vector2f> v2List = new List<Vector2f>();
            for (int i = 0; i < v2fList.Count; i++)
            {
                v2List.Add(new Vector2f(v2fList[i].x, v2fList[i].y));
            }
            return v2List;
        }

        public static void AddToVector2List(this List<Vector2f> v2fList, List<Vector2f> v2List)
        {
            for (int i = 0; i < v2fList.Count; i++)
            {
                v2List.Add(new Vector2f(v2fList[i].x, v2fList[i].y));
            }
        }
    }
}
