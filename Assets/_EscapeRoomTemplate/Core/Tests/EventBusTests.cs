using System.Text.RegularExpressions;
using EscapeRoomRevolt.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EscapeRoomRevolt.Core.Tests
{
    public class EventBusTests
    {
        private struct TestEvent { public int value; }
        private struct OtherEvent { public int value; }

        [TearDown]
        public void TearDown() => EventBus.Clear();

        [Test]
        public void Publish_InvokesSubscribedHandlerWithData()
        {
            int received = -1;
            EventBus.Subscribe<TestEvent>(evt => received = evt.value);

            EventBus.Publish(new TestEvent { value = 42 });

            Assert.AreEqual(42, received);
        }

        [Test]
        public void Publish_DoesNotInvokeHandlersOfOtherEventTypes()
        {
            bool wrongHandlerCalled = false;
            EventBus.Subscribe<OtherEvent>(_ => wrongHandlerCalled = true);

            EventBus.Publish(new TestEvent { value = 1 });

            Assert.IsFalse(wrongHandlerCalled);
        }

        [Test]
        public void Unsubscribe_StopsFurtherInvocations()
        {
            int callCount = 0;
            void Handler(TestEvent evt) => callCount++;

            EventBus.Subscribe<TestEvent>(Handler);
            EventBus.Publish(new TestEvent());
            EventBus.Unsubscribe<TestEvent>(Handler);
            EventBus.Publish(new TestEvent());

            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void Publish_WithNoSubscribers_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => EventBus.Publish(new TestEvent { value = 1 }));
        }

        [Test]
        public void Publish_HandlerThatThrows_DoesNotStopOtherHandlers()
        {
            bool secondHandlerCalled = false;
            EventBus.Subscribe<TestEvent>(_ => throw new System.Exception("boom"));
            EventBus.Subscribe<TestEvent>(_ => secondHandlerCalled = true);

            // EventBus.Publish deliberately catches and logs a misbehaving handler instead of
            // propagating — tell the framework that error log is expected, not a test failure.
            LogAssert.Expect(LogType.Error, new Regex(@"\[EventBus\] Error in handler for TestEvent.*"));
            Assert.DoesNotThrow(() => EventBus.Publish(new TestEvent()));
            Assert.IsTrue(secondHandlerCalled);
        }

        [Test]
        public void GetSubscriberCount_ReflectsSubscribeAndUnsubscribe()
        {
            void Handler(TestEvent evt) { }

            Assert.AreEqual(0, EventBus.GetSubscriberCount<TestEvent>());
            EventBus.Subscribe<TestEvent>(Handler);
            Assert.AreEqual(1, EventBus.GetSubscriberCount<TestEvent>());
            EventBus.Unsubscribe<TestEvent>(Handler);
            Assert.AreEqual(0, EventBus.GetSubscriberCount<TestEvent>());
        }

        [Test]
        public void Clear_RemovesAllSubscribers()
        {
            EventBus.Subscribe<TestEvent>(_ => { });
            EventBus.Clear();

            Assert.AreEqual(0, EventBus.GetSubscriberCount<TestEvent>());
        }
    }
}
