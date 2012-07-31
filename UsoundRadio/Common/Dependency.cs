using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UsoundRadio.Utils;
using Ninject;

namespace UsoundRadio.Common
{
    public static class Dependency
    {
        public static IKernel Kernel { get; set; }

        public static T Get<T>()
        {
            return Kernel.Get<T>();
        }
    }
}