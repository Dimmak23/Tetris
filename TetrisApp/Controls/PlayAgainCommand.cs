//using System;
//using System.Windows.Controls;
//using System.Windows.Input;
//using System.Windows;

//namespace TetrisApp.Controls
//{
//    public class PlayAgainCommand : ICommand
//    {
//        public bool CanExecute(object parameter) => true;

//        public void Execute(object parameter)
//        {
//            var button = parameter as Button;
//            if (button != null)
//            {
//                button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
//            }
//        }

//        public event EventHandler CanExecuteChanged;
//    }
//}
