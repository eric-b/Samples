using System;
using System.Threading.Tasks;

namespace ServiceHostAsync.Extensions
{
    static class ExtensionMethods
    {
        public static StrongAwaiter<TResult> WithStrongAwaiter<TResult>(this Task<TResult> @task, bool configureAwaitToContinueOnCapturedContext)
        {
            return new StrongAwaiter<TResult>(@task, configureAwaitToContinueOnCapturedContext);
        }
        
        public static StrongAwaiter WithStrongAwaiter(this Task @task, bool configureAwaitToContinueOnCapturedContext)
        {
            return new StrongAwaiter(@task, configureAwaitToContinueOnCapturedContext);
        }

        public sealed class StrongAwaiter<TResult> : System.Runtime.CompilerServices.ICriticalNotifyCompletion
        {
            // Source: http://stackoverflow.com/a/26983205/249742
            private readonly Task<TResult> _task;
            private readonly System.Runtime.CompilerServices.ConfiguredTaskAwaitable<TResult>.ConfiguredTaskAwaiter _configuredAwaiter;
            private System.Runtime.InteropServices.GCHandle _gcHandle;

            public bool IsCompleted
            {
                get
                {
                    return _task.IsCompleted;
                }
            }

            public StrongAwaiter(Task<TResult> task, bool configureAwaitToContinueOnCapturedContext)
            {
                _task = task;
                _configuredAwaiter = _task.ConfigureAwait(configureAwaitToContinueOnCapturedContext).GetAwaiter();
            }

            public StrongAwaiter<TResult> GetAwaiter()
            {
                return this;
            }

            public TResult GetResult()
            {
                return _configuredAwaiter.GetResult();
            }

            public void OnCompleted(Action continuation)
            {
                _configuredAwaiter.OnCompleted(WrapContinuation(continuation));
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                _configuredAwaiter.UnsafeOnCompleted(WrapContinuation(continuation));
            }

            private Action WrapContinuation(Action continuation)
            {
                Action wrapper = () =>
                {
                    _gcHandle.Free();
                    continuation();
                };

                _gcHandle = System.Runtime.InteropServices.GCHandle.Alloc(wrapper);
                return wrapper;
            }
        }


        public sealed class StrongAwaiter : System.Runtime.CompilerServices.ICriticalNotifyCompletion
        {
            // Source: http://stackoverflow.com/a/26983205/249742
            private readonly Task _task;
            private readonly System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter _configuredAwaiter;
            private System.Runtime.InteropServices.GCHandle _gcHandle;

            public bool IsCompleted
            {
                get
                {
                    return _task.IsCompleted;
                }
            }

            public StrongAwaiter(Task task, bool configureAwaitToContinueOnCapturedContext)
            {
                _task = task;
                _configuredAwaiter = _task.ConfigureAwait(configureAwaitToContinueOnCapturedContext).GetAwaiter();
            }

            public StrongAwaiter GetAwaiter()
            {
                return this;
            }

            public void GetResult()
            {
                _configuredAwaiter.GetResult();
            }

            public void OnCompleted(Action continuation)
            {
                _configuredAwaiter.OnCompleted(WrapContinuation(continuation));
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                _configuredAwaiter.UnsafeOnCompleted(WrapContinuation(continuation));
            }

            private Action WrapContinuation(Action continuation)
            {
                Action wrapper = () =>
                {
                    _gcHandle.Free();
                    continuation();
                };

                _gcHandle = System.Runtime.InteropServices.GCHandle.Alloc(wrapper);
                return wrapper;
            }
        }

    }
}
