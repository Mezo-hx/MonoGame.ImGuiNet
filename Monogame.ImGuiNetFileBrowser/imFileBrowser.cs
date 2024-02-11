using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using System.Numerics;

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
        public Vector4 folderColor = new Vector4(0.7f, 0.7f, 0.7f, 1.0f); // Yellow color for folders
        public Vector4 fileColor = new Vector4(0.0f, 0.8f, 0.8f, 1.0f); // White color for files

        public class FileTypeColorMap
        {
            public static Dictionary<string, System.Numerics.Vector4> ExtensionToColor = new Dictionary<string, System.Numerics.Vector4>
            {
                // Document and Text Files
                { ".txt", new System.Numerics.Vector4(0.75f, 0.75f, 0.75f, 1.0f) }, // Light Grey
                { ".doc", new System.Numerics.Vector4(0.2f, 0.2f, 1.0f, 1.0f) }, // Dark Blue
                { ".docx", new System.Numerics.Vector4(0.2f, 0.2f, 1.0f, 1.0f) }, // Dark Blue
                { ".pdf", new System.Numerics.Vector4(1.0f, 0.3f, 0.3f, 1.0f) }, // Red
                { ".md", new System.Numerics.Vector4(0.9f, 0.6f, 0.2f, 1.0f) }, // Orange-Brown
        
                // Image Files
                { ".png", new System.Numerics.Vector4(1.0f, 0.5f, 0.0f, 1.0f) }, // Orange
                { ".jpg", new System.Numerics.Vector4(1.0f, 0.5f, 0.0f, 1.0f) }, // Orange
                { ".jpeg", new System.Numerics.Vector4(1.0f, 0.5f, 0.0f, 1.0f) }, // Orange
                { ".gif", new System.Numerics.Vector4(0.6f, 1.0f, 0.6f, 1.0f) }, // Light Green
                { ".bmp", new System.Numerics.Vector4(0.5f, 0.25f, 0.0f, 1.0f) }, // Brown
        
                // Programming and Source Code Files
                { ".cs", new System.Numerics.Vector4(0.0f, 0.6f, 0.0f, 1.0f) }, // Green
                { ".cpp", new System.Numerics.Vector4(0.0f, 0.5f, 0.5f, 1.0f) }, // Teal
                { ".py", new System.Numerics.Vector4(0.4f, 0.6f, 0.8f, 1.0f) }, // Light Blue
                { ".java", new System.Numerics.Vector4(0.7f, 0.4f, 0.2f, 1.0f) }, // Orange-Brown
                { ".ts", new System.Numerics.Vector4(0.2f, 0.7f, 0.9f, 1.0f) }, // Sky Blue
        
                // Audio and Video Files
                { ".mp3", new System.Numerics.Vector4(1.0f, 0.0f, 1.0f, 1.0f) }, // Magenta
                { ".wav", new System.Numerics.Vector4(0.5f, 0.0f, 0.5f, 1.0f) }, // Purple
                { ".mp4", new System.Numerics.Vector4(0.9f, 0.2f, 0.2f, 1.0f) }, // Reddish
                { ".avi", new System.Numerics.Vector4(0.2f, 0.2f, 0.9f, 1.0f) }, // Blue
                        
                 // Scripting and Configuration Files
                { ".sh", new System.Numerics.Vector4(0.1f, 0.6f, 0.1f, 1.0f) }, // Dark Green
                { ".yaml", new System.Numerics.Vector4(0.6f, 0.4f, 0.2f, 1.0f) }, // Dark Orange
                { ".json", new System.Numerics.Vector4(0.7f, 0.7f, 0.4f, 1.0f) }, // Khaki
                { ".ini", new System.Numerics.Vector4(0.5f, 0.5f, 0.5f, 1.0f) }, // Grey

                // Data and Database Files
                { ".sql", new System.Numerics.Vector4(0.8f, 0.8f, 0.0f, 1.0f) }, // Olive Yellow
                { ".db", new System.Numerics.Vector4(0.6f, 0.6f, 0.3f, 1.0f) }, // Dark Khaki
                { ".sqlite", new System.Numerics.Vector4(0.6f, 0.3f, 0.3f, 1.0f) }, // Brownish

                // Web and Internet Files
                { ".html", new System.Numerics.Vector4(0.9f, 0.5f, 0.5f, 1.0f) }, // Soft Red
                { ".css", new System.Numerics.Vector4(0.5f, 0.5f, 0.9f, 1.0f) }, // Soft Blue
                { ".js", new System.Numerics.Vector4(0.9f, 0.9f, 0.2f, 1.0f) }, // Yellow
                { ".php", new System.Numerics.Vector4(0.6f, 0.4f, 0.8f, 1.0f) }, // Light Purple

                // Multimedia and Design
                { ".psd", new System.Numerics.Vector4(0.0f, 0.5f, 0.5f, 1.0f) }, // Dark Cyan
                { ".ai", new System.Numerics.Vector4(1.0f, 0.0f, 0.0f, 1.0f) }, // Bright Red
                { ".fla", new System.Numerics.Vector4(0.9f, 0.6f, 0.2f, 1.0f) }, // Orange-Yellow
                { ".svg", new System.Numerics.Vector4(0.1f, 0.7f, 0.7f, 1.0f) }, // Turquoise

                // Executable and Binary Files
                { ".dll", new System.Numerics.Vector4(0.8f, 0.3f, 0.8f, 1.0f) }, // Purple
                { ".so", new System.Numerics.Vector4(0.5f, 0.2f, 0.2f, 1.0f) }, // Dark Red
                { ".exe", new System.Numerics.Vector4(0.9f, 0.1f, 0.1f, 1.0f) }, // Bright Red
                { ".bin", new System.Numerics.Vector4(0.4f, 0.4f, 0.4f, 1.0f) }, // Dark Grey

                // Archive Files
                { ".zip", new System.Numerics.Vector4(0.5f, 0.5f, 0.0f, 1.0f) }, // Olive
                { ".rar", new System.Numerics.Vector4(0.5f, 0.5f, 0.0f, 1.0f) }, // Olive
                { ".7z", new System.Numerics.Vector4(0.5f, 0.5f, 0.0f, 1.0f) }, // Olive
                { ".tar", new System.Numerics.Vector4(0.4f, 0.3f, 0.2f, 1.0f) }, // Brown

                // Engineering and Design
                { ".cad", new System.Numerics.Vector4(0.5f, 0.3f, 0.7f, 1.0f) }, // Purple-ish
                { ".dwg", new System.Numerics.Vector4(0.3f, 0.5f, 0.7f, 1.0f) }, // Blue-ish
                { ".dxf", new System.Numerics.Vector4(0.4f, 0.4f, 0.7f, 1.0f) }, // Soft Blue
                { ".stl", new System.Numerics.Vector4(0.8f, 0.3f, 0.5f, 1.0f) }, // Pink-ish
        
                // Science and Data Analysis
                { ".pdb", new System.Numerics.Vector4(0.9f, 0.7f, 0.2f, 1.0f) }, // Gold
                { ".csv", new System.Numerics.Vector4(0.9f, 0.45f, 0.0f, 1.0f) }, // Dark Orange
                { ".nc", new System.Numerics.Vector4(0.2f, 0.5f, 0.8f, 1.0f) }, // Light Blue
                { ".dat", new System.Numerics.Vector4(0.7f, 0.7f, 0.7f, 1.0f) }, // Light Grey

                // Scripting and Markup Languages
                { ".lua", new System.Numerics.Vector4(0.1f, 0.7f, 0.1f, 1.0f) }, // Green
                { ".perl", new System.Numerics.Vector4(0.7f, 0.1f, 0.7f, 1.0f) }, // Purple
                { ".xml", new System.Numerics.Vector4(0.8f, 0.4f, 0.4f, 1.0f) }, // Soft Red
        
                // Virtual Machine and Container Files
                { ".vmdk", new System.Numerics.Vector4(0.6f, 0.4f, 0.6f, 1.0f) }, // Lavender
                { ".ova", new System.Numerics.Vector4(0.5f, 0.5f, 0.8f, 1.0f) }, // Periwinkle
                { ".dockerfile", new System.Numerics.Vector4(0.2f, 0.6f, 0.7f, 1.0f) }, // Turquoise
        
                // Miscellaneous
                { ".iso", new System.Numerics.Vector4(0.8f, 0.8f, 0.0f, 1.0f) }, // Yellow
                { ".torrent", new System.Numerics.Vector4(0.0f, 0.5f, 0.0f, 1.0f) }, // Dark Green
                { ".vbs", new System.Numerics.Vector4(0.5f, 0.0f, 0.5f, 1.0f) }, // Purple
        
                // Development and Build Files
                { ".makefile", new System.Numerics.Vector4(0.5f, 0.3f, 0.0f, 1.0f) }, // Brown
                { ".cmake", new System.Numerics.Vector4(0.3f, 0.3f, 0.5f, 1.0f) }, // Slate Blue
                { ".docker-compose.yml", new System.Numerics.Vector4(0.2f, 0.5f, 0.5f, 1.0f) }, // Dark Cyan
        
                // Project Management and Collaboration
                { ".ppt", new System.Numerics.Vector4(0.8f, 0.2f, 0.2f, 1.0f) }, // Dark Red
                { ".pptx", new System.Numerics.Vector4(0.8f, 0.2f, 0.2f, 1.0f) }, // Dark Red
                { ".xls", new System.Numerics.Vector4(0.2f, 0.8f, 0.2f, 1.0f) }, // Dark Green
                { ".xlsx", new System.Numerics.Vector4(0.2f, 0.8f, 0.2f, 1.0f) }, // Dark Green

           };
        }



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
            Pwd = Path.GetFullPath(Directory.GetCurrentDirectory());
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

        public void SetFolderColor(float r, float g, float b, float a)
        {
            folderColor = new Vector4(r, g, b, a);
        }

        public void SetFileColor(float r, float g, float b, float a)
        {
            fileColor = new Vector4(r, g, b, a);
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
            try
            {
                var fullPath = Path.GetFullPath(pwd);
                if (Directory.Exists(fullPath))
                {
                    Pwd = fullPath;
                    UpdateFileRecords();
                }
                else
                {
                    StatusStr = "Directory does not exist: " + pwd;
                }
            }
            catch (Exception e)
            {
                StatusStr = "Error setting directory: " + e.Message;
            }
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

        /*
        public void SetOkButtonLabel(string label)
        {
            //openLabel = label;
        }

        public void SetCancelButtonLabel(string label)
        {
            OpenNewDirLabel = label;
        }
        */
        public void Display()
        {
            ImGui.PushID(GetHashCode());
            if (OpenFlag)
            {
                ImGui.OpenPopup(OpenLabel);
            }

            IsOpened_ = false;
            CloseFlag = false;

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
                bool open = true;
                if (!ImGui.BeginPopupModal(OpenLabel, ref open, (Flags & ImGuiFileBrowserFlags.NoTitleBar) != 0 ? ImGuiWindowFlags.NoTitleBar : 0))
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

                    ImGui.PushID(rsc.Name);

                    var color = rsc.IsDir ? folderColor : fileColor;

                    if (!rsc.IsDir && FileTypeColorMap.ExtensionToColor.TryGetValue(rsc.Extension, out var extColor))
                    {
                        color = extColor;
                    }


                    ImGui.PushStyleColor(ImGuiCol.Text, color);

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

                    ImGui.PopStyleColor(); // Revert to the original text color
                    ImGui.PopID();
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
                CloseFlag = true;
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
                            UpdateFileRecords();
                        }
                    }

                    ImGui.EndCombo();
                }

                ImGui.PopItemWidth();
            }

            if (OpenFlag)
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

        public bool HasSelected()
        {
            return Ok;
        }

        public int GetSelectCount()
        {

            return SelectedFilenames.Count;
        }

        public List<string> GetSelected()
        {
            Ok = false;
            return SelectedFilenames.ToList();
        }

        public bool HasCancelled()
        {
            return CloseFlag;
        }

        private void UpdateFileRecords()
        {
            FileRecords = new List<FileRecord>();

            StatusStr = string.Empty;

            try
            {
                // Check if we're not at the root directory to add the ".." entry
                if (Directory.GetParent(Pwd) != null)
                {
                    FileRecords.Add(new FileRecord
                    {
                        IsDir = true, // It's a directory
                        Name = "..", // The name is ".." indicating to go up
                        ShowName = "..", // The displayed name is also ".."
                        Extension = "" // No extension for a directory
                    });
                }

                var directories = Directory.GetDirectories(Pwd);
                foreach (var dir in directories)
                {
                    FileRecords.Add(new FileRecord { IsDir = true, Name = Path.GetFileName(dir), ShowName = Path.GetFileName(dir) });
                }

                var files = Directory.GetFiles(Pwd);
                foreach (var file in files)
                {
                    var extension = Path.GetExtension(file).ToLowerInvariant(); // Normalize extension
                    if (!IsExtensionMatched(extension))
                    {
                        continue; // This line might be skipping files unintentionally. Check your conditions.
                    }
                    FileRecords.Add(new FileRecord { IsDir = false, Name = Path.GetFileName(file), ShowName = Path.GetFileName(file), Extension = extension });
                }
            }
            catch (Exception e)
            {
                StatusStr = "Error: " + e.Message;
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
            // Check if no filters are set, should not happen as there should at least be one filter.
            if (TypeFilters.Count == 0)
            {
                return true;
            }

            // If the special case "*.*" is selected, allow all files.
            string currentFilter = TypeFilters[(int)TypeFilterIndex];
            if (currentFilter == "*.*")
            {
                return true;
            }

            // Normalize the extension to ensure consistent comparison.
            extension = extension?.ToLowerInvariant();

            // Check if the current filter is a wildcard filter like "*.txt"
            if (currentFilter.StartsWith("*"))
            {
                var filterExt = currentFilter.Substring(1).ToLowerInvariant(); // Gets the extension part, e.g., ".txt" from "*.txt"
                if (!string.IsNullOrEmpty(extension) && extension.EndsWith(filterExt))
                {
                    return true;
                }
            }
            else if (currentFilter == extension)
            {
                // The current filter matches the file extension exactly.
                return true;
            }

            // If none of the above conditions are met, the file does not match the currently selected filter.
            return false;
        }


        // Simplify and ensure cross-platform drive selection
        private static uint GetDrivesBitMask()
        {
            uint drives = 0;
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && (drive.DriveType == DriveType.Fixed || drive.DriveType == DriveType.Removable))
                {
                    int index = drive.Name.ToUpperInvariant()[0] - 'A';
                    if (index >= 0 && index < 26) // Protect against out-of-range drive letters
                    {
                        drives |= 1u << index;
                    }
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

