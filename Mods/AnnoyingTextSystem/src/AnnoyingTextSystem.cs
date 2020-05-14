using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace AnnoyingTextSystem
{
    public class GuiDialogAnnoyingText : GuiDialog
    {
        public override string ToggleKeyCombinationCode => "annoyingtextgui";

        public GuiDialogAnnoyingText(ICoreClientAPI capi) : base(capi)
        {
            SetupDialog();
        }

        private void SetupDialog()
        {
            // Auto-sized dialog at the center of the screen
            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);

            // Just a simple 300x300 pixel box
            ElementBounds textBounds = ElementBounds.Fixed(0, 40, 300, 100);

            // Background boundaries. Again, just make it fit it's child elements, then add the text as a child element
            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;
            bgBounds.WithChildren(textBounds);

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

    public class AnnoyingTextSystem : ModSystem
    {
        ICoreClientAPI capi;
        GuiDialog dialog;

        public override bool ShouldLoad(EnumAppSide forSide)
        {
            return forSide == EnumAppSide.Client;
        }
        
        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);

            dialog = new GuiDialogAnnoyingText(api);

            capi = api;
            capi.Input.RegisterHotKey("annoyingtextgui", "Annoys you with annoyingly centered text", GlKeys.U, HotkeyType.GUIOrOtherControls);
            capi.Input.SetHotKeyHandler("annoyingtextgui", ToggleGui);
        }

        private bool ToggleGui(KeyCombination comb)
        {
            if (dialog.IsOpened()) dialog.TryClose();
            else dialog.TryOpen();

            return true;
        }
    }
}
