namespace MenuSystem;

public class Menu
{
    private readonly string _title;
    private int _selectedIndex;
    private readonly EMenuLevel _level;
    private readonly List<MenuItem> _menuItems = new List<MenuItem>();
    
    private readonly MenuItem _menuItemExit = new MenuItem("Exit", null);
    private readonly MenuItem _menuItemGoBack = new MenuItem("Back", null);
    private readonly MenuItem _menuItemGoToMain = new MenuItem("Main menu", null);
    public Menu(string title, EMenuLevel level, List<MenuItem> menuItems)
    {
        _title = title;
        _selectedIndex = 0;
        _level = level;
        
        foreach (var menuItem in menuItems)
        {
            _menuItems.Add(menuItem);
        }

        if (_level != EMenuLevel.Main)
            _menuItems.Add(_menuItemGoBack);

        if (_level == EMenuLevel.Other)
            _menuItems.Add(_menuItemGoToMain);

        _menuItems.Add(_menuItemExit);
        
    }

    private void DisplayMenuItems()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(_title);
        Console.ResetColor();
        Console.WriteLine("===============");
        for (var i = 0; i < _menuItems.Count; i++)
        {
            var currentOption = _menuItems[i].Title;

            if (i == _selectedIndex)
            {
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.White;
                Console.WriteLine($">>{currentOption}<<");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.BackgroundColor = ConsoleColor.Black;
                Console.WriteLine($"  {currentOption}  ");
            }
        }
        Console.ResetColor();
        Console.WriteLine("===============");
    }

    public string RunMenu()
    {
        //Console.Clear();
        var menuDone = false;
        var runReturnValue = "";
        ConsoleKey keyPressed;
        do
        {
            Console.Clear();
            DisplayMenuItems();
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            keyPressed = keyInfo.Key;


            if (keyPressed == ConsoleKey.UpArrow)
            {
                if (_selectedIndex == 0)
                {
                    _selectedIndex = _menuItems.Count - 1;
                }
                else
                {
                    _selectedIndex--;
                }
            }

            else if (keyPressed == ConsoleKey.DownArrow)
            {
                if (_selectedIndex == _menuItems.Count - 1)
                {
                    _selectedIndex = 0;
                }
                else
                {
                    _selectedIndex++;
                }
            }

            else if (keyPressed == ConsoleKey.Enter)
            {
                if (_menuItems[_selectedIndex].MethodToRun != null)
                {
                    runReturnValue = _menuItems[_selectedIndex].MethodToRun!();
                }

                if (_menuItems[_selectedIndex].Title.Equals(_menuItemGoBack.Title))
                {
                    menuDone = true;
                }

                if (_menuItems[_selectedIndex].Title.Equals(_menuItemExit.Title) ||
                    runReturnValue.Equals(_menuItemExit.Title))
                {
                    menuDone = true;
                }

                if ((_menuItems[_selectedIndex].Title.Equals(_menuItemGoToMain.Title) ||
                    runReturnValue.Equals(_menuItemGoToMain.Title)) && _level != EMenuLevel.Main)
                {
                    menuDone = true;
                }
                

            }
            
        } while (menuDone == false);


        if (runReturnValue.Equals(_menuItemExit.Title) || runReturnValue.Equals(_menuItemGoToMain.Title))
        {
            return runReturnValue;
        }
        return _menuItems[_selectedIndex].Title;
    }
    
}
