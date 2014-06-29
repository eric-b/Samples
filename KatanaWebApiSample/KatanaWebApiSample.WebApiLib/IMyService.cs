using System;

namespace KatanaWebApiSample.WebApiLib
{
    /// <summary>
    /// The aim of this interface is to demonstrate the use of IoC in this sample.
    /// </summary>
    public interface IMyService
    {
        string SayHello(string name);
    }
}