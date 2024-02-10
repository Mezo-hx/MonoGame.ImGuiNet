using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Monogame.ImGuiNetFileBrowser
{
    [Flags]
    public enum ImGuiFileBrowserFlags
    {
        None = 0,
        SelectDirectory = 1 << 0, // select directory instead of regular file
        EnterNewFilename = 1 << 1, // allow user to enter new filename when selecting regular file
        NoModal = 1 << 2, // file browsing window is modal by default. specify this to use a popup window
        NoTitleBar = 1 << 3, // hide window title bar
        NoStatusBar = 1 << 4, // hide status bar at the bottom of browsing window
        CloseOnEsc = 1 << 5, // close file browser when pressing 'ESC'
        CreateNewDir = 1 << 6, // allow user to create new directory
        MultipleSelection = 1 << 7, // allow user to select multiple files. this will hide ImGuiFileBrowserFlags_EnterNewFilename
        HideRegularFiles = 1 << 8, // hide regular files when ImGuiFileBrowserFlags_SelectDirectory is enabled
        ConfirmOnEnter = 1 << 9, // confirm selection when pressnig 'ENTER'
    }

    public class imFileBrowser
    {
        public ImGuiFileBrowserFlags Flags;
        public string Title;
        public string OpenLabel;
        public bool OpenFlag;
        public bool CloseFlag;
        public bool IsOpened_;
        public bool Ok;
        public bool PosIsSet;
        public string StatusStr;
        public List<string> TypeFilters;
        public uint TypeFilterIndex;
        public bool HasAllFilter;
        public string Pwd;
        public HashSet<string> SelectedFilenames = new HashSet<string>();
        public uint RangeSelectionStart;
        public List<FileRecord> FileRecords;
        public const int INPUT_NAME_BUF_SIZE = 512;
        public string InputNameBuf;
        public string OpenNewDirLabel;
        public string NewDirNameBuf;
        public uint Drives;
        public int Width;
        public int Height;
        public int PosX;
        public int PosY;

        public imFileBrowser(ImGuiFileBrowserFlags flags = 0)
        {
            Flags = flags;
            Width = 700;
            Height = 450;
            PosX = 0;
            PosY = 0;
            OpenFlag = false;
            CloseFlag = false;
            IsOpened_ = false;
            Ok = false;
            PosIsSet = false;
            RangeSelectionStart = 0;
            InputNameBuf = new string('\0', INPUT_NAME_BUF_SIZE);
            Title = "file browser";
            Pwd = Directory.GetCurrentDirectory();
            TypeFilters = new List<string>();
            TypeFilterIndex = 0;
            HasAllFilter = false;
            Drives = GetDrivesBitMask();
        }

        public void SetWindowPos(int posx, int posy)
        {
            PosX = posx;
            PosY = posy;
            PosIsSet = true;
        }

        public void SetWindowSize(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public void SetTitle(string title)
        {
            Title = title;
            OpenLabel = Title + "##filebrowser_" + GetHashCode();
            OpenNewDirLabel = "new dir##new_dir_" + GetHashCode();
        }

        public void Open()
        {
            UpdateFileRecords();
            ClearSelected();
            StatusStr = "";
            OpenFlag = true;
            CloseFlag = false;
        }

        public void Close()
        {
            ClearSelected();
            StatusStr = "";
            CloseFlag = true;
            OpenFlag = false;
        }

        public bool IsOpened()
        {
            return IsOpened_;
        }

        public void SetPwd(string pwd)
        {
            Pwd = pwd;
            UpdateFileRecords();
        }

        public void SetTypeFilters(List<string> typeFilters)
        {
            TypeFilters = typeFilters;
            UpdateFileRecords();
        }

        public void SetCurrentTypeFilterIndex(int index)
        {
            TypeFilterIndex = (uint)index;
            UpdateFileRecords();
        }

        public void SetInputName(string input)
        {
            InputNameBuf = input;
        }

        public void SetOkButtonLabel(string label)
        {
            OpenLabel = label;
        }

        public void SetCancelButtonLabel(string label)
        {
            OpenNewDirLabel = label;
        }

        public void DisplayClean()
        {
            ImGui.PushID(GetHashCode());
            if (OpenFlag)
            {
                ImGui.OpenPopup(OpenLabel);
            }
            IsOpened_ = false;

            if (OpenFlag && (Flags & ImGuiFileBrowserFlags.NoModal) != 0)
            {
                if (PosIsSet)
                {
                    ImGui.SetNextWindowPos(new System.Numerics.Vector2(PosX, PosY));
                }
                ImGui.SetNextWindowSize(new System.Numerics.Vector2(Width, Height));
            }
            else
            {
                if (PosIsSet)
                {
                    ImGui.SetNextWindowPos(new System.Numerics.Vector2(PosX, PosY), ImGuiCond.FirstUseEver);
                }
                ImGui.SetNextWindowSize(new System.Numerics.Vector2(Width, Height), ImGuiCond.FirstUseEver);
            }
            if ((Flags & ImGuiFileBrowserFlags.NoModal) != 0)
            {
                if (!ImGui.BeginPopup(OpenLabel))
                {
                    return;
                }
            }
            else
            {
                if (!ImGui.BeginPopupModal(OpenLabel, ref IsOpened_, (Flags & ImGuiFileBrowserFlags.NoTitleBar) != 0 ? ImGuiWindowFlags.NoTitleBar : 0))
                {
                    return;
                }
            }
            IsOpened_ = true;

            ImGui.PushItemWidth(4 * ImGui.GetFontSize());
            if (ImGui.BeginCombo("##select_drive", Pwd[0] + ":"))
            {
                for (int i = 0; i < 26; ++i)
                {
                    if ((Drives & (1 << i)) == 0)
                    {
                        continue;
                    }
                    char driveCh = (char)('A' + i);
                    string selectableStr = driveCh + ":";
                    bool selected = Pwd[0] == driveCh;
                    if (ImGui.Selectable(selectableStr, selected) && !selected)
                    {
                        string newPwd = driveCh + ":\\";
                        SetPwd(newPwd);
                    }
                }
                ImGui.EndCombo();
            }
            ImGui.PopItemWidth();

            int secIdx = 0, newPwdLastSecIdx = -1;
            foreach (var sec in Pwd)
            {
                if (secIdx == 1)
                {
                    ++secIdx;
                    continue;
                }
                ImGui.PushID(secIdx);
                if (secIdx > 0)
                {
                    ImGui.SameLine();
                }
                if (ImGui.SmallButton(sec.ToString()))
                {
                    newPwdLastSecIdx = secIdx;
                }
                ImGui.PopID();
                ++secIdx;
            }
            if (newPwdLastSecIdx >= 0)
            {
                int i = 0;
                string newPwd = "";
                foreach (var sec in Pwd)
                {
                    if (i++ > newPwdLastSecIdx)
                    {
                        break;
                    }
                    newPwd += sec;
                }
                if (newPwdLastSecIdx == 0)
                {
                    newPwd += "\\";
                }
                SetPwd(newPwd);
            }
            ImGui.SameLine();
            if (ImGui.SmallButton("*"))
            {
                UpdateFileRecords();
                HashSet<string> newSelectedFilenames = new HashSet<string>();
                foreach (var name in SelectedFilenames)
                {
                    var it = FileRecords.FindIndex(record => name == record.Name);
                    if (it != -1)
                    {
                        newSelectedFilenames.Add(name);
                    }
                }
                if (!string.IsNullOrEmpty(InputNameBuf))
                {
                    newSelectedFilenames.Add(InputNameBuf);
                }
            }
            bool focusOnInputText = false;
            if (!string.IsNullOrEmpty(NewDirNameBuf))
            {
                ImGui.SameLine();
                if (ImGui.SmallButton("+"))
                {
                    ImGui.OpenPopup(OpenNewDirLabel);
                    NewDirNameBuf = "";
                }
                if (ImGui.BeginPopup(OpenNewDirLabel))
                {
                    ImGui.InputText("name", ref NewDirNameBuf, INPUT_NAME_BUF_SIZE);
                    focusOnInputText |= ImGui.IsItemFocused();
                    ImGui.SameLine();
                    if (ImGui.Button("ok") && !string.IsNullOrEmpty(NewDirNameBuf))
                    {
                        ImGui.CloseCurrentPopup();
                        if (Directory.CreateDirectory(Path.Combine(Pwd, NewDirNameBuf)) != null)
                        {
                            UpdateFileRecords();
                        }
                        else
                        {
                            StatusStr = "failed to create " + NewDirNameBuf;
                        }
                    }
                    ImGui.EndPopup();
                }
            }

            float reserveHeight = ImGui.GetFrameHeightWithSpacing();
            if ((Flags & ImGuiFileBrowserFlags.SelectDirectory) == 0 && (Flags & ImGuiFileBrowserFlags.EnterNewFilename) != 0)
            {
                reserveHeight += ImGui.GetFrameHeightWithSpacing();
            }
            ImGui.BeginChild("ch", new System.Numerics.Vector2(0, -reserveHeight), ImGuiChildFlags.Border, (Flags & ImGuiFileBrowserFlags.NoModal) != 0 ? ImGuiWindowFlags.AlwaysHorizontalScrollbar : 0);
            foreach (var rsc in FileRecords)
            {
                if (!rsc.IsDir && (Flags & ImGuiFileBrowserFlags.HideRegularFiles) != 0)
                {
                    continue;
                }
                if (!rsc.IsDir && !IsExtensionMatched(rsc.Extension))
                {
                    continue;
                }
                if (!string.IsNullOrEmpty(rsc.Name) && rsc.Name[0] == '$')
                {
                    continue;
                }
                bool selected = SelectedFilenames.Contains(rsc.Name);
                if (ImGui.Selectable(rsc.ShowName, selected, ImGuiSelectableFlags.DontClosePopups))
                {
                    bool wantDir = (Flags & ImGuiFileBrowserFlags.SelectDirectory) != 0;
                    bool canSelect = rsc.Name != ".." && rsc.IsDir == wantDir;
                    bool rangeSelect = canSelect && ImGui.GetIO().KeyShift && RangeSelectionStart < FileRecords.Count && (Flags & ImGuiFileBrowserFlags.MultipleSelection) != 0 && ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows);
                    bool multiSelect = !rangeSelect && ImGui.GetIO().KeyCtrl && (Flags & ImGuiFileBrowserFlags.MultipleSelection) != 0 && ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows);
                    if (rangeSelect)
                    {
                        uint first = Math.Min(RangeSelectionStart, (uint)FileRecords.IndexOf(rsc));
                        uint last = Math.Max(RangeSelectionStart, (uint)FileRecords.IndexOf(rsc));
                        SelectedFilenames.Clear();
                        for (uint i = first; i <= last; ++i)
                        {
                            if (FileRecords[(int)i].IsDir != wantDir)
                            {
                                continue;
                            }
                            if (!wantDir && !IsExtensionMatched(FileRecords[(int)i].Extension))
                            {
                                continue;
                            }
                            SelectedFilenames.Add(FileRecords[(int)i].Name);
                        }
                    }
                    else if (selected)
                    {
                        if (!multiSelect)
                        {
                            SelectedFilenames = new HashSet<string> { rsc.Name };
                            RangeSelectionStart = (uint)FileRecords.IndexOf(rsc);
                        }
                        else
                        {
                            SelectedFilenames.Remove(rsc.Name);
                        }
                        InputNameBuf = "";
                    }
                    else if (canSelect)
                    {
                        if (multiSelect)
                        {
                            SelectedFilenames.Add(rsc.Name);
                        }
                        else
                        {
                            SelectedFilenames = new HashSet<string> { rsc.Name };
                        }
                        if ((Flags & ImGuiFileBrowserFlags.SelectDirectory) == 0)
                        {
                            InputNameBuf = rsc.Name;
                        }
                        RangeSelectionStart = (uint)FileRecords.IndexOf(rsc);
                    }
                    else
                    {
                        if (!multiSelect)
                        {
                            SelectedFilenames.Clear();
                        }
                    }
                }
                if (ImGui.IsItemClicked(0) && ImGui.IsMouseDoubleClicked(0))
                {
                    if (rsc.IsDir)
                    {
                        SetPwd(rsc.Name != ".." ? Path.Combine(Pwd, rsc.Name) : Path.GetDirectoryName(Pwd));
                    }
                    else if ((Flags & ImGuiFileBrowserFlags.SelectDirectory) == 0)
                    {
                        SelectedFilenames = new HashSet<string> { rsc.Name };
                        Ok = true;
                        ImGui.CloseCurrentPopup();
                    }
                }
            }
            ImGui.EndChild();
            if ((Flags & ImGuiFileBrowserFlags.SelectDirectory) == 0 && (Flags & ImGuiFileBrowserFlags.EnterNewFilename) != 0)
            {
                ImGui.PushID(GetHashCode());
                if (ImGui.InputText("", ref InputNameBuf, INPUT_NAME_BUF_SIZE) && !string.IsNullOrEmpty(InputNameBuf))
                {
                    SelectedFilenames = new HashSet<string> { InputNameBuf };
                }
                focusOnInputText |= ImGui.IsItemFocused();
                ImGui.PopID();
            }
            if (!focusOnInputText)
            {
                bool selectAll = (Flags & ImGuiFileBrowserFlags.MultipleSelection) != 0 && ImGui.IsKeyPressed(ImGuiKey.A) && (ImGui.IsKeyDown(ImGuiKey.LeftCtrl) || ImGui.IsKeyDown(ImGuiKey.RightCtrl));
                if (selectAll)
                {
                    SelectedFilenames = new HashSet<string>(FileRecords.Where(rsc => rsc.IsDir == ((Flags & ImGuiFileBrowserFlags.SelectDirectory) != 0) && (Flags & ImGuiFileBrowserFlags.HideRegularFiles) == 0 && IsExtensionMatched(rsc.Extension)).Select(rsc => rsc.Name));
                }
            }
            ImGui.EndPopup();
            ImGui.PopID();
        }

        public void Display()
        {
            ImGui.PushID(GetHashCode());
            if (OpenFlag)
            {
                ImGui.OpenPopup(OpenLabel);
            }
            IsOpened_ = false;

            if (OpenFlag && (Flags & ImGuiFileBrowserFlags.NoModal) != 0)
            {
                if (PosIsSet)
                {
                    ImGui.SetNextWindowPos(new System.Numerics.Vector2(PosX, PosY));
                }
                ImGui.SetNextWindowSize(new System.Numerics.Vector2(Width, Height));
            }
            else
            {
                if (PosIsSet)
                {
                    ImGui.SetNextWindowPos(new System.Numerics.Vector2(PosX, PosY), ImGuiCond.FirstUseEver);
                }
                ImGui.SetNextWindowSize(new System.Numerics.Vector2(Width, Height), ImGuiCond.FirstUseEver);
            }

            if ((Flags & ImGuiFileBrowserFlags.NoModal) != 0)
            {
                if (!ImGui.BeginPopup(OpenLabel))
                {
                    return;
                }
            }
            else
            {
                if (!ImGui.BeginPopupModal(OpenLabel, ref IsOpened_, (Flags & ImGuiFileBrowserFlags.NoTitleBar) != 0 ? ImGuiWindowFlags.NoTitleBar : 0))
                {
                    return;
                }
            }
            
            IsOpened_ = true;
            
            char currentDrive = (char)Pwd[0];
            char[] driveStr = { currentDrive, ':', '\0' };
            ImGui.PushItemWidth(4 * ImGui.GetFontSize());
            
            if (ImGui.BeginCombo("##select_drive", driveStr))
            {
                for (int i = 0; i < 26; ++i)
                {
                    if ((Drives & (1 << i)) == 0)
                    {
                        continue;
                    }
                    char driveCh = (char)('A' + i);
                    char[] selectableStr = { driveCh, ':', '\0' };
                    bool selected = currentDrive == driveCh;
                    if (ImGui.Selectable(selectableStr, selected) && !selected)
                    {
                        char[] driveChars = { driveCh, ':', '\\', '\0' };
                        SetPwd(driveChars.ToString());
                    }
                }

                ImGui.EndCombo();
            }

            ImGui.PopItemWidth();
            ImGui.SameLine();

            Pwd = Path.GetFullPath(Pwd); // Convert to absolute path if not already
            var sections = Pwd.Split(Path.DirectorySeparatorChar); // Split the path into sections
            int secIdx = 0;
            int newPwdLastSecIdx = -1;

            foreach (var sec in sections)
            {
                if (string.IsNullOrEmpty(sec) && secIdx == 0)
                {
                    // This handles the root directory in Unix-like systems or the drive letter in Windows.
                    ImGui.PushID(secIdx);
                    if (ImGui.SmallButton(Path.DirectorySeparatorChar.ToString()))
                    {
                        newPwdLastSecIdx = secIdx;
                    }
                    ImGui.PopID();
                }
                else
                {
                    ImGui.PushID(secIdx);
                    if (secIdx > 0)
                    {
                        ImGui.SameLine();
                    }
                    if (ImGui.SmallButton(sec))
                    {
                        newPwdLastSecIdx = secIdx;
                    }
                    ImGui.PopID();
                }
                ++secIdx;
            }

            if (newPwdLastSecIdx >= 0)
            {
                string newPwd2 = string.Join(Path.DirectorySeparatorChar.ToString(), sections, 0, newPwdLastSecIdx + 1);
                if (newPwdLastSecIdx == 0 && !Pwd.StartsWith(Path.DirectorySeparatorChar.ToString()))
                {
                    // Add a trailing separator to root drives on Windows
                    newPwd2 += Path.DirectorySeparatorChar;
                }
                SetPwd(newPwd2);
            }

            ImGui.SameLine();
            
            if (ImGui.SmallButton("*"))
            {
                UpdateFileRecords();
                HashSet<string> newSelectedFilenames = new HashSet<string>();
                foreach (var name in SelectedFilenames)
                {
                    var it = FileRecords.FindIndex(record => name == record.Name);
                    if (it != -1)
                    {
                        newSelectedFilenames.Add(name);
                    }
                }
                if (!string.IsNullOrEmpty(InputNameBuf))
                {
                    newSelectedFilenames.Add(InputNameBuf);
                }
            }

            bool focusOnInputText = false;
            if (!string.IsNullOrEmpty(NewDirNameBuf))
            {
                ImGui.SameLine();
                if (ImGui.SmallButton("+"))
                {
                    ImGui.OpenPopup(OpenNewDirLabel);
                    NewDirNameBuf = "";
                }
                if (ImGui.BeginPopup(OpenNewDirLabel))
                {
                    ImGui.InputText("name", ref NewDirNameBuf, INPUT_NAME_BUF_SIZE);
                    focusOnInputText |= ImGui.IsItemFocused();
                    ImGui.SameLine();
                    if (ImGui.Button("ok") && !string.IsNullOrEmpty(NewDirNameBuf))
                    {
                        ImGui.CloseCurrentPopup();
                        if (Directory.CreateDirectory(Path.Combine(Pwd, NewDirNameBuf)) != null)
                        {
                            UpdateFileRecords();
                        }
                        else
                        {
                            StatusStr = "failed to create " + NewDirNameBuf;
                        }
                    }
                    ImGui.EndPopup();
                }
            }

            
            float reserveHeight = ImGui.GetFrameHeightWithSpacing();
            System.IO.DirectoryInfo newPwd = null; bool setNewPwd = false;
            if ((Flags & ImGuiFileBrowserFlags.SelectDirectory) == 0 && (Flags & ImGuiFileBrowserFlags.EnterNewFilename) != 0)
            {
                reserveHeight += ImGui.GetFrameHeightWithSpacing();
            }

            {
                ImGui.BeginChild("ch", new System.Numerics.Vector2(0, -reserveHeight), ImGuiChildFlags.Border, (Flags & ImGuiFileBrowserFlags.NoModal) != 0 ? ImGuiWindowFlags.AlwaysHorizontalScrollbar : 0);
                bool shouldHideRegularFiles = (Flags & ImGuiFileBrowserFlags.HideRegularFiles) != 0 && (Flags & ImGuiFileBrowserFlags.SelectDirectory) != 0;

                for (int rscIndex = 0; rscIndex < FileRecords.Count; ++rscIndex)
                {
                    var rsc = FileRecords[rscIndex];
                    if (!rsc.IsDir && shouldHideRegularFiles)
                    {
                        continue;
                    }
                    if (!rsc.IsDir && !IsExtensionMatched(rsc.Extension))
                    {
                        continue;
                    }
                    if (!String.IsNullOrEmpty(rsc.Name) && rsc.Name[0] == '$')
                    {
                        continue;
                    }
                    bool selected = SelectedFilenames.Contains(rsc.Name);
                    if (ImGui.Selectable(rsc.ShowName, selected, ImGuiSelectableFlags.DontClosePopups))
                    {
                        bool wantDir = (Flags & ImGuiFileBrowserFlags.SelectDirectory) != 0;
                        bool canSelect = rsc.Name != ".." && rsc.IsDir == wantDir;
                        bool rangeSelect = canSelect && ImGui.GetIO().KeyShift && RangeSelectionStart < FileRecords.Count && (Flags & ImGuiFileBrowserFlags.MultipleSelection) != 0 && ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows);
                        bool multiSelect = !rangeSelect && ImGui.GetIO().KeyCtrl && (Flags & ImGuiFileBrowserFlags.MultipleSelection) != 0 && ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows);
                        if (rangeSelect)
                        {


                            int first = Math.Min((int)RangeSelectionStart, rscIndex);
                            int last = Math.Max((int)RangeSelectionStart, rscIndex);
                            SelectedFilenames.Clear();
                            for (int i = first; i <= last; ++i)
                            {
                                if (FileRecords[i].IsDir != wantDir)
                                {
                                    continue;
                                }
                                if (!wantDir && !IsExtensionMatched(FileRecords[i].Extension))
                                {
                                    continue;
                                }
                                SelectedFilenames.Add(FileRecords[i].Name);
                            }
                        }
                        else if (selected)
                        {
                            if (!multiSelect)
                            {
                                SelectedFilenames = new HashSet<string> { rsc.Name };
                                RangeSelectionStart = (uint)rscIndex;
                            }
                            else
                            {
                                SelectedFilenames.Remove(rsc.Name);
                            }
                            InputNameBuf = "";
                        }
                        else if (canSelect)
                        {
                            if (multiSelect)
                            {
                                SelectedFilenames.Add(rsc.Name);
                            }
                            else
                            {
                                SelectedFilenames = new HashSet<string> { rsc.Name };
                            }

                            if ((Flags & ImGuiFileBrowserFlags.SelectDirectory) == 0)
                            {
                                InputNameBuf = rsc.Name;
                            }
                            RangeSelectionStart = (uint)rscIndex;
                        }
                        else
                        {
                            if (!multiSelect)
                            {
                                SelectedFilenames.Clear();
                            }
                        }
                    }
                    if (ImGui.IsItemClicked(0) && ImGui.IsMouseDoubleClicked(0))
                    {
                        if (rsc.IsDir)
                        {
                            setNewPwd = true;
                            newPwd = new System.IO.DirectoryInfo(rsc.Name != ".." ? Path.Combine(Pwd, rsc.Name) : Path.GetDirectoryName(Pwd));
                        }
                        else if ((Flags & ImGuiFileBrowserFlags.SelectDirectory) == 0)
                        {
                            SelectedFilenames = new HashSet<string> { rsc.Name };
                            Ok = true;
                            ImGui.CloseCurrentPopup();
                        }
                    }
                }

                ImGui.EndChild();
            }
            
            if (setNewPwd)
            {
                SetPwd(newPwd.FullName);
            }

            if ((Flags & ImGuiFileBrowserFlags.SelectDirectory) == 0 && (Flags & ImGuiFileBrowserFlags.EnterNewFilename) != 0)
            {
                ImGui.PushID(GetHashCode());
                ImGui.PushItemWidth(-1);
                if (ImGui.InputText("", ref InputNameBuf, INPUT_NAME_BUF_SIZE) && InputNameBuf[0] != '\0')
                {
                    SelectedFilenames = new HashSet<string> { InputNameBuf };
                }
                focusOnInputText |= ImGui.IsItemFocused();
                ImGui.PopItemWidth();
                ImGui.PopID();
            }

            if (!focusOnInputText)
            {
                bool selectAll = (Flags & ImGuiFileBrowserFlags.MultipleSelection) != 0 && ImGui.IsKeyPressed(ImGuiKey.A) && (ImGui.IsKeyDown(ImGuiKey.LeftCtrl) || ImGui.IsKeyDown(ImGuiKey.RightCtrl));
                if (selectAll)
                {
                    bool needDir = (Flags & ImGuiFileBrowserFlags.SelectDirectory) != 0;
                    SelectedFilenames.Clear();
                    for (int i = 1; i < FileRecords.Count; ++i)
                    {
                        var record = FileRecords[i];
                        if (record.IsDir == needDir && (needDir || IsExtensionMatched(record.Extension)))
                        {
                            SelectedFilenames.Add(record.Name);
                        }
                    }
                }
            }
            bool enter = (Flags & ImGuiFileBrowserFlags.ConfirmOnEnter) != 0 && ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows) && ImGui.IsKeyPressed(ImGuiKey.Enter);
            if ((Flags & ImGuiFileBrowserFlags.SelectDirectory) == 0)
            {
                if ((ImGui.Button(" ok ") || enter) && SelectedFilenames.Count > 0)
                {
                    Ok = true;
                    ImGui.CloseCurrentPopup();
                }
            }
            else
            {
                if (ImGui.Button(" ok ") || enter)
                {
                    Ok = true;
                    ImGui.CloseCurrentPopup();
                }
            }
            ImGui.SameLine();
            bool shouldExit = ImGui.Button("cancel") || CloseFlag || ((Flags & ImGuiFileBrowserFlags.CloseOnEsc) != 0 && ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows) && ImGui.IsKeyPressed(ImGuiKey.Escape));
            if (shouldExit)
            {
                ImGui.CloseCurrentPopup();
            }

            if (!String.IsNullOrEmpty(StatusStr) && (Flags & ImGuiFileBrowserFlags.NoStatusBar) == 0)
            {
                ImGui.SameLine();
                ImGui.Text(StatusStr);
            }
            if (TypeFilters.Count > 0)
            {
                ImGui.SameLine();
                ImGui.PushItemWidth(8 * ImGui.GetFontSize());
                if (ImGui.BeginCombo("##type_filters", TypeFilters[(int)TypeFilterIndex]))
                {
                    for (int i = 0; i < TypeFilters.Count; ++i)
                    {
                        bool selected = i == TypeFilterIndex;
                        if (ImGui.Selectable(TypeFilters[i], selected) && !selected)
                        {
                            TypeFilterIndex = (uint)i;
                        }
                    }

                    ImGui.EndCombo();
                }

                ImGui.PopItemWidth();
            }

            if(OpenFlag)
            {
                OpenFlag = false;
                CloseFlag = false;
                ImGui.PopID();
            }
        }

        public void ClearSelected()
        {
            SelectedFilenames.Clear();
        }

        private void UpdateFileRecords()
        {
            FileRecords = new List<FileRecord>();
            if ((Flags & ImGuiFileBrowserFlags.SelectDirectory) != 0)
            {
                FileRecords.Add(new FileRecord { IsDir = true, Name = "..", ShowName = ".." });
            }
            try
            {
                foreach (var dir in Directory.GetDirectories(Pwd))
                {
                    FileRecords.Add(new FileRecord { IsDir = true, Name = Path.GetFileName(dir), ShowName = Path.GetFileName(dir) });
                }
                foreach (var file in Directory.GetFiles(Pwd))
                {
                    FileRecords.Add(new FileRecord { IsDir = false, Name = Path.GetFileName(file), ShowName = Path.GetFileName(file), Extension = Path.GetExtension(file) });
                }
            }
            catch (Exception e)
            {
                StatusStr = e.Message;
            }
            if (TypeFilters.Count > 0)
            {
                FileRecords = FileRecords.Where(rsc => rsc.IsDir || IsExtensionMatched(rsc.Extension)).ToList();
            }
            if (HasAllFilter)
            {
                FileRecords.Insert(0, new FileRecord { IsDir = true, Name = "*", ShowName = "*" });
            }
            FileRecords.Sort((a, b) =>
            {
                if (a.IsDir != b.IsDir)
                {
                    return a.IsDir ? -1 : 1;
                }
                return string.Compare(a.Name, b.Name, StringComparison.Ordinal);
            });
        }

        private bool IsExtensionMatched(string extension)
        {
            if (TypeFilters.Count == 0)
            {
                return true;
            }
            if (string.IsNullOrEmpty(extension))
            {
                return HasAllFilter;
            }
            return TypeFilters.Contains(extension);
        }

        private static uint GetDrivesBitMask()
        {
            uint drives = 0;
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.DriveType == DriveType.Fixed)
                {
                    drives |= 1u << (drive.Name[0] - 'A');
                }
            }
            return drives;
        }
    }

    public struct FileRecord
    {
        public bool IsDir;
        public string Name;
        public string ShowName;
        public string Extension;
    }
}

