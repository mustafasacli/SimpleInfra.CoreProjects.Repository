namespace SimpleInfra.IoC
{
    using SimpleFileLogging;
    using SimpleFileLogging.Enums;
    using SimpleFileLogging.Interfaces;
    using SimpleInjector;
    using SimpleInjector.Lifestyles;
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Simple IoC class
    /// </summary>
    public class SimpleIoC
    {
        /// <summary>   The lock object. </summary>
        private static object lockObj = new object();

        /// <summary>   The container. </summary>
        private Container container = null;

        /// <summary>   The instance lazy. </summary>
        private static Lazy<SimpleIoC> instanceLazy = new Lazy<SimpleIoC>(() =>
        {
            return new SimpleIoC();
        }, isThreadSafe: true);

        /// <summary>
        /// ISimpleLogger instance.
        /// </summary>
        protected ISimpleLogger Logger
        { get { return SimpleFileLogger.Instance; } }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Constructor that prevents a default instance of this class from being created.
        /// </summary>
        ///
        /// <remarks>   Mustafa SAÇLI, 23.01.2020. </remarks>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        private SimpleIoC()
        {
            this.Logger.LogDateFormatType = SimpleLogDateFormats.Hour;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets the ınstance. </summary>
        ///
        /// <value> The instance. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public static SimpleIoC Instance
        {
            get { return instanceLazy.Value; }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets the document store. </summary>
        ///
        /// <value> The document store. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public Container Container
        {
            get
            {
                BootstrapContainer();

                return container;
            }
        }

        private void BootstrapContainer()
        {
            if (container == null)
            {
                lock (lockObj)
                {
                    if (container == null)
                    {
                        Bootstrap();
                    }
                }
            }
        }

        /// <summary>
        /// Gets Instance of containing class.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        public TService GetInstance<TService>() where TService : class
        {
            return this.Container.GetInstance<TService>();
        }

        /// <summary>
        /// Starts all registirations.
        /// </summary>
        public void Start()
        {
            BootstrapContainer();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Bootstraps this object. </summary>
        ///
        /// <remarks>   Mustafa SAÇLI, 23.01.2020. </remarks>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        private void Bootstrap()
        {
            container = new Container();
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            try
            {
                var path = AppDomain.CurrentDomain.BaseDirectory;
                var businessFiles = Directory.GetFiles(path, "*.Business.dll") ?? new string[] { };

                foreach (var file in businessFiles)
                {
                    try
                    {
                        var assembly = Assembly.LoadFile(file);
                        RegisterAssembly(assembly);

                        Logger?.Info($"\"{assembly.FullName}\" is loaded.");
                        // throw new Exception("Sample Exception");
                    }
                    catch (Exception ex)
                    {
                        Logger?.Error(ex, $"File Name: {file}");
                    }
                }
            }
            catch (Exception ex2)
            {
                Logger?.Error(ex2);
            }

            container.Verify();
        }

        private void RegisterAssembly(Assembly assembly)
        {
            var registrations = assembly
                .GetExportedTypes()
                .Where(type => type.IsClass && !type.IsAbstract && type.Namespace.EndsWith(".Business"))
                .Select(q => new { service = q.GetInterfaces().LastOrDefault(), implementation = q })
                .ToList();

            if (registrations == null || registrations.Count < 1)
                return;

            //var registrations =
            //    from type in assembly.GetExportedTypes()
            //    where type.IsClass && !type.IsAbstract && type.Namespace.EndsWith(".Business")
            //    from service in type.GetInterfaces()
            //    select new { service, implementation = type };

            foreach (var reg in registrations)
            {
                container.Register(reg.service, reg.implementation, Lifestyle.Singleton);
            }
        }
    }
}
