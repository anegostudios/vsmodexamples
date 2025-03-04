using VSTutorial.GUI;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace VSTutorial
{
    /*
     * This is the entry point for the mod. ModSystems will be automatically detected and contain a load of useful functions for loading mods.
     * Take a look at https://apidocs.vintagestory.at/api/Vintagestory.API.Common.ModSystem.html for more info.
     */    
    public class VSTutorialModSystem : ModSystem
    {

        GuiDialogCenteredTextBox centeredTextBox;

        /// <summary>
        /// This function can be used to control whether a mod should be client or server side only.
        /// In this case, since GUI's are client-side only, we'll set the mod to only load on the client.
        /// </summary>
        public override bool ShouldLoad(EnumAppSide forSide)
        {
            return forSide == EnumAppSide.Client;
        }

        /// <summary>
        /// This function is automatically called only on the client when a world is loaded.
        /// It is often used to create rendering mechanics, or create client-side commands.
        /// </summary>
        public override void StartClientSide(ICoreClientAPI api)
        {

            //This is where we register the hotkey. In essence, a keyboard combination (or just a single key) that will run a certain function.
            //Note that this uses an ID 'centeredtextbox', and it'll also create a changeable entry in the controls menu (when a world is loaded).
            api.Input.RegisterHotKey("centeredtextbox", "Opens a very centered text box", GlKeys.U, HotkeyType.GUIOrOtherControls);

            //An instance of the GUI needs to be created.
            centeredTextBox = new GuiDialogCenteredTextBox(api);
            //Using the ID, this is how you call a function when the key combination is pressed.
            api.Input.SetHotKeyHandler("centeredtextbox", OnGuiKeyCombination);
        }

        /// <summary>
        /// This function is called when our key combination, mentioned above, is pressed.
        /// </summary>
        private bool OnGuiKeyCombination(KeyCombination keyCombo)
        {
            //A rather simple toggle to open or close the GUI. 
            if (centeredTextBox.IsOpened()) centeredTextBox.TryClose();
            else centeredTextBox.TryOpen();
            //Return true to tell the game the keycode succeeded.
            return true;
        }
    }
}
