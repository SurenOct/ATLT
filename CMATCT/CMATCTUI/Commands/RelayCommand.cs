using System;
using System.Windows.Input;

namespace CMATCTUI.Commands
{
    public class RelayCommand : ICommand
    {

        //private Action start;


        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute ?? (() => true);
        }



        public bool CanExecute(object parameter) => _canExecute();
        public void Execute(object parameter) => _execute();

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
