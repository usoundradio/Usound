using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ninject;

namespace UsoundRadio.Common
{
    public static class Get
    {
        public static IKernel Kernel { get; set; }

        public static T A<T>()
        {
            return Kernel.Get<T>();
        }
    }
}