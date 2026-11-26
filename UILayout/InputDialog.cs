using System;
using System.Collections.Generic;

namespace UILayout
{
    public class DialogInput
    {
        public bool WaitForRelease { get; set; }
        public string Text { get; set; }
        public Action Action { get; set; }
        public bool CloseOnInput { get; set; }

        /// <summary>The dialog's default input (such as Ok): its button has the accent border.</summary>
        public bool IsPrimary { get; set; }

        public DialogInput()
        {
            CloseOnInput = false;
            WaitForRelease = true;
        }
    }

    /// <summary>
    /// A popup dialog: a <see cref="Frame"/> holding its contents over a row of buttons, one for each
    /// <see cref="DialogInput"/>. Centered when shown without an anchor. In the popup style, as all popups
    /// (<see cref="Frame.SetPopupStyle"/>: the accent border, in a soft shadow), setting it apart from what's behind it.
    /// </summary>
    public class InputDialog : Frame, IPopup
    {
        public Action CloseAction { get { return inputStack.CloseAction; } set { inputStack.CloseAction = value; } }

        public float ButtonSpacing
        {
            get { return inputStack.InputContainer.ChildSpacing; }
            set { inputStack.InputContainer.ChildSpacing = value; }
        }

        public EHorizontalAlignment ButtonAlignment
        {
            get { return inputStack.HorizontalAlignment; }
            set { inputStack.HorizontalAlignment = value; }
        }

        VerticalStack stack;
        UIElementWrapper wrapper;
        protected DialogInputStack inputStack;

        public InputDialog()
            : this(null, null)
        {
        }

        public InputDialog(UIElement contents)
            : this(contents, null)
        {
        }

        public InputDialog(UIElement contents, params DialogInput[] inputs)
        {
            HorizontalAlignment = EHorizontalAlignment.Center;
            VerticalAlignment = EVerticalAlignment.Center;
            SetPopupStyle();

            ContentPadding = new LayoutPadding(12);

            stack = new VerticalStack();
            stack.HorizontalAlignment = EHorizontalAlignment.Stretch;
            stack.VerticalAlignment = EVerticalAlignment.Stretch;

            stack.ChildSpacing = 12;
            Content = stack;

            wrapper = new UIElementWrapper();
            wrapper.HorizontalAlignment = EHorizontalAlignment.Stretch;
            wrapper.VerticalAlignment = EVerticalAlignment.Stretch;
            stack.Children.Add(wrapper);

            wrapper.Child = contents;

            inputStack = new DialogInputStack(this, new HorizontalStack { HorizontalAlignment = EHorizontalAlignment.Center, ChildSpacing = 8 }, inputs) { HorizontalAlignment = EHorizontalAlignment.Center, VerticalAlignment = EVerticalAlignment.Bottom };
            stack.Children.Add(inputStack);
        }

        public virtual void Opened()
        {
        }

        public void SetContents(UIElement contents)
        {
            wrapper.Child = contents;
        }

        public void AddInput(DialogInput input)
        {
            inputStack.AddInput(input);
        }

        public virtual void Exit()
        {
            if (CloseAction != null)
            {
                CloseAction();
            }
        }
    }

    public class DialogInputStack : UIElementWrapper
    {
        public Action CloseAction { get; set; }

        public ListUIElement InputContainer { get; private set; }

        IPopup hostElement;
        List<DialogInput> inputs = new List<DialogInput>();
        int selectedInput = 0;

        public DialogInputStack(IPopup hostElement, ListUIElement inputContainer, params DialogInput[] inputs)
        {
            this.hostElement = hostElement;
            this.InputContainer = inputContainer;

            Child = inputContainer;

            if (inputs != null)
            {
                foreach (DialogInput input in inputs)
                {
                    AddInput(input);
                }
            }
        }

        public void AddInput(DialogInput input)
        {
            inputs.Add(input);

            TextButton button = new TextButton(input.Text)
            {
                Primary = input.IsPrimary
            };

            Action action = delegate { DoAction(input); };

            if (input.WaitForRelease)
                button.ClickAction = action;
            else
                button.PressAction = action;

            InputContainer.Children.Add(button);

            SelectInput(0);
        }

        public void SelectInput(int pos)
        {
            selectedInput = pos;
        }

        bool DoAction(DialogInput input)
        {
            bool handled = false;

            if (input.CloseOnInput)
            {
                if ((hostElement != null) && (hostElement.CloseAction != null))
                    hostElement.CloseAction();

                handled = true;
            }

            if (input.Action != null)
            {
                input.Action();

                handled = true;
            }

            return handled;
        }
    }
}
