using System;
using System.Collections.Generic;
using System.Linq;

namespace RayTracer.Json
{
    /// <summary>
    /// Thrown when a JSON scene description is structurally valid JSON but
    /// describes an invalid scene (missing camera, unknown material reference,
    /// out-of-range values, unknown asset key, etc.). Carries every problem
    /// found so the API can return them all at once as a 400 response.
    /// </summary>
    public class SceneValidationException : Exception
    {
        /// <summary>
        /// The list of human-readable validation errors.
        /// </summary>
        public IReadOnlyList<string> Errors { get; }

        public SceneValidationException(IEnumerable<string> errors)
            : base("Scene validation failed: " + string.Join("; ", errors))
        {
            this.Errors = errors.ToList();
        }
    }
}
