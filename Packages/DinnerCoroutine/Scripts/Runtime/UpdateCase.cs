namespace CANStudio.DinnerCoroutine
{
    internal enum UpdateCase
    {
        /// <summary>
        ///     Set none to tell <see cref="CoroutinePool"/> to remove this coroutine.
        /// </summary>
        None,
        Update,
        FixedUpdate,
        OnPostRender
    }
}