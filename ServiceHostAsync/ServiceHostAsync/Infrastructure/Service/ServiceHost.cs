using System;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using ServiceHostAsync.Extensions;

namespace ServiceHostAsync.Infrastructure
{
    sealed class ServiceHost<T> : ServiceBase where T : class, IDisposable
    {
        internal const string ServiceName = "AsyncSvcHostSample";
        private static readonly TimeSpan InfiniteTimeout = TimeSpan.FromMilliseconds(-1);
        
        private readonly Func<IServiceProvider> _providerFactory;
        private readonly ILogger _logger;
        private readonly bool _isInteractiveContext;
        private readonly AutoResetEvent _stopAsyncTrigger, _stopAsyncSignal;
        private T _host;
        private IServiceProvider _currentProvider;
        private bool _isStoppingOnException;
        private int _started;
        
        public ServiceHost(Func<IServiceProvider> serviceProviderFactory, ILogger logger)
        {
            if (serviceProviderFactory == null)
                throw new ArgumentNullException("serviceProviderFactory");
            if (logger == null)
                throw new ArgumentNullException("logger");
            _logger = logger;
            _providerFactory = serviceProviderFactory;
            try
            {
                _isInteractiveContext = Environment.UserInteractive;

                _stopAsyncTrigger = new AutoResetEvent(false);
                _stopAsyncSignal = new AutoResetEvent(false);
                TaskScheduler.UnobservedTaskException += (sender, e) =>
                {
                    _logger.Error(e.Exception, "Unhandled exception in async task.");
                    e.SetObserved();
                    Environment.Exit(1);
                };

                base.ServiceName = ServiceName;
                EventLog.Source = "Application";
                EventLog.Log = "ServiceHostAsync";
                AutoLog = true;
                CanStop = true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw;
            }
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        }
        
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (_started != 0)
                        Stop();

                    AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
                    _stopAsyncSignal.Dispose();
                    _stopAsyncTrigger.Dispose();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw;
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                if (Interlocked.Increment(ref _started) != 1)
                    return;

                _logger.Debug("Starting...");

                HandleEndOfStartAsync(Task.Run(new Func<Task>(StartAsync)));
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw;
            }
            finally
            {
                base.OnStart(args);
            }
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logger.Error((Exception)e.ExceptionObject, "Unhandled exception.");
            if (e.IsTerminating)
            {
                try
                {
                    OnStop();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                    Environment.FailFast("Failed to stop service.", ex);
                }
            }
        }

        private Task StartAsync()
        {
            _currentProvider = _providerFactory();
            var hostFactory = (Func<T>) _currentProvider.GetService(typeof(Func<T>));
            if (hostFactory == null)
                throw new InvalidOperationException("Func<T> cannot be resolved.");
            _host = hostFactory();
            return Task.FromResult(0);
        }

        private async void HandleEndOfStartAsync(Task task)
        {
            try
            {
                await task.WithStrongAwaiter(configureAwaitToContinueOnCapturedContext: false);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error while starting service.");
                _isStoppingOnException = true;
                ThreadPool.QueueUserWorkItem(o => OnStop());
            }
            ThreadPool.RegisterWaitForSingleObject(_stopAsyncTrigger, new WaitOrTimerCallback((state, timedOut) =>
            {
                try
                {
                    InternalTearDown();
                }
                catch (Exception ex)
                {
                    Environment.FailFast("Error while stopping.", ex);
                }
                finally
                {
                    _stopAsyncSignal.Set();
                    if (_isStoppingOnException && _isInteractiveContext)
                        Environment.Exit(1);
                }
            }), state: null, timeout: InfiniteTimeout, executeOnlyOnce: true);
            _logger.Debug("Service initialized.");
        }
        
        private void InternalTearDown()
        {
            _logger.Debug("Stopping...");

            if (_host != null)
            {
                try
                {
                    _host.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }

            var disposable = _currentProvider as IDisposable;
            if (disposable != null)
                disposable.Dispose();
            _currentProvider = null;
            _logger.Debug("Service stopped.");
        }

        protected override void OnStop()
        {
            try
            {
                if (Interlocked.Decrement(ref _started) != 0)
                    return;
                _stopAsyncTrigger.Set();

                const int interval = 10000;
                while (!_stopAsyncSignal.WaitOne(interval))
                { 
                    RequestAdditionalTime(interval);
                    _logger.Debug("Additional time requested");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw;
            }
            finally
            {
                base.OnStop();
            }
        }


        internal void SetUp(string[] args)
        {
            OnStart(args);
        }


        public static void RunAsService(Func<IServiceProvider> providerFactory, ILogger logger)
        {
            Run(new ServiceBase[]
            {
                new ServiceHost<T>(providerFactory, logger)
            });
        }
        
        public void TearDown()
        {
            Stop();
        }

        public static void RunInConsole(Func<IServiceProvider> providerFactory, ILogger logger, string[] args)
        {
            if (logger == null)
                throw new ArgumentNullException("logger");
            using (var service = new ServiceHost<T>(providerFactory, logger))
            {
                service.SetUp(args);
                logger.Debug("Service started. Press [ESC]...");
                while (!NativeMethods.PeekEscapeKey()) { }
                logger.Debug("[ESC]");
                service.TearDown();
            }
        }
    }
}