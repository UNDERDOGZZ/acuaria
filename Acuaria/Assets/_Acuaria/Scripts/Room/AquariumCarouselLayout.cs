using System.Collections.Generic;
using UnityEngine;

namespace Acuaria.Room
{
    public static class AquariumCarouselLayout
    {
        public static Vector3 Position(int occupiedIndex,float spacing,Vector3 origin=default)=>
            origin+Vector3.right*(occupiedIndex*Mathf.Max(.1f,spacing));
        public static void Calculate(int count,float spacing,List<Vector3> destination,Vector3 origin=default)
        {
            destination.Clear();
            for(var i=0;i<Mathf.Max(0,count);i++)destination.Add(Position(i,spacing,origin));
        }
        public static int Previous(int index)=>index>0?index-1:-1;
        public static int Next(int index,int count)=>index>=0&&index<count-1?index+1:-1;
        public static float VisibleWidth(float orthographicSize,float aspect)=>2f*Mathf.Max(.01f,orthographicSize)*Mathf.Max(.01f,aspect);
        public static float SpacingForPreview(float tankWidth,float visibleWidth,float preview)=>
            Mathf.Max(.1f,(tankWidth+visibleWidth)*.5f-tankWidth*Mathf.Clamp01(preview));
    }
}
