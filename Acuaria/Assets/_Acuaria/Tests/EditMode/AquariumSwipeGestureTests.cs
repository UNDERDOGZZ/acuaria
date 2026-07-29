using Acuaria.Room;
using NUnit.Framework;
using UnityEngine;

namespace Acuaria.Tests.Room
{
    public sealed class AquariumSwipeGestureTests
    {
        [Test] public void LeftSwipe_SelectsNext()=>
            Assert.That(AquariumSwipeGesture.Evaluate(new Vector2(300,200),new Vector2(120,205),.4f),Is.EqualTo(AquariumSwipeResult.Next));
        [Test] public void RightSwipe_SelectsPrevious()=>
            Assert.That(AquariumSwipeGesture.Evaluate(new Vector2(120,200),new Vector2(300,205),.4f),Is.EqualTo(AquariumSwipeResult.Previous));
        [Test] public void ShortOrVerticalGesture_DoesNothing()
        {
            Assert.That(AquariumSwipeGesture.Evaluate(Vector2.zero,new Vector2(40,2),.3f),Is.EqualTo(AquariumSwipeResult.None));
            Assert.That(AquariumSwipeGesture.Evaluate(Vector2.zero,new Vector2(100,180),.3f),Is.EqualTo(AquariumSwipeResult.None));
        }
        [Test] public void SlowGesture_DoesNothing()=>
            Assert.That(AquariumSwipeGesture.Evaluate(Vector2.zero,new Vector2(200,0),2f),Is.EqualTo(AquariumSwipeResult.None));
    }
}
