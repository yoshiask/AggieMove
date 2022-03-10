namespace AggieMove.Services
{
    public class DialogResult
    {
        public DialogResult(DialogButtonResult button, object result)
        {
            Button = button;
            Result = result;
        }

        public DialogButtonResult Button { get; }
        public object Result { get; }
    }

    public class DialogResult<TResult> : DialogResult
    {
        public DialogResult(DialogButtonResult button, object result) : base(button, result) { }

        public new TResult Result { get; }
    }

    public enum DialogButtonResult
    {
        None, Primary, Secondary
    }
}
