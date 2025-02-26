using Vintagestory.API.Client;

namespace AnnoyingTextSystem
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
            //Call our setup.
            SetupDialog();
        }

        /// <summary>
        /// This setup function is essentially creating our GuiDialog, so it is ready to be shown when needed.
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

            //Using the composer will actually create the dialog itself using the bounds 
            SingleComposer = capi.Gui.CreateCompo("myAwesomeDialog", dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar("Heck yeah!", OnTitleBarCloseClicked)
                .AddStaticText("This is a piece of text at the center of your screen - Enjoy!", CairoFont.WhiteDetailText(), textBounds)
              .Compose()
            ;
        }

        private void OnTitleBarCloseClicked()
        {
            TryClose();
        }
    }
}
