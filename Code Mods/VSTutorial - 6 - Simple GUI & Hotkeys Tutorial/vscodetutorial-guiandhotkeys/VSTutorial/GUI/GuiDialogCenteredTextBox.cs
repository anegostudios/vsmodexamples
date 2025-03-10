using Vintagestory.API.Client;

namespace VSTutorial.GUI
{

    /// <summary>
    /// The GuiDalog class is the base for all GUI stuff.
    /// </summary>
    public class GuiDialogCenteredTextBox : GuiDialog
    {
        /// <summary>
        /// Currently this doesn't actually do anything - But the GuiDialog class requires it to be set.
        /// </summary>
        public override string ToggleKeyCombinationCode => "centeredtextbox";

        /// <summary>
        /// Gui menus don't really use any initialization features, they're just created when the GuiDialog object itself is created.
        /// </summary>
        /// <param name="capi"></param>
        public GuiDialogCenteredTextBox(ICoreClientAPI capi) : base(capi)
        {
            //Call our setup function.
            SetupDialog();
        }

        /// <summary>
        /// This setup function is essentially creating our GuiDialog, so it is ready to be shown when needed.
        /// In this particular function, and with most GuiDialogs, you should calculate the bounds (where your elements will go) first, and then create the elements.
        /// </summary>
        private void SetupDialog()
        {
            /*
             * ELementBounds are essentially a parented 2D rectangle which are used to determine the positions of UI elements.
             * In this case, we're using an autosized dialog that is centered in the center middle of the screen.
             */
            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);

            //This creates a fixed position rectangle with a size of 300x100 pixels, and 40 pixels from the top of the gui box.
            ElementBounds textBounds = ElementBounds.Fixed(0, 40, 300, 100);

            //Background boundaries. Again, just make it fit it's child elements, then add the text as a child element
            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;
            bgBounds.WithChildren(textBounds);

            //Using the composer will actually create the dialog itself using the bounds supplied.
            SingleComposer = capi.Gui.CreateCompo("CenteredTextBox", dialogBounds)
                //add the background...
                .AddShadedDialogBG(bgBounds)
                //now the title bar... We also need to register an event for when the X is clicked on the UI menu.
                .AddDialogTitleBar("A centered text box!", OnTitleBarCloseClicked)
                //and finally add the text in the text box.
                .AddStaticText("This is a piece of text at the center of your screen - Enjoy!", CairoFont.WhiteDetailText(), textBounds)
                //Calling compose is what actually 'builds' the Gui menu.
                .Compose();
        }

        /// <summary>
        /// Called when the 'X' is clicked on the title bar.
        /// You could include custom logic in here to only allow closing the menu if certain conditions are met.
        ///     Just keep in mind that pressing the 'esc' key or using the keycode will also close the UI without triggering this function.
        /// </summary>
        private void OnTitleBarCloseClicked()
        {
            TryClose();
        }
    }
}
