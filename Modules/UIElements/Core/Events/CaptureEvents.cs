// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;

namespace UnityEngine.UIElements
{
    /// <summary>
    /// Interface for pointer capture events and mouse capture events.
    /// </summary>
    /// <remarks>
    /// Refer to the [[wiki:UIE-Capture-Events|Capture events]] manual page for more information and examples.
    /// </remarks>
    public interface IPointerCaptureEvent
    {
    }

    internal interface IPointerCaptureEventInternal
    {
        int pointerId { get; }
    }

    /// <summary>
    /// Base class for pointer capture events and mouse capture events.
    /// </summary>
    /// <remarks>
    /// Refer to the [[wiki:UIE-Capture-Events|Capture events]] manual page for more information and examples.
    /// </remarks>
    [EventCategory(EventCategory.Pointer)]
    public abstract class PointerCaptureEventBase<T> : EventBase<T>, IPointerCaptureEvent, IPointerCaptureEventInternal where T : PointerCaptureEventBase<T>, new()
    {
        /// <summary>
        /// For PointerCaptureEvent and MouseCaptureEvent, returns the VisualElement that loses the pointer capture, if any. For PointerCaptureOutEvent and MouseCaptureOutEvent, returns the VisualElement that captures the pointer.
        /// </summary>
        public IEventHandler relatedTarget { get; private set; }
        /// <summary>
        /// Identifies the pointer that sends the event.
        /// </summary>
        /// <remarks>
        /// If the mouse sends the event, this property is set to 0. If a touchscreen device sends the event, this property is set to the finger ID, which ranges from 1 to the number of touches the device supports.
        /// </remarks>
        public int pointerId { get; private set; }

        /// <summary>
        /// Resets the event members to their initial values.
        /// </summary>
        protected override void Init()
        {
            base.Init();
            LocalInit();
        }

        void LocalInit()
        {
            propagation = EventPropagation.TricklesDown | EventPropagation.Bubbles;
            relatedTarget = null;
            pointerId = PointerId.invalidPointerId;
        }

        /// <summary>
        /// Gets an event from the event pool and initializes it with the given values. Use this function instead of creating new events. Events obtained using this method need to be released back to the pool. You can use `Dispose()` to release them.
        /// </summary>
        /// <param name="target">For PointerCapture and MouseCapture events, the element that captures the pointer. For PointerCaptureOut and MouseCaptureOut events, the element that releases the pointer.</param>
        /// <param name="relatedTarget">For PointerCaptureEvent and MouseCaptureEvent, returns the element that loses the pointer capture, if any. For PointerCaptureOutEvent and MouseCaptureOutEvent, returns the element that captures the pointer.</param>
        /// <param name="pointerId">The pointer that is captured or released.</param>
        /// <returns>An initialized event.</returns>
        public static T GetPooled(IEventHandler target, IEventHandler relatedTarget, int pointerId)
        {
            T e = GetPooled();
            e.elementTarget = (VisualElement) target;
            e.relatedTarget = relatedTarget;
            e.pointerId = pointerId;
            return e;
        }

        protected PointerCaptureEventBase()
        {
            LocalInit();
        }
    }

    /// <summary>
    /// Event sent when a VisualElement releases a pointer.
    /// </summary>
    public class PointerCaptureOutEvent : PointerCaptureEventBase<PointerCaptureOutEvent>
    {
        static PointerCaptureOutEvent()
        {
            SetCreateFunction(() => new PointerCaptureOutEvent());
        }

        protected internal override void PreDispatch(IPanel panel)
        {
            base.PreDispatch(panel);
            elementTarget.UpdateHoverPseudoStateAfterCaptureChange(pointerId);
        }
    }

    /// <summary>
    /// Event sent when a pointer is captured by a VisualElement.
    /// </summary>
    /// <remarks>
    /// When a pointer is captured by a VisualElement, all pointer events are sent to that VisualElement until the pointer is released.
    /// </remarks>
    public class PointerCaptureEvent : PointerCaptureEventBase<PointerCaptureEvent>
    {
        static PointerCaptureEvent()
        {
            SetCreateFunction(() => new PointerCaptureEvent());
        }

        protected internal override void PreDispatch(IPanel panel)
        {
            base.PreDispatch(panel);
            elementTarget.UpdateHoverPseudoStateAfterCaptureChange(pointerId);
        }
    }


    /// <summary>
    /// Interface for mouse capture events.
    /// </summary>
    /// <remarks>
    /// Refer to the [[wiki:UIE-Capture-Events|Capture events]] manual page for more information and examples.
    /// </remarks>
    public interface IMouseCaptureEvent
    {
    }

    /// <summary>
    /// Event sent when the handler capturing the mouse changes.
    /// </summary>
    /// <remarks>
    /// Refer to the [[wiki:UIE-Capture-Events|Capture events]] manual page for more information and examples.
    /// </remarks>
    public abstract class MouseCaptureEventBase<T> : PointerCaptureEventBase<T>, IMouseCaptureEvent where T : MouseCaptureEventBase<T>, new()
    {
        /// <summary>
        /// In the case of a MouseCaptureEvent, this property is the IEventHandler that loses the capture. In the case of a MouseCaptureOutEvent, this property is the IEventHandler that gains the capture.
        /// </summary>
        public new IEventHandler relatedTarget => base.relatedTarget;

        /// <summary>
        /// Gets an event from the event pool and initializes it with the given values. Use this function instead of creating new events. Events obtained using this method need to be released back to the pool. You can use `Dispose()` to release them.
        /// </summary>
        /// <param name="target">The handler taking or releasing the mouse capture.</param>
        /// <param name="relatedTarget">The related target.</param>
        /// <returns>An initialized event.</returns>
        public static T GetPooled(IEventHandler target, IEventHandler relatedTarget)
        {
            T e = GetPooled(target, relatedTarget, 0);
            return e;
        }
    }

    /// <summary>
    /// Event sent before a handler stops capturing the mouse.
    /// </summary>
    public class MouseCaptureOutEvent : MouseCaptureEventBase<MouseCaptureOutEvent>
    {
        static MouseCaptureOutEvent()
        {
            SetCreateFunction(() => new MouseCaptureOutEvent());
        }

        protected internal override void PreDispatch(IPanel panel)
        {
            base.PreDispatch(panel);

            // Updating cursor has to happen on MouseOver/Out because exiting a child does not send a mouse enter to the parent.
            // We can use MouseEvents instead of PointerEvents since only the mouse has a displayed cursor.
            elementTarget.UpdateCursorStyle(eventTypeId);
        }
    }

    /// <summary>
    /// Event sent after a handler starts capturing the mouse.
    /// </summary>
    public class MouseCaptureEvent : MouseCaptureEventBase<MouseCaptureEvent>
    {
        static MouseCaptureEvent()
        {
            SetCreateFunction(() => new MouseCaptureEvent());
        }
    }
}
