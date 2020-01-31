using System;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace Swan
{
    /// <summary>
    /// Provides extension methods for <see cref="Exception"/>.
    /// </summary>
    public static class ExceptionExtensions
    {
        /// <summary>
        /// Returns a value that tells whether an <see cref="Exception"/> is of a type that
        /// we better not catch and ignore.
        /// </summary>
        /// <param name="this">The exception being thrown.</param>
        /// <returns><see langword="true"/> if <paramref name="this"/> is a critical exception;
        /// otherwise, <see langword="false"/>.</returns>
        public static bool IsCriticalException(this Exception @this)
            => @this.IsCriticalExceptionCore()
            || (@this.InnerException?.IsCriticalException() ?? false)
            || (@this is AggregateException aggregateException && aggregateException.InnerExceptions.Any(e => e.IsCriticalException()));

        /// <summary>
        /// Returns a value that tells whether an <see cref="Exception"/> is of a type that
        /// will likely cause application failure.
        /// </summary>
        /// <param name="this">The exception being thrown.</param>
        /// <returns><see langword="true"/> if <paramref name="this"/> is a fatal exception;
        /// otherwise, <see langword="false"/>.</returns>
        public static bool IsFatalException(this Exception @this)
            => @this.IsFatalExceptionCore()
            || (@this.InnerException?.IsFatalException() ?? false)
            || (@this is AggregateException aggregateException && aggregateException.InnerExceptions.Any(e => e.IsFatalException()));

        /// <summary>
        /// <para>Rethrows an already-thrown exception, preserving the stack trace of the original throw.</para>
        /// <para>This method does not return; its return type is an exception type so it can be used
        /// with <c>throw</c> semantics, e.g.: <c>throw ex.RethrowPreservingStackTrace();</c>,
        /// to let static code analysis tools that it throws instead of returning.</para>
        /// </summary>
        /// <param name="this">The exception to rethrow.</param>
        /// <returns>This method should never return; if it does, it is an indication of an internal error,
        /// so it returns an instance of <see cref="InternalErrorException"/>.</returns>
        public static InternalErrorException RethrowPreservingStackTrace(this Exception @this)
        {
            ExceptionDispatchInfo.Capture(@this).Throw();
            return SelfCheck.Failure("Reached unreachable code.");
        }

        private static bool IsCriticalExceptionCore(this Exception @this)
            => IsFatalExceptionCore(@this)
            || @this is AppDomainUnloadedException
            || @this is BadImageFormatException
            || @this is CannotUnloadAppDomainException
            || @this is InvalidProgramException
            || @this is NullReferenceException;

        private static bool IsFatalExceptionCore(this Exception @this)
            => @this is StackOverflowException
            || @this is OutOfMemoryException
            || @this is ThreadAbortException
            || @this is AccessViolationException;
    }
}