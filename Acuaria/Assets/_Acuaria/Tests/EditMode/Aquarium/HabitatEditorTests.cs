using System.Collections.Generic;
using Acuaria.Aquarium.Decorations;
using NUnit.Framework;
using UnityEngine;

namespace Acuaria.Tests.EditMode.Aquarium
{
    public sealed class HabitatEditorTests
    {
        readonly List<Object> created=new();
        T New<T>()where T:ScriptableObject{var x=ScriptableObject.CreateInstance<T>();created.Add(x);return x;}
        [TearDown]public void TearDown(){foreach(var x in created)Object.DestroyImmediate(x);created.Clear();}
        DecorationDefinition Decoration(string id,DecorationCategory category,Vector2 scale)
        {var d=New<DecorationDefinition>();d.Configure(id,id,"",category,scale,.1f,new HabitatContribution(),"");return d;}
        HabitatEditorDefinition Settings(float overlap=.55f,int undo=20){var s=New<HabitatEditorDefinition>();s.Configure("test",.05f,overlap,undo);return s;}

        [Test]public void Definition_ValidatesRequiredValues(){Assert.That(Settings().IsValid,Is.True);var invalid=Settings();invalid.Configure("",0,float.NaN,-1);Assert.That(invalid.IsValid,Is.False);}
        [Test]public void Snapshot_IsIndependentAndDetectsChanges()
        {var d=Decoration("rock",DecorationCategory.Rock,new(.2f,.2f));var p=new DecorationPlacementData("1",d,new(.2f,.2f),Vector2.one);var snapshot=new HabitatLayoutSnapshot(new[]{p});var copy=snapshot.CreateWorkingCopy();copy[0]=copy[0].WithPosition(new(.8f,.2f));Assert.That(snapshot.Placements[0].NormalizedPosition.x,Is.EqualTo(.2f));Assert.That(snapshot.Matches(copy),Is.False);}
        [Test]public void Selection_IsSafeAndDoesNotMutatePlacement()
        {var d=Decoration("plant",DecorationCategory.Plant,new(.1f,.2f));var p=new DecorationPlacementData("p",d,new(.2f,.2f),Vector2.one);var model=new DecorationSelectionModel();Assert.That(model.Select("p",new[]{p}),Is.True);Assert.That(model.Select("missing",new[]{p}),Is.False);Assert.That(p.NormalizedPosition.x,Is.EqualTo(.2f));model.Clear();Assert.That(model.SelectedId,Is.Null);}
        [Test]public void Validator_SnapsBottomAndRejectsOutside()
        {var d=Decoration("plant",DecorationCategory.Plant,new(.1f,.2f));var validator=new DecorationPlacementValidator(Settings());var p=new DecorationPlacementData("p",d,new(.4f,.8f),Vector2.one);var valid=validator.Validate(p,new[]{p});Assert.That(valid.IsValid,Is.True);Assert.That(valid.CorrectedPosition.y,Is.LessThan(.2f));var outside=new DecorationPlacementData("p",d,new(0,.2f),Vector2.one);Assert.That(validator.Validate(outside,new[]{outside}).Validity,Is.EqualTo(DecorationPlacementValidity.OutsideBounds));}
        [Test]public void Overlap_IsZeroPartialAndComplete()
        {var a=new DecorationFootprint(new Rect(0,0,1,1));Assert.That(DecorationOverlapEvaluator.Ratio(a,new(new Rect(2,2,1,1))),Is.Zero);Assert.That(DecorationOverlapEvaluator.Ratio(a,new(new Rect(.5f,0,1,1))),Is.EqualTo(.5f));Assert.That(DecorationOverlapEvaluator.Ratio(a,a),Is.EqualTo(1));}
        [Test]public void Validator_BlocksCriticalOverlap()
        {var d=Decoration("rock",DecorationCategory.Rock,new(.2f,.2f));var a=new DecorationPlacementData("a",d,new(.5f,.2f),Vector2.one);var b=new DecorationPlacementData("b",d,new(.5f,.2f),Vector2.one);Assert.That(new DecorationPlacementValidator(Settings(.2f)).Validate(a,new[]{a,b}).Validity,Is.EqualTo(DecorationPlacementValidity.ExcessiveOverlap));}
        [Test]public void AddRemoveMoveFlipCommands_AreReversible()
        {var d=Decoration("wood",DecorationCategory.Wood,new(.2f,.1f));var p=new DecorationPlacementData("x",d,new(.2f,.2f),Vector2.one);var list=new List<DecorationPlacementData>();var add=new AddDecorationCommand(p);Assert.That(add.Execute(list),Is.True);add.Undo(list);Assert.That(list,Is.Empty);list.Add(p);var move=new ReplacePlacementCommand(p,p.WithPosition(new(.7f,.2f)).WithFlip(true));Assert.That(move.Execute(list),Is.True);Assert.That(list[0].FlipX,Is.True);move.Undo(list);Assert.That(list[0].NormalizedPosition.x,Is.EqualTo(.2f));var remove=new RemoveDecorationCommand(p);remove.Execute(list);remove.Undo(list);Assert.That(list[0].InstanceId,Is.EqualTo("x"));}
        [Test]public void History_RespectsLimitAndUndoOrder()
        {var d=Decoration("rock",DecorationCategory.Rock,new(.1f,.1f));var p=new DecorationPlacementData("x",d,new(.2f,.2f),Vector2.one);var list=new List<DecorationPlacementData>{p};var history=new HabitatEditHistory(1);history.Execute(new ReplacePlacementCommand(list[0],list[0].WithPosition(new(.3f,.2f))),list);history.Execute(new ReplacePlacementCommand(list[0],list[0].WithPosition(new(.4f,.2f))),list);Assert.That(history.Count,Is.EqualTo(1));Assert.That(history.Undo(list),Is.True);Assert.That(list[0].NormalizedPosition.x,Is.EqualTo(.3f).Within(.001f));}
    }
}
