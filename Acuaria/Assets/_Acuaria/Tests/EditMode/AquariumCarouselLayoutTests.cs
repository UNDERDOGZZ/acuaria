using System.Collections.Generic;
using Acuaria.Room;
using NUnit.Framework;
using UnityEngine;

namespace Acuaria.Tests.Room
{
    public sealed class AquariumCarouselLayoutTests
    {
        [TestCase(1)][TestCase(2)][TestCase(3)][TestCase(6)]
        public void Layout_IsDeterministicAndUsesPositiveSpacing(int count)
        {
            var values=new List<Vector3>();
            AquariumCarouselLayout.Calculate(count,12f,values);
            Assert.That(values.Count,Is.EqualTo(count));
            for(var i=0;i<count;i++)Assert.That(values[i],Is.EqualTo(new Vector3(i*12f,0,0)));
        }
        [Test] public void Neighbours_RespectBoundaries()
        {
            Assert.That(AquariumCarouselLayout.Previous(0),Is.EqualTo(-1));
            Assert.That(AquariumCarouselLayout.Previous(2),Is.EqualTo(1));
            Assert.That(AquariumCarouselLayout.Next(0,3),Is.EqualTo(1));
            Assert.That(AquariumCarouselLayout.Next(2,3),Is.EqualTo(-1));
        }
        [TestCase(5.625f,1.7777778f)]
        [TestCase(5.625f,1.3333333f)]
        public void PreviewCalculation_IsFiniteForSupportedAspects(float size,float aspect)
        {
            var width=AquariumCarouselLayout.VisibleWidth(size,aspect);
            var spacing=AquariumCarouselLayout.SpacingForPreview(10f,width,.16f);
            Assert.That(float.IsFinite(width)&&float.IsFinite(spacing),Is.True);
            Assert.That(spacing,Is.GreaterThan(0));
        }
        [Test] public void CarouselDefinition_ValidatesAndScalesDuration()
        {
            var definition=ScriptableObject.CreateInstance<AquariumCarouselDefinition>();
            try
            {
                definition.Configure(12f,.16f,.48f);
                Assert.That(definition.Spacing,Is.EqualTo(12f));
                Assert.That(definition.DurationForDistance(2),Is.GreaterThan(definition.DurationForDistance(1)));
                Assert.That(definition.DurationForDistance(20),Is.LessThanOrEqualTo(.8f));
            }
            finally{Object.DestroyImmediate(definition);}
        }
    }
}
