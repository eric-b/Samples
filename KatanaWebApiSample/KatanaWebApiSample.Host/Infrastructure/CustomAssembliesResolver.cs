using System;
using System.Collections.Generic;
using System.Web.Http.Dispatcher;
using KatanaWebApiSample.WebApiLib.Controllers;

namespace KatanaWebApiSample.Host.Infrastructure
{
    /// <summary>
    /// <para>Extends <see cref="DefaultAssembliesResolver"/> 
    /// to expose controllers of third party assemblies 
    /// in the WebApi framework.</para>
    /// </summary>
    internal class CustomAssembliesResolver : DefaultAssembliesResolver
    {
        public override ICollection<System.Reflection.Assembly> GetAssemblies()
        {
            var assemblies = base.GetAssemblies();
            
            // Interestingly, if an assembly is referenced more than 1 time in the collection,
            // an exception can be raised on the resolution of a controller in this assembly 
            // (to me, it's a bug in WebApi 2.1).
            var customControllersAssembly = typeof(SayHelloController).Assembly;
            if (!assemblies.Contains(customControllersAssembly))
                assemblies.Add(customControllersAssembly);

            return assemblies;
        }
    }
}