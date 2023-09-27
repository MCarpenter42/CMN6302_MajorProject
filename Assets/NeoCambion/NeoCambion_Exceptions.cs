namespace NeoCambion
{
    using System;
    using System.Runtime.Serialization;

    public static class ExtensionUtility
    {

    }

    public abstract class ExtendedException : Exception
    {
        /*
        public virtual string StackTrace { get; }
        public virtual string Source { get; set; }
        public virtual string Message { get; }
        public Exception InnerException { get; }
        public int HResult { get; protected set; }
        public virtual IDictionary Data { get; }
        public MethodBase TargetSite { get; }
        public virtual string HelpLink { get; set; }
        */

        public ExtendedException() { }
        public ExtendedException(string message) : base(message) { }
        public ExtendedException(string message, Exception innerException) : base(message, innerException) { }
        protected ExtendedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    namespace Unity
    {
        using UnityEngine;

        public enum ObjectSearchException { Generic = -1, Scene, Transform, Component, Attribute }
        public static class UnityExtensionUtility
        {
            public static T ExceptionIfNotFound<T>(this T obj, ObjectSearchException searchType = ObjectSearchException.Generic) where T : Component
            {
                if (obj == null)
                {
                    throw searchType switch
                    {
                        ObjectSearchException.Scene => new SceneSearchException<T>(),
                        ObjectSearchException.Transform => new TransformSearchException<T>(),
                        ObjectSearchException.Component => new ComponentSearchException<T>(),
                        ObjectSearchException.Attribute => new AttributeSearchException<T>(),
                        _ => new NotFoundException<T>()
                    };
                }
                return obj;
            }
        }

        public abstract class UnityException : ExtendedException
        {
            public UnityException() { }
            public UnityException(string message) : base(message) { }
            public UnityException(string message, Exception innerException) : base(message, innerException) { }
            protected UnityException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        }

        public class NotFoundException : UnityException
        {
            public NotFoundException() { }
            public NotFoundException(string message) : base(message) { }
            public NotFoundException(string message, Exception innerException) : base(message, innerException) { }
            protected NotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        }
        public class NotFoundException<T> : UnityException
        {
            public NotFoundException() : base("No object of type \"" + typeof(T).Name + "\" was found!") { }
            public NotFoundException(string message) : base(message) { }
            public NotFoundException(string message, Exception innerException) : base(message, innerException) { }
            protected NotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        }
        
        public class SceneSearchException : NotFoundException
        {
            public SceneSearchException() { }
            public SceneSearchException(string message) : base(message) { }
            public SceneSearchException(string message, Exception innerException) : base(message, innerException) { }
            protected SceneSearchException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        }
        public class SceneSearchException<T> : NotFoundException
        {
            public SceneSearchException() : base("No object of type \"" + typeof(T).Name + "\" was found in the loaded scene!") { }
            public SceneSearchException(string message) : base(message) { }
            public SceneSearchException(string message, Exception innerException) : base(message, innerException) { }
            protected SceneSearchException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        }

        public class TransformSearchException : NotFoundException
        {
            public TransformSearchException() { }
            public TransformSearchException(string message) : base(message) { }
            public TransformSearchException(string message, Exception innerException) : base(message, innerException) { }
            protected TransformSearchException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        }
        public class TransformSearchException<T> : NotFoundException
        {
            public TransformSearchException() : base("No child object of type \"" + typeof(T).Name + "\" was found for the specified transform!") { }
            public TransformSearchException(string message) : base(message) { }
            public TransformSearchException(string message, Exception innerException) : base(message, innerException) { }
            protected TransformSearchException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        }

        public class ComponentSearchException : NotFoundException
        {
            public ComponentSearchException() { }
            public ComponentSearchException(string message) : base(message) { }
            public ComponentSearchException(string message, Exception innerException) : base(message, innerException) { }
            protected ComponentSearchException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        }
        public class ComponentSearchException<T> : NotFoundException
        {
            public ComponentSearchException() : base("No component of type \"" + typeof(T).Name + "\" was found on the specified object!") { }
            public ComponentSearchException(string message) : base(message) { }
            public ComponentSearchException(string message, Exception innerException) : base(message, innerException) { }
            protected ComponentSearchException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        }

        public class AttributeSearchException : NotFoundException
        {
            public AttributeSearchException() { }
            public AttributeSearchException(string message) : base(message) { }
            public AttributeSearchException(string message, Exception innerException) : base(message, innerException) { }
            protected AttributeSearchException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        }
        public class AttributeSearchException<T> : NotFoundException
        {
            public AttributeSearchException() : base("The specified attribute of type \"" + typeof(T).Name + "\" did not contain a valid object!") { }
            public AttributeSearchException(string message) : base(message) { }
            public AttributeSearchException(string message, Exception innerException) : base(message, innerException) { }
            protected AttributeSearchException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        }
    }
}