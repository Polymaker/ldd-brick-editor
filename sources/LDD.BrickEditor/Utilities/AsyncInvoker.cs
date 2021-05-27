using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LDD.BrickEditor.Utilities
{
    public class AsyncInvoker
    {
        public ISynchronizeInvoke SynchronizationObject { get; }
        public bool IsRunning { get; private set; }
        private Queue<ScheduledAction> ActionQueue { get; }

        public AsyncInvoker(ISynchronizeInvoke synchronizationObject)
        {
            SynchronizationObject = synchronizationObject;
            ActionQueue = new Queue<ScheduledAction>();
        }

        

        private class ScheduledAction
        {
            public Action Action { get; set; }
            public bool IsAsync { get; set; }
            public ScheduledAction NextAction { get; set; }

            public ScheduledAction(Action action, bool isAsync)
            {
                Action = action;
                IsAsync = isAsync;
            }

            public void Trigger()
            {
                Action();
            }
        }

        public InvokeBuilder WhenShown(System.Windows.Forms.Form form, Action action)
        {
            var newAction = new ScheduledAction(action, false);
            ActionQueue.Enqueue(newAction);

            void tmpDelegate(object s, EventArgs e)
            {
                form.Shown -= tmpDelegate;
                newAction.Trigger();
                IsRunning = true;
            }

            form.Shown += tmpDelegate;

            return new InvokeBuilder(this);
        }

        public InvokeBuilder Do(Action action)
        {
            return new InvokeBuilder(this).Then(action);
        }

        public InvokeBuilder DoAsync(Action action)
        {
            return new InvokeBuilder(this).ThenAsync(action);
        }

        private void ExecuteActions()
        {

        }

        private ScheduledAction LastAction { get; set; }

        public class InvokeBuilder
        {
            public AsyncInvoker Invoker { get; }

            public InvokeBuilder(AsyncInvoker invoker)
            {
                Invoker = invoker;
            }

            public InvokeBuilder Then(Action action)
            {
                if (Invoker.IsRunning)
                    return this;

                var newAction = new ScheduledAction(action, false);
                Invoker.ActionQueue.Enqueue(newAction);
                return this;
            }

            public InvokeBuilder ThenAsync(Action action)
            {
                if (Invoker.IsRunning)
                    return this;

                var newAction = new ScheduledAction(action, true);
                Invoker.ActionQueue.Enqueue(newAction);
                return this;
            }

            public void Start()
            {

            }
        }
    }
}
