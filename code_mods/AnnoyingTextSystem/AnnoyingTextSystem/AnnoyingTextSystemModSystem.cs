using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace AnnoyingTextSystem
{
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
