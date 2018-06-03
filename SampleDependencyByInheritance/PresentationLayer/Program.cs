namespace PresentationLayer
{
    public static class Program
    {
        public static void Main()
        {
            // It's OK to use business layer from presentation layer
            // This can be allowed with the following configuration:
            // <Allowed From="PresentationLayer.*" To="BusinessLayer.*" />
            var cmd = new BusinessLayer.MyBusinessCommand();
            cmd.RunBusinessProcess();

            // This is forbidden, so i will not include the NsDepCop configuration....
            var repo = new DataLayer.Repository();
            repo.Load();

            // ...But i can work around NsDepCop by creating a "fake" class with the following specs:
            // Is defined in an "allowed" namespace
            // Inherit from the class that i want to use
            var hack = new BusinessLayer.HackToUseRepository();
            hack.Load();
        }
    }
}