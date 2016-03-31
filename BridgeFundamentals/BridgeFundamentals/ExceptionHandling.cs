using System;
using System.Diagnostics;

namespace Sodes.Bridge.Base
{
    /// <summary>Base class for all fatal bridge exceptions
    /// Allows for a debugger breakpoint on all exceptions
    /// </summary>
    public class FatalBridgeException : Exception
    {
        /// <summary>Constructor</summary>
        //public FatalBridgeException(string msg)
        //  : base(msg)
        //{
        //}

        /// <summary>Constructor</summary>
        /// <param name="msg">The error description</param>
        public FatalBridgeException(string format, params object[] args)
            : base(string.Format(format, args))
        {
#if DEBUG
            Debug.WriteLine(string.Format(format, args));
            if (Debugger.IsAttached) Debugger.Break();
#endif
        }
    }

    public class OutOfTurnException : FatalBridgeException
    {
        public OutOfTurnException(string format, params object[] args)
            : base(string.Format(format, args))
        {
        }
    }

    public class NoReportException : FatalBridgeException
    {
        public NoReportException(string format, params object[] args)
            : base(string.Format(format, args))
        {
        }
    }

    public class DeploymentException : FatalBridgeException
    {
        public DeploymentException(string format, params object[] args)
            : base(string.Format(format, args))
        {
        }
    }

    public class UnknownConventionCardException : FatalBridgeException
    {
        public UnknownConventionCardException(string format, params object[] args)
            : base(string.Format(format, args))
        {
        }
    }
}
