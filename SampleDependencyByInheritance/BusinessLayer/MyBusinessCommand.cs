using DataLayer;

namespace BusinessLayer
{
    public class MyBusinessCommand
    {
        private readonly Repository _repository;

        public MyBusinessCommand()
        {
            _repository = new Repository();
        }

        public void RunBusinessProcess()
        {
            _repository.Load();
        }
    }
}