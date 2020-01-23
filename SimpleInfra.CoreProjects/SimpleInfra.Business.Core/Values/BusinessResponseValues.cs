namespace SimpleInfra.Business.Core
{
    /// <summary>
    /// Defines the <see cref="BusinessResponseValues" />
    /// </summary>
    public sealed class BusinessResponseValues
    {
        /// <summary>
        /// Defines the NullIdValue
        /// </summary>
        public static readonly int NullIdValue = -100;

        /// <summary>
        /// Defines the NullEntityValue
        /// </summary>
        public static readonly int NullEntityValue = -101;

        /// <summary>
        /// Defines the NullDtoValue
        /// </summary>
        public static readonly int NullDtoValue = -102;

        /// <summary>
        /// Defines the ValidationErrorResult
        /// </summary>
        public static readonly int ValidationErrorResult = -200;

        /// <summary>
        /// Defines the InternalError
        /// </summary>
        public static readonly int InternalError = -500;
    }
}
