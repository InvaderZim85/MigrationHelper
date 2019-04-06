namespace CmsMigrationHelper.Ui.View
{
    internal interface IUserControl
    {
        /// <summary>
        /// Gets the description of the control
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Init the control
        /// </summary>
        void InitControl();
    }
}
