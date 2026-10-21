using Ruler.Wpf.Models;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Wpf.ViewModels
{
    public class ShowShortcutsViewModel : ViewModelBase
    {
        public ObservableCollection<ShortcutItem> Shortcuts { get; set; } = new ObservableCollection<ShortcutItem>();

        public ShowShortcutsViewModel()
        {
            LoadShortcuts();
        }

        private void LoadShortcuts()
        {
            // --- SECTION 1: CORE CONTROLS ---
            Shortcuts.Add(new ShortcutItem { IsHeader = true, Category = "Core Controls" });
            Shortcuts.Add(new ShortcutItem { KeyCombination = "Space / O", Description = "Flip Orientation", ColorHex = "#FFFFFF" });
            Shortcuts.Add(new ShortcutItem { KeyCombination = "L", Description = "Lock / Unlock Resizing", ColorHex = "#FFFFFF" });
            Shortcuts.Add(new ShortcutItem { KeyCombination = "G", Description = "Toggle Guideline Visibility", ColorHex = "#FFFFFF" });
            Shortcuts.Add(new ShortcutItem { KeyCombination = "D", Description = "Duplicate Active Ruler", ColorHex = "#FFFFFF" });
            Shortcuts.Add(new ShortcutItem { KeyCombination = "Esc", Description = "Close Active Ruler", ColorHex = "#FFFFFF" });
            Shortcuts.Add(new ShortcutItem { KeyCombination = "Ctrl + Esc", Description = "Exit All Application Instances", ColorHex = "#FF5555" });
            Shortcuts.Add(new ShortcutItem { KeyCombination = "Ctrl + U", Description = "Check or Apply Updates", ColorHex = "#55FF55" });

            // --- SECTION 2: NUDGING & SIZING ---
            Shortcuts.Add(new ShortcutItem { IsHeader = true, Category = "Nudging & Sizing" });
            Shortcuts.Add(new ShortcutItem { KeyCombination = "Arrow Keys", Description = "Ruler Nudge (10px)", ColorHex = "#FFFFFF" });
            Shortcuts.Add(new ShortcutItem { KeyCombination = "Ctrl + Arrow Keys", Description = "Fine Nudge (1px)", ColorHex = "#FFFFFF" });
            Shortcuts.Add(new ShortcutItem { KeyCombination = "Shift + Arrow Keys", Description = "Resize Ruler (1px)", ColorHex = "#FFFFFF" });
            Shortcuts.Add(new ShortcutItem { KeyCombination = "Ctrl + Shift + Arrow Keys", Description = "Resize Ruler (10px)", ColorHex = "#FFFFFF" });
            Shortcuts.Add(new ShortcutItem { KeyCombination = "Ctrl + R", Description = "Reset Ruler to Defaults", ColorHex = "#FFFFFF" });

            // --- SECTION 3: OPACITY TWEAKS ---
            Shortcuts.Add(new ShortcutItem { IsHeader = true, Category = "Opacity Tweaks" });
            Shortcuts.Add(new ShortcutItem { KeyCombination = "Page Up / Dn", Description = "Nudge Opacity 10%", ColorHex = "#FFFFFF" });
            Shortcuts.Add(new ShortcutItem { KeyCombination = "Shift + PgUp / Dn", Description = "Nudge Opacity 5%", ColorHex = "#FFFFFF", IsBold = true });
            Shortcuts.Add(new ShortcutItem { KeyCombination = "Ctrl + Page Up", Description = "Instant Snap to 100% Opacity", ColorHex = "#FFFFFF" });
            Shortcuts.Add(new ShortcutItem { KeyCombination = "Ctrl + Page Down", Description = "Instant Snap to 10% Opacity", ColorHex = "#FFFFFF" });
        }
    }
}
