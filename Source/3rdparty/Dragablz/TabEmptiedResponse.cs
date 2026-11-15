namespace Dragablz
{
    public enum TabEmptiedResponse
    {
        /// <summary>
        /// Allow the Window and Layout branch to be closed automatically.
        /// </summary>
        CloseWindowOrLayoutBranch,

        /// <summary>
        /// Allow the Layout branch to be closed automatically, but not the window.
        /// </summary>
        CloseLayoutBranch,

        /// <summary>
        /// The window will not be closed by the <see cref="TabablzControl"/>, probably meaning the implementor will close the window manually
        /// </summary>
        DoNothing
    }
}