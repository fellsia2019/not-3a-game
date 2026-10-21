#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Not3A.Stage3.VisualPilot.Tests
{
    public sealed class Stage3TravelNetworkRulesTests
    {
        private Type stateType,pointType;
        private object state;
        [SetUp] public void Create()
        {
            stateType=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Not3A.Stage3.VisualPilot.Stage3TravelState")).First(t=>t!=null);
            pointType=stateType.Assembly.GetType("Not3A.Stage3.VisualPilot.Stage3TravelPoint");state=Activator.CreateInstance(stateType);
            var points=Array.CreateInstance(pointType,3);
            for(var i=0;i<3;i++)points.SetValue(Activator.CreateInstance(pointType,new object[]{new[]{"base","city","fixture-third"}[i],"Test "+i,new Vector2(i*10,1)}),i);
            Call("Configure",points);
        }
        private object Call(string name,params object[] args)=>stateType.GetMethods().Single(m=>m.Name==name&&m.GetParameters().Length==args.Length).Invoke(state,args);
        private bool Route(string from,string to)=>(bool)Call("CanRoute",from,to);
        [Test] public void ThirdOpenedPointUsesTheSameRoutesWithoutUnlockingUnknownPoints()
        {
            Call("BuildWorkshop");Call("Study");Call("BuildBaseCircle",new Vector2(4,7));Call("Discover","city");
            Assert.That(Route("base","city"),Is.True);Assert.That(Route("base","fixture-third"),Is.False);
            Call("Discover","fixture-third");Assert.That(Route("city","fixture-third"),Is.True);Assert.That(Route("fixture-third","base"),Is.True);
            Assert.That(Route("base","base"),Is.False);Assert.That(Route("city","invented"),Is.False);
        }
        [Test] public void SnapshotAndResetPreserveOnlyRegisteredOpenedPointsAndResearch()
        {
            Call("Discover","city");Assert.That(Route("city","base"),Is.False,"Discovery cannot bypass research.");
            Call("BuildWorkshop");Call("Study");Call("BuildBaseCircle",new Vector2(4,7));Call("Discover","fixture-third");var saved=Call("CaptureSnapshot");Call("Reset");
            Assert.That(Route("base","city"),Is.False);Call("RestoreSnapshot",saved);
            Assert.That(Route("city","fixture-third"),Is.True);Assert.That(Route("base","city"),Is.True);
        }
        [Test] public void ResearchDoesNotOpenAnUnbuiltBaseAndConstructionIsSingleAndSnapshotted()
        {
            Assert.That(Call("BuildBaseCircle",new Vector2(4,7)),Is.False,"Research is required.");
            Call("BuildWorkshop");Call("Discover","city");Call("Study");
            Assert.That(Call("IsOpened","base"),Is.False);Assert.That(Route("city","base"),Is.False);
            Assert.That(Call("BuildBaseCircle",new Vector2(float.NaN,7)),Is.False);
            var chosen=new Vector2(4,7);Assert.That(Call("BuildBaseCircle",chosen),Is.True);
            Assert.That(Call("BuildBaseCircle",new Vector2(8,9)),Is.False,"Only one player base endpoint.");
            var saved=Call("CaptureSnapshot");Call("Reset");
            Assert.That((bool)stateType.GetProperty("BaseBuilt").GetValue(state),Is.False);
            Assert.That(Call("IsOpened","base"),Is.False);Call("RestoreSnapshot",saved);
            Assert.That((bool)stateType.GetProperty("BaseBuilt").GetValue(state),Is.True);
            Assert.That(Route("base","city"),Is.True);
            var points=(System.Collections.IEnumerable)stateType.GetProperty("Points").GetValue(state);
            foreach(var point in points)if((string)pointType.GetField("Id").GetValue(point)=="base")
                Assert.That((Vector2)pointType.GetField("Position").GetValue(point),Is.EqualTo(chosen));
        }
        [Test] public void DuplicateDestinationIdsFailBeforeReplacingTheRegistry()
        {
            var points=Array.CreateInstance(pointType,2);
            for(var i=0;i<2;i++)points.SetValue(Activator.CreateInstance(pointType,new object[]{"same","Test",Vector2.zero}),i);
            var error=Assert.Throws<TargetInvocationException>(()=>Call("Configure",points));Assert.That(error.InnerException,Is.TypeOf<ArgumentException>());
            Assert.That((bool)Call("Discover","city"),Is.True,"Rejected definitions leave the old registry intact.");
        }
    }
}
#endif
