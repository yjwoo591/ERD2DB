using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using CsvHelper;
using System.Globalization;
using System.Windows.Forms;
using FDCommon.LibBase.LibBase14LB;

namespace ERD2DB.Menu
{
    public class KryptonMenu34ME
    {
        private static KryptonMenu34ME _instance;
        private readonly Logging14LB _logger;
        private readonly string _menuFilePath;
        private List<MenuData34ME> _menuItems;
        private FileMenuHandler34ME _fileMenuHandler;
        private MenuStrip _menuStrip;
        private ToolStrip _toolStrip;
        private StatusStrip _statusStrip;
        private readonly KryptonManager14LB _kryptonManager;
        private Form _mainForm;

        public static KryptonMenu34ME Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new KryptonMenu34ME();
                }
                return _instance;
            }
        }

        private KryptonMenu34ME()
        {
            _logger = Logging14LB.Instance;
            _kryptonManager = KryptonManager14LB.Instance;
            _menuFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "menu.csv");
            _menuItems = new List<MenuData34ME>();
            InitializeComponents();
        }

        public void Initialize(Form mainForm)
        {
            try
            {
                _logger.LogMethodEntry(nameof(Initialize), nameof(KryptonMenu34ME));
                _mainForm = mainForm;
                _fileMenuHandler = new FileMenuHandler34ME(_mainForm);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to initialize menu: {ex.Message}", ex);
                throw;
            }
        }

        private void InitializeComponents()
        {
            try
            {
                _logger.LogMethodEntry(nameof(InitializeComponents), nameof(KryptonMenu34ME));
                _menuStrip = (MenuStrip)_kryptonManager.CreateMenuStrip();
                _toolStrip = (ToolStrip)_kryptonManager.CreateToolStrip();
                _statusStrip = (StatusStrip)_kryptonManager.CreateStatusStrip();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to initialize components: {ex.Message}", ex);
                throw;
            }
        }

        public MenuStrip CreateMainMenu()
        {
            _logger.LogMethodEntry(nameof(CreateMainMenu), nameof(KryptonMenu34ME));
            try
            {
                if (_mainForm == null)
                {
                    throw new InvalidOperationException("Main form not initialized");
                }

                LoadMenuData();
                BuildMainMenu();
                return _menuStrip;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to create main menu: {ex.Message}", ex);
                throw;
            }
        }

        private void LoadMenuData()
        {
            try
            {
                _logger.LogMethodEntry(nameof(LoadMenuData), nameof(KryptonMenu34ME));

                if (!File.Exists(_menuFilePath))
                {
                    _logger.LogError($"Menu configuration file not found: {_menuFilePath}");
                    throw new FileNotFoundException("Menu configuration file not found", _menuFilePath);
                }

                using (var reader = new StreamReader(_menuFilePath))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    _menuItems = csv.GetRecords<MenuData34ME>().ToList();
                }

                _logger.LogInformation($"Loaded {_menuItems.Count} menu items");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to load menu data: {ex.Message}", ex);
                throw;
            }
        }

        private void BuildMainMenu()
        {
            try
            {
                _logger.LogMethodEntry(nameof(BuildMainMenu), nameof(KryptonMenu34ME));

                _menuStrip.Items.Clear();

                var rootItems = _menuItems.Where(m => m.ParentID == "0")
                                        .OrderBy(m => m.Order);

                foreach (var menuData in rootItems)
                {
                    var menuItem = CreateMenuItem(menuData);
                    _menuStrip.Items.Add(menuItem);
                }

                _logger.LogInformation("Main menu built successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to build main menu: {ex.Message}", ex);
                throw;
            }
        }

        private ToolStripMenuItem CreateMenuItem(MenuData34ME menuData)
        {
            try
            {
                var menuItem = new ToolStripMenuItem(menuData.Text);
                menuItem.Tag = menuData;
                menuItem.Enabled = menuData.Enabled;

                // 단축키 설정
                if (!string.IsNullOrEmpty(menuData.Shortcut))
                {
                    try
                    {
                        string[] parts = menuData.Shortcut.Split('+');
                        Keys shortcutKey = Keys.None;

                        foreach (string part in parts)
                        {
                            if (Enum.TryParse<Keys>(part.Trim(), true, out Keys key))
                            {
                                shortcutKey |= key;
                            }
                        }

                        if (shortcutKey != Keys.None)
                        {
                            menuItem.ShortcutKeys = shortcutKey;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Failed to parse shortcut key '{menuData.Shortcut}': {ex.Message}");
                    }
                }

                // 하위 메뉴 추가
                AddChildMenuItems(menuItem, menuData.MenuID);

                // 이벤트 핸들러 연결
                if (!string.IsNullOrEmpty(menuData.Handler_Method))
                {
                    menuItem.Click += (sender, e) =>
                    {
                        InvokeMenuHandler(menuData.Handler_Method);
                    };
                }

                return menuItem;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to create menu item: {ex.Message}", ex);
                throw;
            }
        }

        private void AddChildMenuItems(ToolStripMenuItem parentItem, string parentId)
        {
            try
            {
                var childItems = _menuItems.Where(m => m.ParentID == parentId)
                                         .OrderBy(m => m.Order);

                if (childItems.Any())
                {
                    foreach (var childData in childItems)
                    {
                        var childItem = CreateMenuItem(childData);
                        parentItem.DropDownItems.Add(childItem);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to add child menu items: {ex.Message}", ex);
                throw;
            }
        }

        private void InvokeMenuHandler(string methodName)
        {
            try
            {
                _logger.LogMethodEntry(nameof(InvokeMenuHandler), nameof(KryptonMenu34ME));
                _logger.LogInformation($"Invoking menu handler: {methodName}");

                switch (methodName)
                {
                    case "HandleOpen":
                        _fileMenuHandler.HandleOpen();
                        break;
                    case "HandleNew":
                        _fileMenuHandler.HandleNew();
                        break;
                    case "HandleSave":
                        _fileMenuHandler.HandleSave();
                        break;
                    case "HandleSaveAs":
                        _fileMenuHandler.HandleSaveAs();
                        break;
                    case "HandleExit":
                        _fileMenuHandler.HandleExit();
                        break;
                    default:
                        _logger.LogWarning($"Unknown menu handler method: {methodName}");
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to invoke menu handler {methodName}: {ex.Message}", ex);
                MessageBox.Show($"메뉴 처리 중 오류가 발생했습니다: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public ToolStrip GetToolStrip()
        {
            return _toolStrip;
        }

        public StatusStrip GetStatusStrip()
        {
            return _statusStrip;
        }

        public void UpdateStatusText(string text)
        {
            try
            {
                if (_statusStrip.Items.Count == 0)
                {
                    _statusStrip.Items.Add(new ToolStripStatusLabel());
                }

                if (_statusStrip.Items[0] is ToolStripStatusLabel label)
                {
                    label.Text = text;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to update status text: {ex.Message}", ex);
            }
        }

        public void RefreshMenu()
        {
            try
            {
                _logger.LogMethodEntry(nameof(RefreshMenu), nameof(KryptonMenu34ME));
                LoadMenuData();
                BuildMainMenu();
                _logger.LogInformation("Menu refreshed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to refresh menu: {ex.Message}", ex);
                throw;
            }
        }
    }
}