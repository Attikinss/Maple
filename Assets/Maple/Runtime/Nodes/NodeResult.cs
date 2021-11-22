namespace Maple.Nodes
{
    /// <summary>The result that a node returns when it is ticked/updated.</summary>
    public enum NodeResult
    {
        /// <summary>The node has been forced to exit before completion.</summary>
        Aborted = 1,

        /// <summary>The node failed to complete its behaviour(s) because conditions could not be met.</summary>
        Failure = 2,

        /// <summary>The node has not yet reached success/failure conditions.</summary>
        Running = 3,
        
        /// <summary>The node completed its behaviour(s) successfully.</summary>
        Success = 4,
    }
}